using HRManagement.Application.Auth.Permissions;
using HRManagement.Application.Behaviors;
using HRManagement.Application.Services;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.SeedWork;
using HRManagement.MsSQL.Base;
using HRManagement.MsSQL.Repositories;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Azure;

using System.Diagnostics.Contracts;

namespace HRManagement.Api.Extensions
{
    public static class DependencyContainer
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            Contract.Assert(configuration != null);

            var applicationAssembly = typeof(Application.AssemblyReference).Assembly;

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(applicationAssembly);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Configure<AzureStorageOptions>(configuration.GetSection(AzureStorageOptions.SectionName));

            services.AddAzureClients(clientBuilder =>
            {
                var connectionString = configuration.GetValue<string>("AppSetting:AzureStorage:ConnectionString");
                clientBuilder.AddBlobServiceClient(connectionString);
            });

            // 3. Daftarkan FileStorageService milikmu
            services.AddScoped<IFileStorageService, FileStorageService>();

            services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAssessmentQuestionRepository, AssessmentQuestionRepository>();
            services.AddScoped<IPerformanceReviewPlanRepository, PerformanceReviewPlanRepository>();
            services.AddScoped<IPlanScoreWeightRepository, PlanScoreWeightRepository>();

            // 3. Authorization
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddScoped<JwtTokenHandler>();

            return services;
        }
    }
}
