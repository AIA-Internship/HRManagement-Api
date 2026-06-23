using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Application.Features.Leave.Commands
{
    public class UpdateLeaveBalanceCommand: IRequest<Result<ApiResponse>>
    {
        public UpdateLeaveBalanceCommand()
        {

        }


        internal class UpdateLeaveBalanceCommandHandler : IRequestHandler<UpdateLeaveBalanceCommand, Result<ApiResponse>>
        {
            private readonly ILogger<UpdateLeaveBalanceCommandHandler> _logger;
            private readonly ILeaveRepository _repo;
            public UpdateLeaveBalanceCommandHandler(
                ILeaveRepository repo
                , ILogger<UpdateLeaveBalanceCommandHandler> logger
            )
            {
                _repo = repo;
                _logger = logger;
            }

            public async Task<Result<ApiResponse>> Handle(UpdateLeaveBalanceCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var updated = await _repo.incrementAllEmployeeLeaveRequest();
                    if (updated == false) return ApiHelperResponse.Failed("update balance failed");

                    return ApiHelperResponse.Success();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return ApiHelperResponse.Failed("failed to update request");
                }
            }


            
        }
    }
}
