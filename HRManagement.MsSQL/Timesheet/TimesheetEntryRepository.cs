using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using HRManagement.Application.Interfaces;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Timesheet;



public class TimesheetEntryRepository(AppDbContext dbContext) 
    : TimesheetBaseRepository<TimesheetEntry>(dbContext), ITimesheetEntryRepository
{
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
            .Where(e => e.EmployeeId == employeeId && e.EntryDate >= start && e.EntryDate <= end && !e.IsDeleted)
            .OrderBy(e => e.EntryDate)
            .ToListAsync();

        await HydrateEntriesAsync(entries);
        return entries;
    }

    public async Task<List<TimesheetEntry>> GetEntriesByWeekAsync(int employeeId, DateOnly weekStart, DateOnly weekEnd)
    {
        var entries = await dbContext.TimesheetEntries
            .AsNoTracking()
            .Where(e => e.EmployeeId == employeeId && e.EntryDate >= weekStart && e.EntryDate <= weekEnd && !e.IsDeleted)
            .OrderBy(e => e.EntryDate)
            .ToListAsync();

        await HydrateEntriesAsync(entries);
        return entries;
    }

    public async Task SaveDailyEntriesAsync(int employeeId, DateOnly date, IEnumerable<TimesheetEntry> entries)
    {
        var existing = await dbContext.TimesheetEntries
            .Where(e => e.EmployeeId == employeeId && e.EntryDate == date && !e.IsDeleted)
            .ToListAsync();

        foreach (var old in existing) old.IsDeleted = true;

        await dbContext.TimesheetEntries.AddRangeAsync(entries);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<DateOnly>> GetMissingEntryDatesAsync(int employeeId, int year, int month)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var start = new DateOnly(year, month, 1);
        var end = (today.Year == year && today.Month == month) ? today : start.AddMonths(1).AddDays(-1);

        var allWorkingDays = new List<DateOnly>();
        for (var d = start; d <= end; d = d.AddDays(1)) { if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday) allWorkingDays.Add(d); }

        var entries = await dbContext.TimesheetEntries
            .AsNoTracking()
            .Where(e => e.EmployeeId == employeeId && e.EntryDate >= start && e.EntryDate <= end && !e.IsDeleted)
            .Select(e => e.EntryDate).Distinct().ToListAsync();

        return allWorkingDays.Except(entries).OrderBy(d => d).ToList();
    }

    public async Task<List<TimesheetEntry>> GetAllEntriesByMonthForAllEmployeesAsync(int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var entries = await dbContext.TimesheetEntries.AsNoTracking().Where(e => e.EntryDate >= start && e.EntryDate <= end && !e.IsDeleted).ToListAsync();
        await HydrateEntriesAsync(entries);
        return entries;
    }

    private async Task HydrateEntriesAsync(IEnumerable<TimesheetEntry> entries)
    {
        if (!entries.Any()) return;
        var pIds = entries.Select(e => e.ProjectId).Distinct().ToList();
        var lIds = entries.Select(e => e.ProjectLeadId).Distinct().ToList();
        var eIds = entries.Select(e => e.EmployeeId).Distinct().ToList();

        var prj = await dbContext.TimesheetProjects.Where(p => pIds.Contains(p.Id)).ToListAsync();
        var lds = await dbContext.Employee.Where(e => lIds.Contains(e.Id)).ToListAsync();
        var emp = await dbContext.Employee.Where(e => eIds.Contains(e.Id)).ToListAsync();

        foreach (var e in entries) {
            e.Project = prj.FirstOrDefault(p => p.Id == e.ProjectId)!;
            e.ProjectLead = lds.FirstOrDefault(l => l.Id == e.ProjectLeadId)!;
            e.Employee = emp.FirstOrDefault(x => x.Id == e.EmployeeId)!;
        }
    }
}



