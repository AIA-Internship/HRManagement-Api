using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Config;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Extensions;
using HRManagement.Api.Repositories.Base;
using HRManagement.Api.Repositories.Seeder;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);
var apiName = "Mini Project HR Management API";

// ==========================================
// 1. Config Setup
// ==========================================
var appSettingSection = builder.Configuration.GetSection("AppSetting");
builder.Services.Configure<AppSetting>(appSettingSection);

var appSetting = appSettingSection.Get<AppSetting>() ?? throw new InvalidOperationException("AppSetting section is missing.");
var jwtSettings = appSetting.Jwt ?? throw new InvalidOperationException("Jwt settings are missing.");
var jwtKey = jwtSettings.Key ?? throw new InvalidOperationException("JWT Key is missing.");
var jwtIssuer = jwtSettings.Issuer ?? throw new InvalidOperationException("JWT Issuer is missing.");
var validAudiences = new[]
{
    jwtSettings.AudienceWeb,
    jwtSettings.Audience1,
    jwtSettings.Audience2,
    jwtSettings.Audience3,
    jwtSettings.Audience4
}.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();


// ==========================================
// 2. JWT Configuration
// ==========================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true, 
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2), 
        ValidIssuer = jwtIssuer,
        ValidAudiences = validAudiences,
    };

    // DEBUGGING EVENT (PENTING UNTUK MELIHAT ERROR DI SWAGGER)
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "text/plain";
            return context.Response.WriteAsync("DEBUG ERROR: " + context.Exception.Message);
        },
        OnChallenge = context =>
        {
            if (!context.Response.HasStarted && context.AuthenticateFailure == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsync("Token Missing or Invalid");
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();


// ==========================================
// 3. Database & Services
// ==========================================
builder.Services.RegisterServices(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// ==========================================
// 5. Controllers, JSON & Validation Response
// ==========================================
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState.Where(e => e.Value.Errors.Count > 0)
            .Select(e => e.Value.Errors.First().ErrorMessage).ToList();
            
        return new BadRequestObjectResult(new ApiResponse()
        {
            Title = "Error",
            StatusCode = (int)HttpStatusCode.BadRequest,
            StatusMessage = "Error Validation Input",
            IsError = true,
            Content = errors
        });
    };
});


// ==========================================
// 6. Swagger Setup
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = apiName, Version = "v1" });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ==========================================
// URUTAN MIDDLEWARE YANG BENAR (PIPELINE)
// ==========================================

// 1. Exception Handling
app.UseMiddleware<ExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 2. Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DefaultModelsExpandDepth(-1);
    });
}

// 3. HTTPS Redirection
app.UseHttpsRedirection();

// 4. Routing
app.UseRouting();

// 5. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Endpoints
app.MapControllers();

// 7. Database Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        context.Database.Migrate();
        await DbSeeder.SeedAsync(context, passwordHasher);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}

app.Run();
