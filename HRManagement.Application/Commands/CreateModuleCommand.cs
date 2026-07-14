using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Commands.ELearningCommands
{
    public class CreateModuleCommand(CreateModuleDto dto) : IRequest<Result<int>>
    {
        public CreateModuleDto Dto { get; set; } = dto;
    }

    internal class CreateModuleHandler : IRequestHandler<CreateModuleCommand, Result<int>>
    {
        private readonly ILogger<CreateModuleHandler> _logger;
        private readonly IELearningRepository _repo;

        public CreateModuleHandler(IELearningRepository repo, ILogger<CreateModuleHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(CreateModuleCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(CreateModuleHandler));
            try
            {
                var newModule = new ModuleModel
                {
                    BatchId = request.Dto.batchId,
                    ModuleTitle = request.Dto.title,
                    ModuleDescription = request.Dto.description,
                    TargetRole = request.Dto.role,
                    IsPriority = request.Dto.isPriority,
                    DueDate = request.Dto.dueDate,
                    CreatedBy = request.Dto.currentUserId.ToString(),
                    CreatedUtcDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                var moduleId = await _repo.CreateModuleAsync(newModule);
                if (moduleId <= 0) return Result.Failure<int>("Failed to create module");

                return Result.Success(moduleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating module: {message}", ex.Message);
                return Result.Failure<int>(ex.Message);
            }
        }
    }
}