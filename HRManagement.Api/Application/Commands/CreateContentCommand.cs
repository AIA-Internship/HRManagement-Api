using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;
using MediatR;

namespace HRManagement.Api.Application.Commands
{
    public class CreateContentCommand : IRequest<Result<ApiResponse>>
    {
        public CreateModuleContentDto Dto { get; set; }

        public long UserId { get; set; }

        public CreateContentCommand(CreateModuleContentDto dto, long userId)
        {
            Dto = dto;
            UserId = userId;
        }
    }
}