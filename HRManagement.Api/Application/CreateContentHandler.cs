using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Commands;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningMapping;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Api.Application
{
    internal class CreateContentHandler
        : IRequestHandler<CreateContentCommand, Result<ApiResponse>>
    {
        private readonly ILogger<CreateContentHandler> _logger;
        private readonly IELearningRepository _repo;

        public CreateContentHandler(
            IELearningRepository repo,
            ILogger<CreateContentHandler> logger )
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(
            CreateContentCommand request,
            CancellationToken ct)
        {
            _logger.LogTrace("Executing: {handler}", nameof(CreateContentHandler));

            try
            {
                var contentModel = ModuleContentMapping.MapToModel(
                    request.Dto,
                    request.UserId
                );

                var contentId = await _repo.AddContentAsync(contentModel);

                if (contentId <= 0)
                {
                    return ApiHelperResponse.Failed("Failed to create content");
                }

                return ApiHelperResponse.Success(
                    "Content created successfully",
                    new { id = contentId }
                );
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating content: {msg}", ex.Message);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}