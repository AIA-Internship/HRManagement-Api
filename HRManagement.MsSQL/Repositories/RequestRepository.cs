using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Repositories;

public class RequestRepository : BaseRepository<EmployeeUpdateRequest>, IRequestRepository
{
    public RequestRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<List<EmployeeRequestResponseDto>> GetMyEmployeeUpdateRequestAsync(int? status, int? employeeId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext
           .AsNoTracking()
           .Where(e => !e.IsDeleted && (employeeId == null || e.Id == employeeId));

        var finalQuery = await query
            .OrderByDescending(r => r.CreatedUtcDate)
            .Select(e => new EmployeeRequestResponseDto(
                e.Id,
                e.Employee.EmploymentInformation.DisplayId,
                e.Employee.FullName,
                e.NewNik,
                e.NewFullName,
                e.NewGender,
                e.NewPersonalEmail,
                e.NewPlaceOfBirth,
                e.NewDateOfBirth,
                e.NewMaritalStatus,
                e.NewCurrentStreetAddress,
                e.NewCurrentCity,
                e.NewCurrentProvince,
                e.NewCurrentZipCode,
                e.NewResidentialStreetAddress,
                e.NewResidentialCity,
                e.NewResidentialProvince,
                e.NewResidentialZipCode,
                e.NewPhoneNumber,
                e.NewEmergencyContactName,
                e.NewEmergencyContactPhone,
                e.NewEmergencyContactRelationship,
                e.Status,
                e.HrReason,
                e.CreatedAt
                ))
            .ToListAsync(cancellationToken);


        return finalQuery;
    }

}