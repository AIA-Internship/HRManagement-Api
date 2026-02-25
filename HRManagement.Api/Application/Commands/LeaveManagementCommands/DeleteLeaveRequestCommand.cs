using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces.NewFolder;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;
using MediatR;

namespace HRManagement.Api.Application.Commands.LeaveManagementCommands
{
    public class DeleteLeaveRequestCommand : IRequest<Result<ApiResponse>>
    {
        public DeleteLeaveRequestDto LeaveRequestDto { get; set; }
        public DeleteLeaveRequestCommand(DeleteLeaveRequestDto dto)
        {
            LeaveRequestDto = dto;
        }
    }
    internal class DeleteLeaveRequestCommandHandler : IRequestHandler<DeleteLeaveRequestCommand, Result<ApiResponse>>
    {
        private readonly ILogger<DeleteLeaveRequestCommandHandler> _logger;
        private readonly ILeaveRequestRepository _repo;
        public DeleteLeaveRequestCommandHandler(
            ILeaveRequestRepository repo
            , ILogger<DeleteLeaveRequestCommandHandler> logger
        )
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(DeleteLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existing = await _repo.getLeaveRequestById(request.LeaveRequestDto.RequestId);

                if (existing == null)
                    return ApiHelperResponse.Failed("Leave request not found");

                existing.IsDeleted = 1;

                var result = await _repo.softDelete(existing.LeaveId);
                if (!result) return ApiHelperResponse.Failed("failed to delete leave request");

                return ApiHelperResponse.Success("Leave request deleted successfully");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return ApiHelperResponse.Failed("Failed to delete leave request");
            }


        }

    }
}