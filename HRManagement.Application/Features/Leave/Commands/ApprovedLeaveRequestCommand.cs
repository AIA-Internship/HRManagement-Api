using CSharpFunctionalExtensions;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using MimeKit;

namespace HRManagement.Application.Features.Leave.Commands
{
    public class ApprovedLeaveRequestCommand : IRequest<Result<ApiResponse>>
    {
        public int LeaveId { get; set; }

        public ApprovedLeaveRequestCommand(int id)
        {
            LeaveId = id;
        }
    }


    internal class ApprovedLeaveRequestCommandHandler
    : IRequestHandler<ApprovedLeaveRequestCommand, Result<ApiResponse>>
    {
        private readonly ILogger<ApprovedLeaveRequestCommandHandler> _logger;
        private readonly ILeaveRepository _repo;
        private readonly ILeaveRepository _leaveBalanceRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public ApprovedLeaveRequestCommandHandler(
            ILeaveRepository repo
            , ILogger<ApprovedLeaveRequestCommandHandler> logger
            , ILeaveRepository leaveBalanceRepository
            , IEmployeeRepository employeeRepository
        )
        {
            _repo = repo;
            _logger = logger;
            _leaveBalanceRepository = leaveBalanceRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<Result<ApiResponse>> Handle(
            ApprovedLeaveRequestCommand request,
            CancellationToken cancellationToken)
        {
            LeaveRequestModel leaveRequest = await _repo.getLeaveRequestById(request.LeaveId);
            Employee requester = await _employeeRepository.GetByReqByIdAsync(leaveRequest.RequesterId);
            LeaveTableConfig config = await _repo.getLeaveTableConfig();

            _logger.LogTrace("Executing handler for request : {request}", nameof(ApprovedLeaveRequestCommandHandler));
            try
            {

                // balance -duration
                var res = await _leaveBalanceRepository.getLeaveBalanceById(requester.Id);
                _logger.LogInformation("EmployeeId: {id}", res?.EmployeeId);
                if (res == null)
                    return ApiHelperResponse.Failed("Leave balance not found");

                res.LeaveBalance -= leaveRequest.DayAmount;
                await _leaveBalanceRepository.updateLeaveBalance(res);

                // change status to approve
                leaveRequest.LeaveStatus = 2;
                leaveRequest.IsCompleted = 1;
                await _repo.updateLeaveRequest(leaveRequest);

                //email send
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Leave Management", config.email));
                message.To.Add(MailboxAddress.Parse(requester.EmployeeEmail));

                message.Subject = LeaveEmailTemplate.getApprovedEmailSubject();
                message.Body = LeaveEmailTemplate.GetApprovedEmailBody(requester.FullName, leaveRequest.LeaveStartDate, config.redirect_link);

                var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(config.email, config.password);
                await smtpClient.SendAsync(message);

                return ApiHelperResponse.Success();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
