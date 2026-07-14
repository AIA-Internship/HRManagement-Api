using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Commands.ELearningCommands
{
    public class CreateBatchCommand(CreateBatchDto dto) : IRequest<Result<int>>
    {
        public CreateBatchDto Dto { get; set; } = dto;
    }

    internal class CreateBatchHandler : IRequestHandler<CreateBatchCommand, Result<int>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<CreateBatchHandler> _logger;

        public CreateBatchHandler(IELearningRepository repo, ILogger<CreateBatchHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(CreateBatchCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(CreateBatchHandler));
            try
            {
                if (request.Dto.startDate >= request.Dto.endDate)
                    return Result.Failure<int>("Batch start date must be before its end date.");

                var programExists = await _repo.ProgramExistsAsync(request.Dto.programId);
                if (!programExists)
                    return Result.Failure<int>("Program not found.");

                var existingBatches = await _repo.GetBatchesByProgramIdAsync(request.Dto.programId);
                bool overlaps = existingBatches.Any(b =>
                    request.Dto.startDate < b.EndDate && b.StartDate < request.Dto.endDate);

                if (overlaps)
                    return Result.Failure<int>("Batch dates overlap with an existing batch under this program.");

                var newBatch = new BatchModel
                {
                    ProgramId = request.Dto.programId,
                    BatchName = $"Batch {existingBatches.Count() + 1}",
                    StartDate = request.Dto.startDate,
                    EndDate = request.Dto.endDate
                };

                var batchId = await _repo.CreateBatchAsync(newBatch);
                return Result.Success(batchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating batch");
                return Result.Failure<int>(ex.Message);
            }
        }
    }
}
