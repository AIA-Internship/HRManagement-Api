using HRManagement.Api.Domain.Models.Config;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Extensions;
using HRManagement.Api.Repositories.Base;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Infrastructure.Repositories;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Net;
using System.Text;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);
var apiName = "HR Management API";


var _appsetting = builder.Configuration.GetSection("AppSetting");
builder.Services.Configure<AppSetting>(_appsetting);

ConfigurationManager configuration = builder.Configuration;
var setting = _appsetting.Get<AppSetting>()!; ;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    if (string.IsNullOrEmpty(setting.Jwt.Key))
        throw new ArgumentNullException("JWT key is not configured.");

    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.Jwt.Key)),

        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,


        ClockSkew = TimeSpan.Zero,

        ValidIssuer = setting.Jwt.Issuer,
        ValidAudiences = new[] { setting.Jwt.Audience1, setting.Jwt.Audience2, setting.Jwt.Audience3, setting.Jwt.Audience4 },
    };

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
                return context.Response.WriteAsync("Token Missing or Invalid (No Exception details)");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<SqlDbContext>(options =>
    options.UseSqlServer(setting.DBConnectionString, providerOptions => providerOptions.EnableRetryOnFailure()));

builder.Services.AddMemoryCache();
builder.Services.RegisterServices(configuration);
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehaviour<,>));
builder.Services.AddScoped<IELearningRepository, ELearningRepository>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = apiName, Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
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

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHsts();
else
    app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DefaultModelsExpandDepth(-1);
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
