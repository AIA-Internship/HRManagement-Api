using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories;

public class RequestRepository(AppDbContext dbContext) : IRequestRepository
{
    public async Task UpdateRequestStatusAsync(EmployeeUpdateRequest request)
    {
        dbContext.EmployeeUpdateRequests.Update(request);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<EmployeeUpdateRequest>> GetEmployeeUpdateRequestAsync(int? status)
    {
        var query = dbContext.EmployeeUpdateRequests
            .Include(r => r.Employee)
            .AsQueryable();

        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<EmployeeUpdateRequest?> GetEmployeeUpdateRequestByIdAsync(int id)
    {
        return await dbContext.EmployeeUpdateRequests
            .Include(r => r.Employee)
            .ThenInclude(e => e.EmergencyContacts)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task SubmitUpdateRequestAsync(EmployeeUpdateRequest request)
    {
        await dbContext.EmployeeUpdateRequests.AddAsync(request);
        await dbContext.SaveChangesAsync();
    }
}