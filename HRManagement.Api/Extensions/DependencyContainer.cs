using HRManagement.Api.Repositories.Authentications;
using HRManagement.Api.Repositories.Services;
using HRManagement.Application.Auth.Permissions;
using HRManagement.Application.Behaviors;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.SeedWork;
using HRManagement.MsSQL.Base;
using HRManagement.MsSQL.Repositories;

using MediatR;

using Microsoft.AspNetCore.Authorization;

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

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            
            services.AddHttpContextAccessor();

            // 3. Authorization
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddScoped<JwtTokenHandler>();

            return services;
        }
    }
}
