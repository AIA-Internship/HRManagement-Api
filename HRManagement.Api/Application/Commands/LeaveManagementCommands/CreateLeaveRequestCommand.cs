using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Commands.LeaveManagementCommands.Helper;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.Queries;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Domain.SeedWork;
using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using MimeKit;

namespace HRManagement.Api.Application.Commands.LeaveManagementCommands
{
    public class CreateLeaveRequestCommand : IRequest<Result<ApiResponse>>
    {
        public CreateLeaveRequestDto LeaveRequestDto { get; set; }
        public CreateLeaveRequestCommand(CreateLeaveRequestDto leaveRequestDto)
        {
            LeaveRequestDto = leaveRequestDto;
        }
    }
    
    internal class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, Result<ApiResponse>>
    {
        private readonly ILogger<CreateLeaveRequestCommandHandler> _logger;
        private readonly ILeaveRepository _repo;
        private readonly IEmployeeRepository _employeeRepo;

        public CreateLeaveRequestCommandHandler(
            ILeaveRepository repo,
            IEmployeeRepository employeeRepo
            , ILogger<CreateLeaveRequestCommandHandler> logger
        )
        {
            _repo = repo;
            _employeeRepo = employeeRepo;
            _logger = logger;
        }
        public async Task<Result<ApiResponse>> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(CreateLeaveRequestCommandHandler));

            try
            {
                bool created = await _repo.createLeaveRequest(mapFromCreateDto(request.LeaveRequestDto));

                if(!created) return ApiHelperResponse.Failed("Failed to create leave request");

                sendEmail(request.LeaveRequestDto);

                return ApiHelperResponse.Success("Leave request created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static LeaveRequestModel mapFromCreateDto(CreateLeaveRequestDto dto)
        {

            return new LeaveRequestModel
            {
                RequesterId = dto.RequesterId,
                SupervisorId = dto.SupervisorId,
                LeaveDescription = dto.LeaveDescription,
                LeaveStartDate = dto.leaveStartDate,
                DayAmount = dto.DayAmount,
                LeaveType = dto.LeaveType,
                AttachmentPath = MappingHelper.joinAttachmentPath(dto.AttachmentPath),
                IsDeleted = 0,
                IsCompleted = 0,
                IsEdit = 0,
                InitialRequestId = -1,
                CreatedBy = dto.RequesterId,
                CreatedUtcDate = DateTime.UtcNow,
                ModifiedBy = dto.RequesterId,
                ModifiedUtcDate = DateTime.UtcNow
            };

        }

        public async void sendEmail(CreateLeaveRequestDto dto)
        {
            LeaveRequestModel request = mapFromCreateDto(dto);
            LeaveConfig config = await _repo.getLeaveConfig();
            Employee? supervisor = await _employeeRepo.GetByIdAsync(request.SupervisorId);
            Employee? requester = await _employeeRepo.GetByIdAsync(request.RequesterId);
            string subject = LeaveTemplate.NewRequestEmailSubject();
            string body = LeaveTemplate.NewRequestEmailBody(request,requester, supervisor, config.RedirectLink);

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

                //return ApiHelperResponse.Success("yes");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //return null;
            }
        }
    }
}
