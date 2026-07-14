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
    public class GetProgramsQuery : IRequest<Result<List<ReadProgramDto>>>
    {
    }

    internal class GetProgramsHandler : IRequestHandler<GetProgramsQuery, Result<List<ReadProgramDto>>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetProgramsHandler> _logger;

        public GetProgramsHandler(IELearningRepository repo, ILogger<GetProgramsHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<List<ReadProgramDto>>> Handle(GetProgramsQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetProgramsHandler));
            try
            {
                var programs = await _repo.GetAllProgramsAsync();
                var mapped = programs.Select(p => new ReadProgramDto
                {
                    programId = p.ProgramId,
                    programName = p.ProgramName,
                    groupId = p.GroupId
                }).ToList();

                return Result.Success(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching programs");
                return Result.Failure<List<ReadProgramDto>>(ex.Message);
            }
        }
    }
}
