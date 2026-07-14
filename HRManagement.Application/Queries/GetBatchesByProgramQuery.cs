using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application.Queries
{
    public class GetBatchesByProgramQuery(int programId) : IRequest<Result<ApiResponse>>
    {
        public int ProgramId { get; set; } = programId;
    }

    internal class GetBatchesByProgramHandler : IRequestHandler<GetBatchesByProgramQuery, Result<ApiResponse>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetBatchesByProgramHandler> _logger;

        public GetBatchesByProgramHandler(IELearningRepository repo, ILogger<GetBatchesByProgramHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(GetBatchesByProgramQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetBatchesByProgramHandler));
            try
            {
                var programExists = await _repo.ProgramExistsAsync(request.ProgramId);
                if (!programExists)
                    return ApiHelperResponse.Failed("Program not found.");

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

                return ApiHelperResponse.Success("Batches retrieved successfully", mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching batches for program {programId}", request.ProgramId);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
