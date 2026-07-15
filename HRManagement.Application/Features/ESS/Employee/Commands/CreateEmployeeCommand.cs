using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;

using MediatR;

using Microsoft.Extensions.Logging;

using System.Text.RegularExpressions;

namespace HRManagement.Application.Features.ESS.Employee.Commands;

public record CreateEmployeeCommand(AddEmployeePayload Payload, int CurrentUserId) : IRequest<Result>;

internal sealed class CreateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IELearningRepository eLearningRepository,
    ILogger<CreateEmployeeCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateEmployeeCommand, Result>
{
    public async Task<Result> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(CreateEmployeeCommandHandler));

        var payload = request.Payload;
        var actionerId = request.CurrentUserId;

        EmploymentInformation? employmentInfo = null;
        if (payload.EmploymentInformation != null) // Contoh validasi
        {
            var displayId = payload.EmploymentInformation.EmployeeDisplayId;
            if (string.IsNullOrWhiteSpace(displayId))
                displayId = await GenerateNextDisplayId(employeeRepository);

            employmentInfo = new EmploymentInformation(
                employeeId: 0,
                payload.EmploymentInformation.EmploymentStatus,
                payload.EmploymentInformation.StartDate,
                payload.EmploymentInformation.EmploymentType,
                payload.EmploymentInformation.Department,
                payload.EmploymentInformation.Position,
                displayId,
                payload.EmploymentInformation.SupervisorId,
                payload.EmploymentInformation.SupervisorName,
                actionerId
            );
        }

        EmergencyContact? emergencyContact = null;
        if (payload.EmergencyContact != null)
        {
            emergencyContact = new EmergencyContact(
                employeeId: 0, // EF Core akan otomatis mengganti ini dengan ID karyawan yang baru di-generate
                payload.EmergencyContact.Name,
                payload.EmergencyContact.PhoneNumber ?? string.Empty,
                payload.EmergencyContact.Relationship ?? string.Empty,
                actionerId
            );
        }

        var data = new Domain.Models.Tables.Employee(
                payload.FullName,
                payload.Gender,
                payload.PersonalEmail,
                payload.EmployeeEmail,
                payload.PhoneNumber,
                payload.Nik,
                payload.PlaceOfBirth,
                payload.DateOfBirth,
                payload.MaritalStatus,
                new Address(
                    payload.CurrentAddress,
                    payload.CurrentCity,
                    payload.CurrentProvince,
                    payload.CurrentPostalCode),
                new Address(
                    payload.ResidentialAddress,
                    payload.ResidentialCity,
                    payload.ResidentialProvince,
                    payload.ResidentialPostalCode),
                payload.RoleId,
                actionerId,
                employmentInfo,
                emergencyContact
            );

        await employeeRepository.AddAsync(data, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);

        const int internRoleId = 2;
        if (data.RoleId == internRoleId)
        {
            await eLearningRepository.AssignInternToGroupAsync(data.Id, DateTime.UtcNow);
        }

        return Result.Success();
    }

    private static async Task<string> GenerateNextDisplayId(IEmployeeRepository repository)
    {
        var lastId = await repository.GetLastEmployeeDisplayIdAsync();
        if (string.IsNullOrEmpty(lastId))
        {
            return "E0001";
        }

        var match = Regex.Match(lastId, @"(\D*)(\d+)");
        if (match.Success)
        {
            var prefix = match.Groups[1].Value;
            var numberStr = match.Groups[2].Value;

            if (long.TryParse(numberStr, out var number))
            {
                var nextNumber = number + 1;
                if (string.IsNullOrEmpty(prefix)) prefix = "E";

                return $"{prefix}{nextNumber.ToString().PadLeft(numberStr.Length, '0')}";
            }
        }

        return "E0001";
    }
}
