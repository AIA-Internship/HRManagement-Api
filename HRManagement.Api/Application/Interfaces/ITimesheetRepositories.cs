using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.SeedWork;

namespace HRManagement.Api.Application.Interfaces;

// ── Projects ────────────────────────────────────────────────────
public interface ITimesheetProjectRepository : IRepository<TimesheetProject>
{
}

// ── Entries ──────────────────────────────────────────────────────
public interface ITimesheetEntryRepository : IRepository<TimesheetEntry>
{
    Task<List<TimesheetEntry>> GetEntriesByDateAsync(int employeeId, DateOnly date);
    Task<List<TimesheetEntry>> GetEntriesByMonthAsync(int employeeId, int year, int month);
    Task<List<TimesheetEntry>> GetEntriesByWeekAsync(int employeeId, DateOnly weekStart, DateOnly weekEnd);
    Task SaveDailyEntriesAsync(int employeeId, DateOnly date, IEnumerable<TimesheetEntry> entries);
    Task<List<DateOnly>> GetMissingEntryDatesAsync(int employeeId, int year, int month);
    Task<List<TimesheetEntry>> GetAllEntriesByMonthForAllEmployeesAsync(int year, int month);
}

// ── Submissions ──────────────────────────────────────────────────
public interface ITimesheetSubmissionRepository : IRepository<TimesheetSubmission>
{
    Task<TimesheetSubmission?> GetSubmissionAsync(int employeeId, int year, int month);
    Task<List<TimesheetSubmission>> GetSubmissionsByEmployeeAsync(int employeeId);
    Task<List<TimesheetSubmission>> GetPendingSubmissionsAsync();
    Task<List<TimesheetSubmission>> GetAllSubmissionsAsync();
    Task<List<TimesheetDayComment>> GetCommentsBySubmissionAsync(int submissionId);
    Task SaveDayCommentsAsync(int submissionId, IEnumerable<TimesheetDayComment> comments);
}

// ── To-Do Tasks ──────────────────────────────────────────────────
public interface ITodoTaskRepository : IRepository<TodoTask>
{
    Task<List<TodoTask>> GetTodoTasksByEmployeeAsync(int employeeId);
}
