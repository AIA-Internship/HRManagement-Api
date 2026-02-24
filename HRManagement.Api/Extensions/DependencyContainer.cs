using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Interfaces.NewFolder;
using HRManagement.Api.Domain.SeedWork;
using HRManagement.Api.Repositories;
using HRManagement.Api.Repositories.Base;
using HRManagement.Api.Repositories.LeaveManagementRepositories;
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

            services.AddScoped(typeof(IAuthorizationRepository), typeof(AuthorizationRepository));

            services.AddScoped<JwtTokenHandler>();

            services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();

            return services;

        }
    }
}
