using HRManagement.Api.Extensions;
using HRManagement.Domain.Models.Config;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.MsSQL.Base;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using Scalar.AspNetCore;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
var apiName = "HR Management API";

// 1. Config Setup
var _appsetting = builder.Configuration.GetSection("AppSetting");
builder.Services.Configure<AppSetting>(_appsetting);

ConfigurationManager configuration = builder.Configuration;
var setting = _appsetting.Get<AppSetting>()!; ;

// --- 1. DAFTARKAN EXCEPTION HANDLER ---
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); // Diperlukan oleh arsitektur internal .NET

// 2. JWT Configuration
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

    // SETUP VALIDASI
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero, // Toleransi waktu server
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.Jwt.Key)),
        ValidIssuer = setting.Jwt.Issuer,
        // Audience tidak perlu dicek karena ValidateAudience = false
        ValidAudiences = new[] { setting.Jwt.Audience1, setting.Jwt.Audience2, setting.Jwt.Audience3, setting.Jwt.Audience4 },
    };

    // DEBUGGING EVENT (PENTING UNTUK MELIHAT ERROR DI SWAGGER)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = async context =>
        {
            // Jangan kembalikan teks mentah. Kembalikan ApiResponse JSON
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(
                message: $"Sesi tidak valid: {context.Exception.Message}",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication Failed"
            );

            await context.Response.WriteAsJsonAsync(response);
        },
        OnChallenge = async context =>
        {
            // Mencegah .NET melanjutkan proses challenge default yang merusak JSON kita
            context.HandleResponse();

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.Fail(
                    message: "Token tidak ditemukan atau tidak valid.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                );

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    };
});

builder.Services.AddAuthorization();

// 3. Database & Services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(setting.DbConnectionString, providerOptions => providerOptions.EnableRetryOnFailure()));

builder.Services.AddMemoryCache();
builder.Services.RegisterServices(configuration);
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();

        // 1. Gunakan Interface IOpenApiSecurityScheme (Perubahan baru di .NET 10)
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            In = ParameterLocation.Header,
            BearerFormat = "JWT",
            Description = "Masukkan token JWT saja."
        };

        // 2. Inisialisasi list Security global
        document.Security ??= [];

        // 3. 🔥 KUNCI UTAMANYA DI SINI 🔥
        // Gunakan class baru: OpenApiSecuritySchemeReference
        // Class ini butuh 2 parameter: ID string ("Bearer") dan object document-nya
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

        return Task.CompletedTask;
    });
});

// 4. Controllers & JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// 5. Validation Response
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
            .Where(e => e.Value!.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var errorMsg = string.Join("; ", errors);

        // Menggunakan method Fail dari ApiResponse<T>
        var response = ApiResponse<object>.Fail(
            message: errorMsg,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Validation Error"
        );

        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

//// 1. Konfigurasi Nama Aplikasi (Sebagai pengganti Cloud Role Name)
//builder.Services.AddOpenTelemetry()
//    .ConfigureResource(resource =>
//    {
//        resource.AddService("PrimaDental-Management-API");
//    });

//// 2. Tambahkan Application Insights (Versi 3.x akan otomatis menyerap konfigurasi di atas)
//builder.Services.AddApplicationInsightsTelemetry(options =>
//{
//    // Isi dengan string kosong atau ambil dari setting custom Anda
//    options.ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000;";
//});

var app = builder.Build();

// --- 2. AKTIFKAN MIDDLEWARE DI PIPELINE ---
// A. Tangani Exception (Harus diletakkan paling atas/awal)
app.UseExceptionHandler();

// B. Tangani Intercept Status 401 & 403 (Pengganti blok IF di middleware kamu yang lama)
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    var statusCode = response.StatusCode;

    if (statusCode == StatusCodes.Status401Unauthorized || statusCode == StatusCodes.Status403Forbidden)
    {
        response.ContentType = "application/json";

        var message = statusCode == StatusCodes.Status401Unauthorized
            ? "Sesi Anda tidak valid atau belum login."
            : "Anda tidak memiliki hak akses (Role) untuk tindakan ini.";

        // Menggunakan method Fail dari ApiResponse<T>
        var errorResponse = ApiResponse<object>.Fail(
            message: message,
            statusCode: statusCode,
            title: statusCode == StatusCodes.Status401Unauthorized ? "Unauthorized" : "Forbidden"
        );

        await response.WriteAsJsonAsync(errorResponse);
    }
});

//app.UseMiddleware<ExceptionHandlingMiddleware>();

// Aktifkan Endpoint OpenAPI (/openapi/v1.json)
app.MapOpenApi();

// Aktifkan Scalar UI di /scalar/v1
app.MapScalarApiReference(options =>
{
    options
        .WithTitle(apiName)
        .WithTheme(ScalarTheme.BluePlanet)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

// 1. Exception Handling & HSTS
if (!app.Environment.IsDevelopment())
    app.UseHsts();
//else
//{
//    app.UseDeveloperExceptionPage();
//}

// 3. HTTPS Redirection
app.UseHttpsRedirection();

// 4. Routing
app.UseRouting();

app.UseCors("AllowAll");

// 6. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 7. Endpoints
app.MapControllers();

app.Run();