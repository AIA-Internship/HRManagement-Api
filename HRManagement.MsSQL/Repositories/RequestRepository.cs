using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Repositories;

public class RequestRepository : BaseRepository<EmployeeUpdateRequest>, IRequestRepository
{
    public async Task UpdateRequestStatusAsync(EmployeeUpdateRequest request)
    {
        dbContext.EmployeeUpdateRequests.Update(request);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<EmployeeRequestResponseDto>> GetMyEmployeeUpdateRequestAsync(int? status, int employeeId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext
           .AsNoTracking()
           .Where(e => !e.IsDeleted && e.Id == employeeId);

        var finalQuery = await query
            .OrderByDescending(r => r.CreatedUtcDate)
            .Select(e => new EmployeeRequestResponseDto(
                e.Id,
                e.Employee.EmploymentInformations.Select(d => d.DisplayId).FirstOrDefault() ?? "",
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

    public async Task<EmployeeUpdateRequest?> GetEmployeeUpdateRequestByIdAsync(int id)
    {
        return await dbContext.EmployeeUpdateRequests
            .Include(r => r.Employee)
            .ThenInclude(e => e.EmploymentInformation) 
            .Include(r => r.Employee)
            .ThenInclude(e => e.EmergencyContacts)
            .FirstOrDefaultAsync(r => r.Id == id);;
    }

    public async Task SubmitUpdateRequestAsync(EmployeeUpdateRequest request)
    {
        await dbContext.EmployeeUpdateRequests.AddAsync(request);
        await dbContext.SaveChangesAsync();
    }
}