using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using HRManagement.Application.Interfaces;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Timesheet;



public class TimesheetProjectRepository(AppDbContext dbContext) 
    : TimesheetBaseRepository<TimesheetProject>(dbContext), ITimesheetProjectRepository
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


