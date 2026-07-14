using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningMapping;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application.Queries
{
    public class GetModulesByBatchQuery(int batchId, string search, List<string> roles) : IRequest<Result<ApiResponse>>
    {
        public int BatchId { get; set; } = batchId;
        public string Search { get; set; } = search;
        public List<string> Roles { get; set; } = roles ?? new List<string>();
    }

    internal class GetModulesByBatchHandler : IRequestHandler<GetModulesByBatchQuery, Result<ApiResponse>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetModulesByBatchHandler> _logger;

        public GetModulesByBatchHandler(IELearningRepository repo, ILogger<GetModulesByBatchHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(GetModulesByBatchQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetModulesByBatchHandler));
            try
            {
                var modules = await _repo.GetModulesByBatchIdAsync(request.BatchId, request.Search, request.Roles);
                var mapped = modules.Select(ModuleMapping.MapToReadDto).ToList();
                return ApiHelperResponse.Success("Modules retrieved successfully", mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching modules for batch {batchId}", request.BatchId);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
