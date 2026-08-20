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
                var batch = await _repo.GetBatchByIdAsync(request.Dto.batchId);
                if (batch == null)
                    return Result.Failure<int>("Batch not found.");

                var today = DateTime.Today;
                var daysUntilStart = (batch.StartDate.Date - today).TotalDays;
                if (daysUntilStart < 7)
                    return Result.Failure<int>("Modules can only be added up to 7 days before the batch starts.");

                if (request.Dto.dueDate.HasValue)
                {
                    var due = request.Dto.dueDate.Value.Date;
                    if (due < batch.StartDate.Date || due > batch.EndDate.Date)
                        return Result.Failure<int>("Due date must be within the batch start and end date.");
                }

                var newModule = new ModuleModel
                {
                    BatchId = request.Dto.batchId,
                    ModuleTitle = request.Dto.title,
                    ModuleDescription = request.Dto.description,
                    TargetRole = request.Dto.role,
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