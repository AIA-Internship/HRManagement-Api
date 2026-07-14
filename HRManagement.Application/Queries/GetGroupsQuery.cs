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
    public class GetGroupsQuery : IRequest<Result<List<ReadGroupDto>>>
    {
    }

    internal class GetGroupsHandler : IRequestHandler<GetGroupsQuery, Result<List<ReadGroupDto>>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetGroupsHandler> _logger;

        public GetGroupsHandler(IELearningRepository repo, ILogger<GetGroupsHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<List<ReadGroupDto>>> Handle(GetGroupsQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetGroupsHandler));
            try
            {
                var groups = await _repo.GetAllGroupsAsync();
                var mapped = groups.Select(g => new ReadGroupDto { groupId = g.GroupId, groupName = g.GroupName }).ToList();
                return Result.Success(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching groups");
                return Result.Failure<List<ReadGroupDto>>(ex.Message);
            }
        }
    }
}
