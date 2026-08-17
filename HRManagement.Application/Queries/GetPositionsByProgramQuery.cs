using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Queries
{
    public class GetPositionsByProgramQuery(int programId) : IRequest<Result<IEnumerable<string>>>
    {
        public int ProgramId { get; set; } = programId;
    }

    internal class GetPositionsByProgramHandler : IRequestHandler<GetPositionsByProgramQuery, Result<IEnumerable<string>>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetPositionsByProgramHandler> _logger;

        public GetPositionsByProgramHandler(IELearningRepository repo, ILogger<GetPositionsByProgramHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<string>>> Handle(GetPositionsByProgramQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetPositionsByProgramHandler));
            try
            {
                var positions = await _repo.GetDistinctPositionsByProgramIdAsync(request.ProgramId);
                return Result.Success(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching positions for program {programId}", request.ProgramId);
                return Result.Failure<IEnumerable<string>>(ex.Message);
            }
        }
    }
}
