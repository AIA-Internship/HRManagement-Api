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
           .Where(e => !e.IsDeleted
                       && (employeeId == null || e.EmployeeId == employeeId)
                       && (status == null || e.Status == status));

        var finalQuery = await query
            .OrderByDescending(r => r.CreatedUtcDate)
            .Select(e => new EmployeeRequestResponseDto(
                e.Id,
                (e.Employee.EmploymentInformation != null ? e.Employee.EmploymentInformation.DisplayId : null) ?? string.Empty,
                e.Employee.FullName,
                e.NewNik,
                e.NewFullName,
                e.NewGender == null ? null : (e.NewGender == 1 ? "Female" : "Male"),
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
                e.CreatedAt,
                // Reviewer name resolved from ModifiedBy (Users.Id) -> Employee.FullName. Only meaningful once reviewed.
                e.Status == 0 ? null : _sqldbContext.Users
                    .Where(u => u.Id == e.ModifiedBy)
                    .Join(_sqldbContext.Employee, u => u.EmployeeId, emp => emp.Id, (u, emp) => emp.FullName)
                    .FirstOrDefault(),
                e.Status == 0 ? (DateTime?)null : e.ModifiedUtcDate,
                e.ChangesJson
                ))
            .ToListAsync(cancellationToken);


        return finalQuery;
    }

    public async Task<EmployeeUpdateRequest?> GetByIdWithEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext
            .Include(e => e.Employee)
                .ThenInclude(emp => emp.EmergencyContact)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

}