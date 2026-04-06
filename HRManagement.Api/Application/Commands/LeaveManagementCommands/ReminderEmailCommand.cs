using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Commands.LeaveManagementCommands.Helper;
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
    public class ReminderEmailCommand : IRequest<Result<ApiResponse>>
    {

        public ReminderEmailCommand()
        {
        }
    

        internal class ReminderEmailCommandHandler : IRequestHandler<ReminderEmailCommand, Result<ApiResponse>>
        {
            private readonly ILogger<DeleteLeaveRequestCommandHandler> _logger;
            private readonly ILeaveRepository _repo;
            private readonly IEmployeeRepository _employeeRepo;
            public ReminderEmailCommandHandler(
                ILeaveRepository repo
                , ILogger<DeleteLeaveRequestCommandHandler> logger
                , IEmployeeRepository employeeRepo
            )
            {
                _repo = repo;
                _logger = logger;
                _employeeRepo = employeeRepo;
            }

            public async Task<Result<ApiResponse>> Handle(ReminderEmailCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var requests = await _repo.getAllRequestNeedsReminder();

                    if(requests == null || requests.Count == 0)
                    {
                        return ApiHelperResponse.NotFound("No leave requests need reminder emails");
                    }

                    var semaphore = new SemaphoreSlim(5);

                    var tasks = requests.Select(async item =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            sendEmail(item);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });


                    return ApiHelperResponse.Success("Leave request deleted successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return ApiHelperResponse.Failed("Failed to delete leave request");
                }


            }



            public async void sendEmail(LeaveRequestModel request)
            {
                LeaveConfig config = await _repo.getLeaveConfig();
                Employee? supervisor = await _employeeRepo.GetByIdAsync(request.SupervisorId);
                Employee? requester = await _employeeRepo.GetByIdAsync(request.RequesterId);
                string subject = LeaveTemplate.ReminderEmailSubject();
                string body = LeaveTemplate.ReminderEmailBody(request, requester, supervisor, config.RedirectLink);

                try
                {
                    var message = new MimeMessage();

                    message.From.Add(new MailboxAddress(subject, config.Email));
                    message.To.Add(MailboxAddress.Parse(supervisor.EmployeeEmail));

                    message.Body = new TextPart("plain")
                    {
                        Text = body
                    };

                    var smtpClient = new SmtpClient();
                    await smtpClient.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(config.Email, config.Password);
                    await smtpClient.SendAsync(message);


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //return null;
                }
            }

        }
    }
}
