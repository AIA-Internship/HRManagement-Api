using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Queries
{
    public class GetAllModuleContentsQuery : IRequest<Result<ApiResponse>>
    {
        public int ModuleId { get; set; }

        public GetAllModuleContentsQuery(int moduleId)
        {
            ModuleId = moduleId;
        }
    }
}