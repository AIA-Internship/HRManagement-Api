using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application.Commands.ELearningCommands
{
    public class CreateProgramCommand(CreateProgramDto dto) : IRequest<Result<ApiResponse>>
    {
        public CreateProgramDto Dto { get; set; } = dto;
    }

    internal class CreateProgramHandler : IRequestHandler<CreateProgramCommand, Result<ApiResponse>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<CreateProgramHandler> _logger;

        public CreateProgramHandler(IELearningRepository repo, ILogger<CreateProgramHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(CreateProgramCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(CreateProgramHandler));
            try
            {
                var groupExists = await _repo.GroupExistsAsync(request.Dto.groupId);
                if (!groupExists)
                    return ApiHelperResponse.Failed("Group not found.");

                var newProgram = new ProgramModel
                {
                    ProgramName = request.Dto.programName,
                    GroupId = request.Dto.groupId
                };

                var programId = await _repo.CreateProgramAsync(newProgram);
                return ApiHelperResponse.Success("Program created successfully", new { id = programId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating program");
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
