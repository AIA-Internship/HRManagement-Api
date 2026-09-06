using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using HRManagement.Application.Interfaces;
using HRManagement.Application.Mappings;
using HRManagement.Application.Queries;
using HRManagement.Domain.SeedWork;
using HRManagement.MsSQL;
using HRManagement.Domain.Interfaces;
using HRManagement.MsSQL.Repositories;
using HRManagement.MsSQL.Authentications;
using HRManagement.MsSQL.Base;
using HRManagement.MsSQL.Services;
using HRManagement.Application.Auth.Permission;
using Microsoft.AspNetCore.Authorization;
using HRManagement.MsSQL.Timesheet;

namespace HRManagement.Extensions
{
    public static class DependencyContainer
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Database Setup
            var connectionString = configuration["AppSetting:DbConnectionString"] ?? throw new InvalidOperationException("Database Connection String is missing!");
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));
            
            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<AppDbContext>());

            // 2. Unit of Work & Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
            
            // Timesheet Module (Modular Repositories)
            services.AddScoped<ITimesheetProjectRepository, TimesheetProjectRepository>();
            services.AddScoped<ITimesheetEntryRepository, TimesheetEntryRepository>();
            services.AddScoped<ITimesheetSubmissionRepository, TimesheetSubmissionRepository>();
            services.AddScoped<ITodoTaskRepository, TodoTaskRepository>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
            services.AddHttpContextAccessor();

            // 3. Authorization
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            // 4. MediatR, AutoMapper & FluentValidation
            var applicationAssembly = typeof(GetMyProfileQuery).Assembly; 
            services.AddValidatorsFromAssembly(applicationAssembly);
            
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(applicationAssembly);
            });

            services.AddAutoMapper(cfg => 
            {
                cfg.AddMaps(typeof(EmployeeMappingProfile).Assembly);
            });
            return services;
        }
    }
}





