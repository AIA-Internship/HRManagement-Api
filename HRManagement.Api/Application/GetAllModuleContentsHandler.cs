using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Queries;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningMapping;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Api.Application
{
    internal class GetAllModuleContentsHandler : IRequestHandler<GetAllModuleContentsQuery, Result<ApiResponse>>
    {
        private readonly ILogger<GetAllModuleContentsHandler> _logger;
        private readonly IELearningRepository _repo;

        public GetAllModuleContentsHandler(
            IELearningRepository repo,
            ILogger<GetAllModuleContentsHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(
            GetAllModuleContentsQuery request,
            CancellationToken ct)
        {
            _logger.LogTrace("Executing: {handler}",
                nameof(GetAllModuleContentsHandler));

            try
            {
                var contents = await _repo.GetContentsByModuleIdAsync(request.ModuleId);

                var result = contents
                    .Select(c => ModuleContentMapping.MapToReadDto(c))
                    .ToList();

                return ApiHelperResponse.Success(
                    "Module contents retrieved successfully",
                    result
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting module contents: {msg}", ex.Message);

                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}