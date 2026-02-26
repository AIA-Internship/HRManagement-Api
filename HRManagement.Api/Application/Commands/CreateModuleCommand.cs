using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using HRManagement.Api.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application.Commands.ELearningCommands
{
    public class CreateModuleCommand(CreateModuleDto dto) : IRequest<Result<ApiResponse>>
    {
        public CreateModuleDto Dto { get; set; } = dto;
    }

    internal class CreateModuleHandler : IRequestHandler<CreateModuleCommand, Result<ApiResponse>>
    {
        private readonly ILogger<CreateModuleHandler> _logger;
        private readonly IELearningRepository _repo;

        public CreateModuleHandler(
            IELearningRepository repo,
            ILogger<CreateModuleHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(CreateModuleCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(CreateModuleHandler));

            try
            {
                var newModule = new ModuleModel
                {
                    ModuleTitle = request.Dto.title,
                    ModuleDescription = request.Dto.description,
                    TargetRole = request.Dto.role,
                    IsPriority = request.Dto.isPriority,
                    CreatedBy = request.Dto.CurrentUserId.ToString(),
                    CreatedUtcDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                var moduleId = await _repo.CreateModuleAsync(newModule);

                if (moduleId <= 0)
                {
                    return ApiHelperResponse.Failed("Failed to create module in system");
                }

                return ApiHelperResponse.Success("Module created successfully", new { id = moduleId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating module: {message}", ex.Message);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}