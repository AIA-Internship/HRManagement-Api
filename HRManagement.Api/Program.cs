using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Config;
using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Extensions;
using HRManagement.Api.Repositories.Base;
using HRManagement.Api.Repositories.Seeder;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

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

    // JWT EVENT HANDLERS (CRITICAL FOR AUTHENTICATION DEBUGGING)
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new ApiResponse
            {
                Title = "Error",
                StatusCode = StatusCodes.Status401Unauthorized,
                StatusMessage = "Authentication failed: " + context.Exception.Message,
                IsError = true
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return context.Response.WriteAsync(result);
        },
        OnChallenge = context =>
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new ApiResponse
                {
                    Title = "Error",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    StatusMessage = "Unauthorized. Access token is missing or invalid.",
                    IsError = true
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                return context.Response.WriteAsync(result);
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
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ==========================================
// 4. CORS Configuration (Allows Frontend to connect to the API)
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:7060", "https://localhost:7060")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

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
        var errors = actionContext.ModelState.Where(e => e.Value!.Errors.Count > 0)
            .Select(e => e.Value!.Errors.First().ErrorMessage).ToList();

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
// 6. OpenAPI & Scalar Setup
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info ??= new OpenApiInfo();
        document.Info.Title = apiName;
        document.Info.Version = "v1";

        document.Components ??= new OpenApiComponents();
        if (document.Components.SecuritySchemes == null)
        {
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();
        }

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        };

        if (document.Security == null)
        {
            document.Security = new List<OpenApiSecurityRequirement>();
        }

        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// MIDDLEWARE PIPELINE ORDER

// 1. Exception Handling
app.UseMiddleware<ExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Internal Server Error",
                message = ExceptionConstants.InternalServerError
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        });
    });
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle(apiName)
            .WithTheme(ScalarTheme.Mars)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .AddPreferredSecuritySchemes("Bearer")
            .HideModels()
            .ExpandAllTags();
    });
}

// 3. HTTPS Redirection
app.UseHttpsRedirection();

// 4. Routing
app.UseRouting();

// 4.1. CORS (Must be placed after Routing and before Auth)
app.UseCors("AllowAll");
// 5. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Endpoints
app.MapControllers();

// 6.1. Root Redirect to Swagger (Hidden from Swagger UI)
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
// 7. Database Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        context.Database.EnsureCreated();
        await DbSeeder.SeedAsync(context, passwordHasher);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}

app.Run();