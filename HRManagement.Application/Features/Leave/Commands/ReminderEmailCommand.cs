using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Application.Features.Leave.Commands.Helper;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using MimeKit;

namespace HRManagement.Application.Features.Leave.Commands
{
    public class ReminderEmailCommand : IRequest<Result<ApiResponse>>
    {

        public ReminderEmailCommand()
        {
        }
    

        internal class ReminderEmailCommandHandler : IRequestHandler<ReminderEmailCommand, Result<ApiResponse>>
        {
            private readonly ILogger<ReminderEmailCommandHandler> _logger;
            private readonly ILeaveRepository _repo;
            private readonly IEmployeeRepository _employeeRepo;
            public ReminderEmailCommandHandler(
                ILeaveRepository repo
                , ILogger<ReminderEmailCommandHandler> logger
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
                            //sendEmail(item);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });


                    return ApiHelperResponse.Success();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return ApiHelperResponse.Failed("Failed to delete leave request");
                }


            }



            public async Task sendEmail(LeaveRequestModel request)
            {
                LeaveTableConfig config = await _repo.getLeaveTableConfig();
                Employee? supervisor = await _employeeRepo.GetByIdAsync(request.SupervisorId);
                Employee? requester = await _employeeRepo.GetByIdAsync(request.RequesterId);
                string subject = LeaveTemplate.ReminderEmailSubject();
                string body = LeaveTemplate.ReminderEmailBody(request, requester, supervisor, config.redirect_link);

                try
                {
                    var message = new MimeMessage();

                    message.From.Add(new MailboxAddress(subject, config.email));
                    message.To.Add(MailboxAddress.Parse(supervisor.EmployeeEmail));

                    message.Body = new TextPart("plain")
                    {
                        Text = body
                    };

                    var smtpClient = new SmtpClient();
                    await smtpClient.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(config.email, config.password);
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
