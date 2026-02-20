using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.SeedWork;
using HRManagement.Api.Repositories;
using HRManagement.Api.Repositories.Base;

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

            return services;

        }
    }
}
