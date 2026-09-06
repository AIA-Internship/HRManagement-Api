using HRManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using HRManagement.Domain.Models.Payload.EmployeeDtos.Queries.Dto;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Payload.EmployeeDtos.Commands.Dto;
using HRManagement.Domain.Models.Tables;

namespace HRManagement.Application.Commands;

public class UpdateEmployeeCommand(int Id, UpdateEmployeeRequestDto commandDto) : IRequest<ApiResponse<string>>
{
    public int Id { get; } = Id;
    public UpdateEmployeeRequestDto RequestDto { get; } = commandDto;

    public class Handler(IEmployeeRepository employeeRepository, IRequestRepository requestRepository, ICurrentUserService currentUserService, IApplicationDbContext appDbContext)
        : IRequestHandler<UpdateEmployeeCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(UpdateEmployeeCommand command, CancellationToken ct)
        {
            var requesterId = currentUserService.UserId;
            var email = currentUserService.Email;
            
            if (requesterId != command.Id)
            {
                throw new ApiException("Forbidden", StatusCodes.Status403Forbidden, "You can only submit update requests for your own profile.");
            }

            var employeeProfile = await employeeRepository.GetProfileByEmailAsync(email, ct);
            if (employeeProfile == null)
            {
                throw new ApiException("Not Found", StatusCodes.Status404NotFound, ExceptionConstants.EmployeeNotFound);
            }
            
            var payload = new UpdateEmployeePayload(
                command.RequestDto.FullName,
                command.RequestDto.Gender?.ToString(), // Since Gender in RequestDto is int, but payload expects string?
                command.RequestDto.PersonalEmail,
                command.RequestDto.PlaceOfBirth,
                null, // Nik
                command.RequestDto.DateOfBirth,
                command.RequestDto.MaritalStatus,
                command.RequestDto.CurrentStreetAddress,
                command.RequestDto.CurrentCity,
                command.RequestDto.CurrentProvince,
                command.RequestDto.CurrentPostalCode,
                command.RequestDto.ResidentialStreetAddress,
                command.RequestDto.ResidentialCity,
                command.RequestDto.ResidentialProvince,
                command.RequestDto.ResidentialPostalCode,
                command.RequestDto.PhoneNumber,
                command.RequestDto.EmergencyContactName,
                command.RequestDto.EmergencyContactPhone,
                command.RequestDto.EmergencyContactRelationship
            );

            var req = new EmployeeUpdateRequest(employeeProfile, payload, requesterId, null);

            await requestRepository.AddAsync(req, ct);
            await appDbContext.SaveChangesAsync(ct);
            return ApiHelperResponse.Success<string>("Update request submitted successfully. Awaiting HR approval.", "Success");
        }
    }
}
