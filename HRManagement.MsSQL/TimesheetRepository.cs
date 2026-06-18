using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories;

public class TimesheetRepository(AppDbContext dbContext) : ITimesheetRepository
{
    // ── Projects ─────────────────────────────────────────────────────────────

    public async Task<List<TimesheetProject>> GetAllProjectsAsync()
    {
        return await dbContext.TimesheetProjects
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<TimesheetProject?> GetProjectByIdAsync(int id)
    {
        return await dbContext.TimesheetProjects
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task AddProjectAsync(TimesheetProject project)
    {
        await dbContext.TimesheetProjects.AddAsync(project);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateProjectAsync(TimesheetProject project)
    {
        await dbContext.SaveChangesAsync();
    }

    // ── Entries ───────────────────────────────────────────────────────────────

    public async Task<List<TimesheetEntry>> GetEntriesByDateAsync(int employeeId, DateOnly date)
    {
        var entries = await dbContext.TimesheetEntries
            .AsNoTracking()
            .Where(e => e.EmployeeId == employeeId && e.EntryDate == date && !e.IsDeleted)
            .ToListAsync();

        await HydrateEntriesAsync(entries);
        return entries;
    }

    public async Task<List<TimesheetEntry>> GetEntriesByMonthAsync(int employeeId, int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var entries = await dbContext.TimesheetEntries
            .AsNoTracking()
            .Where(e => e.EmployeeId == employeeId
                        && e.EntryDate >= start
                        && e.EntryDate <= end
                        && !e.IsDeleted)
            .OrderBy(e => e.EntryDate)
            .ToListAsync();

        await HydrateEntriesAsync(entries);
        return entries;
    }

    public async Task<List<TimesheetEntry>> GetEntriesByWeekAsync(int employeeId, DateOnly weekStart, DateOnly weekEnd)
    {
        var entries = await dbContext.TimesheetEntries
            .AsNoTracking()
            .Where(e => e.EmployeeId == employeeId
                        && e.EntryDate >= weekStart
                        && e.EntryDate <= weekEnd
                        && !e.IsDeleted)
            .OrderBy(e => e.EntryDate)
            .ToListAsync();

        await HydrateEntriesAsync(entries);
        return entries;
    }

    public async Task<TimesheetEntry?> GetEntryByIdAsync(int id)
    {
        var entry = await dbContext.TimesheetEntries
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (entry != null)
        {
            await HydrateEntriesAsync(new List<TimesheetEntry> { entry });
        }
        return entry;
    }

    public async Task SaveDailyEntriesAsync(int employeeId, DateOnly date, IEnumerable<TimesheetEntry> entries)
    {
        // Replace all entries for this employee on this date (soft-delete old)
        var existing = await dbContext.TimesheetEntries
            .Where(e => e.EmployeeId == employeeId && e.EntryDate == date && !e.IsDeleted)
            .ToListAsync();

        foreach (var old in existing)
        {
            old.IsDeleted = true;
        }

        await dbContext.TimesheetEntries.AddRangeAsync(entries);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteEntryAsync(TimesheetEntry entry)
    {
        entry.IsDeleted = true;
        await dbContext.SaveChangesAsync();
    }

    // ── Submissions ───────────────────────────────────────────────────────────

    public async Task<TimesheetSubmission?> GetSubmissionAsync(int employeeId, int year, int month)
    {
        return await dbContext.TimesheetSubmissions
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId
                                      && s.Year == year
                                      && s.Month == month
                                      && !s.IsDeleted);
    }

    public async Task<TimesheetSubmission?> GetSubmissionByIdAsync(int submissionId)
    {
        var sub = await dbContext.TimesheetSubmissions
            .FirstOrDefaultAsync(s => s.Id == submissionId && !s.IsDeleted);

        if (sub != null)
        {
            sub.Employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.Id == sub.EmployeeId);
        }
        return sub;
    }

    public async Task<List<TimesheetSubmission>> GetSubmissionsByEmployeeAsync(int employeeId)
    {
        return await dbContext.TimesheetSubmissions
            .AsNoTracking()
            .Where(s => s.EmployeeId == employeeId && !s.IsDeleted)
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .ToListAsync();
    }

    public async Task<List<TimesheetSubmission>> GetPendingSubmissionsAsync()
    {
        var subs = await dbContext.TimesheetSubmissions
            .AsNoTracking()
            .Where(s => s.Status == 0 && !s.IsDeleted)
            .OrderBy(s => s.SubmittedDate)
            .ToListAsync();

        var empIds = subs.Select(s => s.EmployeeId).ToList();
        var employees = await dbContext.Employees.Where(e => empIds.Contains(e.Id)).ToListAsync();

        foreach (var s in subs) s.Employee = employees.FirstOrDefault(e => e.Id == s.EmployeeId);
        return subs;
    }

    public async Task<List<TimesheetSubmission>> GetAllSubmissionsAsync()
    {
        var subs = await dbContext.TimesheetSubmissions
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.SubmittedDate)
            .ToListAsync();

        var empIds = subs.Select(s => s.EmployeeId).ToList();
        var employees = await dbContext.Employees.Where(e => empIds.Contains(e.Id)).ToListAsync();

        foreach (var s in subs) s.Employee = employees.FirstOrDefault(e => e.Id == s.EmployeeId);
        return subs;
    }

    public async Task AddSubmissionAsync(TimesheetSubmission submission)
    {
        await dbContext.TimesheetSubmissions.AddAsync(submission);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateSubmissionAsync(TimesheetSubmission submission)
    {
        await dbContext.SaveChangesAsync();
    }

    // ── Day Comments ──────────────────────────────────────────────────────────

    public async Task<List<TimesheetDayComment>> GetCommentsBySubmissionAsync(int submissionId)
    {
        return await dbContext.TimesheetDayComments
            .AsNoTracking()
            .Where(c => c.SubmissionId == submissionId && !c.IsDeleted)
            .OrderBy(c => c.CommentDate)
            .ToListAsync();
    }

    public async Task SaveDayCommentsAsync(int submissionId, IEnumerable<TimesheetDayComment> comments)
    {
        // Remove old comments for this submission
        var existing = await dbContext.TimesheetDayComments
            .Where(c => c.SubmissionId == submissionId && !c.IsDeleted)
            .ToListAsync();

        foreach (var old in existing)
        {
            old.IsDeleted = true;
        }

        await dbContext.TimesheetDayComments.AddRangeAsync(comments);
        await dbContext.SaveChangesAsync();
    }

    // ── To-Do Tasks ───────────────────────────────────────────────────────────

    public async Task<List<TodoTask>> GetTodoTasksByEmployeeAsync(int employeeId)
    {
        return await dbContext.TodoTasks
            .AsNoTracking()
            .Where(t => t.EmployeeId == employeeId && !t.IsDeleted)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<TodoTask?> GetTodoTaskByIdAsync(int id)
    {
        return await dbContext.TodoTasks
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task AddTodoTaskAsync(TodoTask task)
    {
        await dbContext.TodoTasks.AddAsync(task);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateTodoTaskAsync(TodoTask task)
    {
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteTodoTaskAsync(TodoTask task)
    {
        task.IsDeleted = true;
        await dbContext.SaveChangesAsync();
    }

    // ── Dashboard Helpers ─────────────────────────────────────────────────────

    public async Task<List<DateOnly>> GetMissingEntryDatesAsync(int employeeId, int year, int month)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var start = new DateOnly(year, month, 1);
        var end = (today.Year == year && today.Month == month) ? today : start.AddMonths(1).AddDays(-1);

        // Collect all working days in range
        var allWorkingDays = new List<DateOnly>();
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
            {
                allWorkingDays.Add(d);
            }
        }

        // Get dates that have at least one entry
        var datesWithEntries = await dbContext.TimesheetEntries
            .AsNoTracking()
            .Where(e => e.EmployeeId == employeeId
                        && e.EntryDate >= start
                        && e.EntryDate <= end
                        && !e.IsDeleted)
            .Select(e => e.EntryDate)
            .Distinct()
            .ToListAsync();

        return allWorkingDays.Except(datesWithEntries).OrderBy(d => d).ToList();
    }

    public async Task<List<TimesheetEntry>> GetAllEntriesByMonthForAllEmployeesAsync(int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var entries = await dbContext.TimesheetEntries
            .AsNoTracking()
            .Where(e => e.EntryDate >= start && e.EntryDate <= end && !e.IsDeleted)
            .ToListAsync();

        await HydrateEntriesAsync(entries);
        return entries;
    }

    private async Task HydrateEntriesAsync(IEnumerable<TimesheetEntry> entries)
    {
        if (!entries.Any()) return;

        var projectIds = entries.Select(e => e.ProjectId).Distinct().ToList();
        var leadIds = entries.Select(e => e.ProjectLeadId).Distinct().ToList();
        var empIds = entries.Select(e => e.EmployeeId).Distinct().ToList();

        var projects = await dbContext.TimesheetProjects.Where(p => projectIds.Contains(p.Id)).ToListAsync();
        var leads = await dbContext.Employees.Where(e => leadIds.Contains(e.Id)).ToListAsync();
        var employees = await dbContext.Employees.Where(e => empIds.Contains(e.Id)).ToListAsync();

        foreach (var e in entries)
        {
            e.Project = projects.FirstOrDefault(p => p.Id == e.ProjectId)!;
            e.ProjectLead = leads.FirstOrDefault(l => l.Id == e.ProjectLeadId)!;
            e.Employee = employees.FirstOrDefault(emp => emp.Id == e.EmployeeId)!;
        }
    }
}
