using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.SeedWork;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories.Timesheet;



public class TimesheetProjectRepository(AppDbContext dbContext) 
    : BaseRepository<TimesheetProject>(dbContext), ITimesheetProjectRepository
{
    public override async Task<List<TimesheetProject>> GetActiveListAsync()
    {
        return await dbContext.TimesheetProjects
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
