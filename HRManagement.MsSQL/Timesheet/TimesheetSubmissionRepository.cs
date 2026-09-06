using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using HRManagement.Application.Interfaces;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Timesheet;



public class TimesheetSubmissionRepository(AppDbContext dbContext) 
    : TimesheetBaseRepository<TimesheetSubmission>(dbContext), ITimesheetSubmissionRepository
{
    public async Task<TimesheetSubmission?> GetSubmissionAsync(int employeeId, int year, int month)
    {
        return await dbContext.TimesheetSubmissions
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.Year == year && s.Month == month && !s.IsDeleted);
    }

    public async Task<List<TimesheetSubmission>> GetSubmissionsByEmployeeAsync(int employeeId)
    {
        return await dbContext.TimesheetSubmissions
            .AsNoTracking()
            .Where(s => s.EmployeeId == employeeId && !s.IsDeleted)
            .OrderByDescending(s => s.Year).ThenByDescending(s => s.Month)
            .ToListAsync();
    }

    public async Task<List<TimesheetSubmission>> GetPendingSubmissionsAsync()
    {
        var subs = await dbContext.TimesheetSubmissions
            .AsNoTracking()
            .Where(s => s.Status == 0 && !s.IsDeleted).OrderBy(s => s.SubmittedDate).ToListAsync();

        var ids = subs.Select(s => s.EmployeeId).ToList();
        var emps = await dbContext.Employee.Where(e => ids.Contains(e.Id)).ToListAsync();
        foreach (var s in subs) s.Employee = emps.FirstOrDefault(e => e.Id == s.EmployeeId);
        return subs;
    }

    public async Task<List<TimesheetSubmission>> GetAllSubmissionsAsync()
    {
        var subs = await dbContext.TimesheetSubmissions
            .AsNoTracking()
            .Where(s => !s.IsDeleted).OrderByDescending(s => s.SubmittedDate).ToListAsync();

        var ids = subs.Select(s => s.EmployeeId).ToList();
        var emps = await dbContext.Employee.Where(e => ids.Contains(e.Id)).ToListAsync();
        foreach (var s in subs) s.Employee = emps.FirstOrDefault(e => e.Id == s.EmployeeId);
        return subs;
    }

    // Comments related specifically to a submission
    public async Task<List<TimesheetDayComment>> GetCommentsBySubmissionAsync(int submissionId)
    {
        return await dbContext.TimesheetDayComments
            .AsNoTracking()
            .Where(c => c.SubmissionId == submissionId && !c.IsDeleted).OrderBy(c => c.CommentDate).ToListAsync();
    }

    public async Task SaveDayCommentsAsync(int submissionId, IEnumerable<TimesheetDayComment> comments)
    {
        var existing = await dbContext.TimesheetDayComments.Where(c => c.SubmissionId == submissionId && !c.IsDeleted).ToListAsync();
        foreach (var old in existing) old.IsDeleted = true;
        await dbContext.TimesheetDayComments.AddRangeAsync(comments);
        await dbContext.SaveChangesAsync();
    }
}



