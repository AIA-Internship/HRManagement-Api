using CSharpFunctionalExtensions;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using MimeKit;

namespace HRManagement.Api.Application.Commands.LeaveManagementCommands
{
    public class RejectedLeaveRequestCommand : IRequest<Result<ApiResponse>>
    {
        public int LeaveId { get; set; }

        public RejectedLeaveRequestCommand(int id)
        {
            LeaveId = id;
        }
    }


    internal class RejectedLeaveRequestCommandHandler : IRequestHandler<RejectedLeaveRequestCommand, Result<ApiResponse>>
    {
        private readonly ILogger<RejectedLeaveRequestCommandHandler> _logger;
        private readonly ILeaveRepository _repo;
        private readonly IEmployeeRepository _employeeRepository;

        public RejectedLeaveRequestCommandHandler(
            ILeaveRepository repo
            ,ILogger<RejectedLeaveRequestCommandHandler> logger
            ,IEmployeeRepository employeeRepository
        )
        {
            _repo = repo;
            _logger = logger;
            _employeeRepository = employeeRepository;
        }

        public async Task<Result<ApiResponse>> Handle(RejectedLeaveRequestCommand request, CancellationToken cancellationToken)
        {

            LeaveRequestModel leaveRequest = await _repo.getLeaveRequestById(request.LeaveId);
            Employee requester = await _employeeRepository.GetByIdAsync(leaveRequest.RequesterId);
            LeaveTableConfig config = await _repo.getLeaveTableConfig();

            _logger.LogTrace("Executing handler for request : {request}", nameof(RejectedLeaveRequestCommandHandler));
            try
            {
                // change status to rejected
                leaveRequest.LeaveStatus = 3;
                leaveRequest.IsCompleted = 0;
                await _repo.updateLeaveRequest(leaveRequest);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Leave Management", config.email));
                message.To.Add(MailboxAddress.Parse(requester.EmployeeEmail));

                message.Subject = LeaveEmailTemplate.getRejectedEmailSubject();
                message.Body = LeaveEmailTemplate.GetRejectedEmailBody(requester.FullName, leaveRequest.LeaveStartDate, config.redirect_link);

                var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(config.email, config.password);
                await smtpClient.SendAsync(message);

                // update status to rejected

                return ApiHelperResponse.Success("yes");
            }
            catch (Exception ex){
                Console.WriteLine(ex.Message);
                return Result.Failure<ApiResponse>(ex.Message); ;
            }
        }
    }
}    
