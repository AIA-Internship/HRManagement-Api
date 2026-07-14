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
    public class GetGroupsQuery : IRequest<Result<ApiResponse>>
    {
    }

    internal class GetGroupsHandler : IRequestHandler<GetGroupsQuery, Result<ApiResponse>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetGroupsHandler> _logger;

        public GetGroupsHandler(IELearningRepository repo, ILogger<GetGroupsHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(GetGroupsQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetGroupsHandler));
            try
            {
                var groups = await _repo.GetAllGroupsAsync();
                var mapped = groups.Select(g => new ReadGroupDto { groupId = g.GroupId, groupName = g.GroupName });
                return ApiHelperResponse.Success("Groups retrieved successfully", mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching groups");
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
