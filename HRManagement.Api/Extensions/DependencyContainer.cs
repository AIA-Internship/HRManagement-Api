<<<<<<< HEAD
using FluentValidation;
=======
﻿using FluentValidation;
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
using MediatR;
using Microsoft.EntityFrameworkCore;

using HRManagement.Api.Application.Interfaces;
<<<<<<< HEAD
using HRManagement.Api.Application.Mappings;
=======
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
using HRManagement.Api.Application.Queries;
using HRManagement.Api.Domain.SeedWork;
using HRManagement.Api.Repositories;
using HRManagement.Api.Repositories.Authentications;
using HRManagement.Api.Repositories.Base;
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
<<<<<<< HEAD
            services.AddScoped<ITimesheetRepository, TimesheetRepository>();
=======
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
            services.AddHttpContextAccessor();

<<<<<<< HEAD
            // 3. MediatR, AutoMapper & FluentValidation
=======
            // 3. MediatR & FluentValidation
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
            var applicationAssembly = typeof(LoginQuery).Assembly; 
            services.AddValidatorsFromAssembly(applicationAssembly);
            
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(applicationAssembly);
            });

<<<<<<< HEAD
            services.AddAutoMapper(cfg => 
            {
                cfg.AddMaps(typeof(EmployeeMappingProfile).Assembly);
            });

=======
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
            return services;
        }
    }
}
