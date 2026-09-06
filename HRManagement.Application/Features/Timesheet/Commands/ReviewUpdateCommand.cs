using HRManagement.Domain.Interfaces;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Payload.EmployeeDtos.Commands.Dto;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Commands;

public class ReviewUpdateCommand(ReviewUpdateRequestDto decision) : IRequest<ApiResponse<string>>
{
    public ReviewUpdateRequestDto Decision { get; } = decision;

    public class Handler(IRequestRepository requestRepository, IApplicationDbContext appDbContext)
        : IRequestHandler<ReviewUpdateCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(ReviewUpdateCommand command, CancellationToken ct)
        {
            var updateRequest = await requestRepository.GetByIdWithEmployeeAsync(command.Decision.RequestId, ct);
            if (updateRequest == null)
            {
                return ApiHelperResponse.Failed<string>("Request not found.");
            }

            if (command.Decision.IsApproved)
            {
                updateRequest.Approve(command.Decision.Reason, 0);
            }
            else
            {
                updateRequest.Reject(command.Decision.Reason, 0);
            }

            await appDbContext.SaveChangesAsync(ct);
            return ApiHelperResponse.Success<string>("Review processed successfully.", "Success");
        }
    }
}
