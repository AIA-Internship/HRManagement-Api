using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces.LeaveManagementInterface;
using HRManagement.Api.Domain.Models.Response.Shared;
using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using MimeKit;

namespace HRManagement.Api.Application.Commands.LeaveManagementCommands
{
    public class SendEmailTestCommand : IRequest<Result<ApiResponse>>
    {
        public string body { get; set; }
        public string receiver { get; set; }

        public SendEmailTestCommand(string body, string receiver) { this.body = body; this.receiver = receiver; }
    }


    internal class SendEmailTestCommandHandler : IRequestHandler<SendEmailTestCommand, Result<ApiResponse>>
    {
        private readonly ILogger<SendEmailTestCommandHandler> _logger;
        private readonly ILeaveRequestRepository _repo;

        public SendEmailTestCommandHandler(
            ILeaveRequestRepository repo
            , ILogger<SendEmailTestCommandHandler> logger
        )
        {
            _repo = repo;
            _logger = logger;
        }

    public async Task<Result<ApiResponse>> Handle(SendEmailTestCommand request, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(SendEmailTestCommandHandler));
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("testis", "fransiskus.wibowo001@binus.ac.id"));
                message.To.Add(MailboxAddress.Parse(request.receiver));

                message.Subject = "Test Email";

                message.Body = new TextPart("plain")
                {
                    Text = request.body
                };

                var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync("fransiskus.wibowo001@binus.ac.id", "Arnold1818");
                await smtpClient.SendAsync(message);

                return ApiHelperResponse.Success("yes");
            }
            catch (Exception ex){
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}    
