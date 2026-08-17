using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Queries
{
    public class GetBatchesByProgramQuery(int programId) : IRequest<Result<List<ReadBatchDto>>>
    {
        public int ProgramId { get; set; } = programId;
    }

    internal class GetBatchesByProgramHandler : IRequestHandler<GetBatchesByProgramQuery, Result<List<ReadBatchDto>>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetBatchesByProgramHandler> _logger;

        public GetBatchesByProgramHandler(IELearningRepository repo, ILogger<GetBatchesByProgramHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<List<ReadBatchDto>>> Handle(GetBatchesByProgramQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetBatchesByProgramHandler));
            try
            {
                if (request.ProgramId != 0)
                {
                    var programExists = await _repo.ProgramExistsAsync(request.ProgramId);
                    if (!programExists)
                        return Result.Failure<List<ReadBatchDto>>("Program not found.");
                }

                var batches = await _repo.GetBatchesByProgramIdAsync(request.ProgramId);
                var mapped = batches
                    .OrderBy(b => b.StartDate)
                    .Select(b => new ReadBatchDto
                    {
                        batchId = b.BatchId,
                        programId = b.ProgramId,
                        batchName = b.BatchName,
                        startDate = b.StartDate,
                        endDate = b.EndDate
                    }).ToList();

                return Result.Success(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching batches for program {programId}", request.ProgramId);
                return Result.Failure<List<ReadBatchDto>>(ex.Message);
            }
        }
    }
}
