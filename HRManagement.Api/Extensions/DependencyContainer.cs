using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.SeedWork;
using HRManagement.Api.Repositories;
using HRManagement.Api.Repositories.Authentications;
using HRManagement.Api.Repositories.Base;
using HRManagement.Api.Repositories.LeaveManagementRepositories;
using HRManagement.Api.Repositories.Services;

namespace HRManagement.Api.Extensions
{
    public static class DependencyContainer
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Database Setup
            var connectionString = configuration["AppSetting:DbConnectionString"] ?? throw new InvalidOperationException("Database Connection String is missing!");
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
            
            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<AppDbContext>());

            // 2. Unit of Work & Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
            services.AddHttpContextAccessor();

            // 3. MediatR, AutoMapper & FluentValidation
            var applicationAssembly = typeof(LoginQuery).Assembly; 
            services.AddValidatorsFromAssembly(applicationAssembly);
            
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(applicationAssembly);
            });

            services.AddAutoMapper(typeof(EmployeeMappingProfile).Assembly);

            services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();

            return services;
        }
    }
}
