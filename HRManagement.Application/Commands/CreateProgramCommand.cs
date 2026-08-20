using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Commands.ELearningCommands
{
    public class CreateProgramCommand(CreateProgramDto dto) : IRequest<Result<int>>
    {
        public CreateProgramDto Dto { get; set; } = dto;
    }

    internal class CreateProgramHandler : IRequestHandler<CreateProgramCommand, Result<int>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<CreateProgramHandler> _logger;

        public CreateProgramHandler(IELearningRepository repo, ILogger<CreateProgramHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(CreateProgramCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(CreateProgramHandler));
            try
            {
                if (request.Dto.groupIds == null || request.Dto.groupIds.Count == 0)
                    return Result.Failure<int>("At least one group must be selected.");

                int firstProgramId = 0;

                foreach (var groupId in request.Dto.groupIds)
                {
                    var groupExists = await _repo.GroupExistsAsync(groupId);
                    if (!groupExists)
                        return Result.Failure<int>($"Group with ID {groupId} not found.");

                    var newProgram = new ProgramModel
                    {
                        ProgramName = request.Dto.programName,
                        GroupId = groupId
                    };

                    var programId = await _repo.CreateProgramAsync(newProgram);
                    if (firstProgramId == 0) firstProgramId = programId;
                }

                return Result.Success(firstProgramId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating program");
                return Result.Failure<int>(ex.Message);
            }
        }
    }
}
