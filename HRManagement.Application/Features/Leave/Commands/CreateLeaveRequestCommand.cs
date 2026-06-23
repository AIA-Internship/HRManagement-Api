using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
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
        private readonly IEmployeeRepository _employeeRepository;


        public CreateLeaveRequestCommandHandler(
            ILeaveRepository repo
            , ILogger<CreateLeaveRequestCommandHandler> logger
            , IEmployeeRepository employeeRepository
        )
        {
            _repo = repo;
            _logger = logger;
            _employeeRepository = employeeRepository;
        }
        public async Task<Result<ApiResponse>> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var req = request.LeaveRequestDto;

            Employee spv = await _employeeRepository.GetByIdAsync(req.SupervisorId);
            Employee emp = await _employeeRepository.GetByIdAsync(req.RequesterId);
            LeaveTableConfig config = await _repo.getLeaveTableConfig();

            _logger.LogTrace("Executing handler for request : {request}", nameof(CreateLeaveRequestCommandHandler));
            try
            {
                bool created = await _repo.createLeaveRequest(mapFromCreateDto(request.LeaveRequestDto));

                if(!created)
                {
                    var failureResponse = ApiHelperResponse.Failed("Failed to create leave request");
                    return Result.Failure<ApiResponse>(failureResponse.StatusMessage);
                }

                else
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Leave Management", "fransiskus.wibowo001@binus.ac.id"));
                    message.To.Add(MailboxAddress.Parse(spv.EmployeeEmail));

                    message.Subject = LeaveEmailTemplate.GetRequestApprovalToSpvSubject();
                    message.Body = LeaveEmailTemplate.GetRequestApprovalToSpv(spv.FullName, emp.FullName, DateTime.Now, req.LeaveType , req.leaveStartDate, req.leaveStartDate.AddDays((double)req.DayAmount), config.redirect_link);

                    var smtpClient = new SmtpClient();
                    await smtpClient.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(config.email, config.password);
                    await smtpClient.SendAsync(message);
                }

                return ApiHelperResponse.Success();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Result.Failure<ApiResponse>("Failed to create leave request");
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
                IsEdited = 0,
                IsCompleted = 0,
                CreatedBy = dto.RequesterId,
                CreatedUtcDate = DateTime.UtcNow,
                ModifiedBy = dto.RequesterId,
                ModifiedUtcDate = DateTime.UtcNow
            };

        }

        public async void sendEmail(CreateLeaveRequestDto dto)
        {
            LeaveRequestModel request = mapFromCreateDto(dto);
            LeaveTableConfig config = await _repo.getLeaveTableConfig();
            Employee? supervisor = await _employeeRepository.GetByIdAsync(request.SupervisorId);
            Employee? requester = await _employeeRepository.GetByIdAsync(request.RequesterId);
            string subject = LeaveEmailTemplate.GetRequestApprovalToSpvSubject();

            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(subject, config.email));
                message.To.Add(MailboxAddress.Parse(supervisor.EmployeeEmail));

                message.Body = LeaveEmailTemplate.GetRequestApprovalToSpv(supervisor.FullName,requester.FullName, request.CreatedUtcDate, request.LeaveType ?? 0, request.LeaveStartDate, request.LeaveStartDate.AddDays((double)request.DayAmount), config.redirect_link);


                var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(config.email, config.password);
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
