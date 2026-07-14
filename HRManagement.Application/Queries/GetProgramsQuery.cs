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
    public class GetProgramsQuery : IRequest<Result<ApiResponse>>
    {
    }

    internal class GetProgramsHandler : IRequestHandler<GetProgramsQuery, Result<ApiResponse>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetProgramsHandler> _logger;

        public GetProgramsHandler(IELearningRepository repo, ILogger<GetProgramsHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(GetProgramsQuery request, CancellationToken ct)
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

                return ApiHelperResponse.Success("Programs retrieved successfully", mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching programs");
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
