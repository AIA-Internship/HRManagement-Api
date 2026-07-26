using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace HRManagement.MsSQL.Repositories
{
    public class LeaveRepository
        : BaseRepository<LeaveRequestModel>, ILeaveRepository
    {
        private readonly AppDbContext _dbContext;

        public LeaveRepository(AppDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<int> createLeaveRequest(LeaveRequestModel leaveRequest)
        {
            try
            {


                await _dbContext.LeaveRequest.AddAsync(leaveRequest);

                var affectedRows = await _dbContext.SaveChangesAsync();

                if (affectedRows > 0)
                {
                    // After SaveChanges, EF should have populated the LeaveId
                    return leaveRequest.LeaveId;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }

        }

        public async Task<LeaveRequestModel> getLeaveRequestById(int id, int requesterId)
        {
            var leaveRequest = await _dbContext.LeaveRequest
                .FirstOrDefaultAsync(x =>
                    x.LeaveId == id &&
                    x.RequesterId == requesterId &&
                    x.IsDeleted == 0);


            return leaveRequest;
        }

        public async Task<List<LeaveRequestModel>> getLeaveRequestsByRequesterId(int requesterId, int max)
        {
            try
            {
                var requests = await _dbContext.LeaveRequest
                    .Where(x => x.RequesterId == requesterId && x.IsDeleted == 0)
                    .OrderByDescending(x => x.CreatedUtcDate)
                    .Take(max)
                    .ToListAsync();

                return requests;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new List<LeaveRequestModel>();
            }

        }

        public async Task<bool> updateLeaveRequest(LeaveRequestModel data)
        {
            data.IsEdited = 1;
            _dbContext.LeaveRequest.Update(data);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task AddLeaveAttachmentsAsync(List<LeaveAttachment> entities, CancellationToken ct)
        {
            await _sqldbContext.Set<LeaveAttachment>().AddRangeAsync(entities, ct);
        }

        public async Task DeleteLeaveAttachmentByIdAsync(int attachmentId, CancellationToken ct)
        {
            var attachment = await _dbContext.LeaveAttachment
                .FirstOrDefaultAsync(x => x.AttachmentId == attachmentId, ct);

            if (attachment != null)
            {
                _dbContext.LeaveAttachment.Remove(attachment);
            }
        }

        public async Task<List<LeaveAttachment>> getLeaveAttachmentsByLeaveId(int leaveId)
        {
            return await _dbContext.LeaveAttachment
                .Where(x => x.LeaveId == leaveId && !x.IsDeleted)
                .OrderBy(x => x.AttachmentId)
                .ToListAsync();
        }

        public async Task<LeaveAttachment?> GetAttachmentByIdAsync(int attachmentId)
        {
            return await _dbContext.LeaveAttachment
                .FirstOrDefaultAsync(x =>
                    x.AttachmentId == attachmentId &&
                    !x.IsDeleted);
        }

        public async Task<List<LeaveRequestHistory>> getAllEditById(int leaveId)
        {
            return await _dbContext.LeaveRequestHistory
                .Where(x => x.LeaveId == leaveId)
                .OrderBy(x => x.HistoryId)
                .ToListAsync();
        }

        public async Task<bool> createLeaveRequestHistory(LeaveRequestHistory data)
        {
            await _dbContext.LeaveRequestHistory.AddAsync(data);

            var affectedRows = await _dbContext.SaveChangesAsync();

            return affectedRows > 0;
        }

        public async Task<bool> DeleteLeaveRequest(int leaveId)
        {
            var leaveRequest = await _dbContext.LeaveRequest
                .FirstOrDefaultAsync(x => x.LeaveId == leaveId);

            if (leaveRequest == null)
                return false;

            // 1. Soft delete LeaveRequest
            leaveRequest.IsDeleted = 1;

            // 2. Soft delete LeaveAttachment
            var attachments = await _dbContext.LeaveAttachment
                .Where(x => x.LeaveId == leaveId)
                .ToListAsync();
            Console.WriteLine($"Attachment Count = {attachments.Count}");

            foreach (var attachment in attachments)
            {
                Console.WriteLine($"AttachmentId = {attachment.AttachmentId}");
                attachment.IsDeleted = true;
            }

            // 3. Hard delete LeaveRequestHistory
            var histories = await _dbContext.LeaveRequestHistory
                .Where(x => x.LeaveId == leaveId)
                .ToListAsync();

            if (histories.Any())
            {
                _dbContext.LeaveRequestHistory.RemoveRange(histories);
            }

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<List<GetLeaveRequestByMonthRangeDto>> getLeaveRequestByMonthRage(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            return await (
                from lr in _dbContext.LeaveRequest
                join e in _dbContext.Employee on lr.RequesterId equals e.Id
                where lr.LeaveStartDate >= startDate && lr.LeaveStartDate < endDate && lr.IsDeleted == 0 /*&& lr.IsEdit == 0*/

                select new GetLeaveRequestByMonthRangeDto
                (

                    lr.LeaveId,
                    lr.RequesterId,
                    lr.SupervisorId,
                    lr.LeaveDescription,
                    lr.LeaveStatus,
                    lr.LeaveStartDate,
                    lr.DayAmount,
                    lr.LeaveType,
                    lr.IsCompleted,
                    lr.CreatedUtcDate,
                    e.FullName,
                    !string.IsNullOrWhiteSpace(lr.RequesterDisplayId) 
                        ? lr.RequesterDisplayId 
                        : (e.EmploymentInformation != null ? e.EmploymentInformation.DisplayId : null)
                )

                ).ToListAsync();
        }

        public async Task<LeaveTableConfig> getLeaveTableConfig()
        {
            return await _dbContext.LeaveTableConfig.FirstOrDefaultAsync();
        }



        public async Task<List<LeaveRequestModel>> getAllRequestNeedsReminder()
        {
            var targetDate = DateTime.Today.AddDays(2);

            return await _dbContext.LeaveRequest
                .Where(x => x.LeaveStartDate.Date == targetDate)
                .ToListAsync();
        }

        public async Task<bool> createLeaveBalance(LeaveBalanceModel leaveBalance)
        {
            try
            {
                await _dbContext.LeaveBalanceModel.AddAsync(leaveBalance);

                var affectedRows = await _dbContext.SaveChangesAsync();

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> deleteLeaveBalance(int id)
        {
            try
            {
                var res = await _dbContext.LeaveBalanceModel.FindAsync(id);
                if (res == null) return false;

                _dbContext.LeaveBalanceModel.Remove(res);

                var affectedRows = await _dbContext.SaveChangesAsync();

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<LeaveBalanceModel> getLeaveBalanceById(int id)
        {
            try
            {
                var res = await _dbContext.LeaveBalanceModel.FirstOrDefaultAsync(x => x.EmployeeId == id);
                if (res == null) return null;

                return res;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> updateLeaveBalance(LeaveBalanceModel leaveBalance)
        {
            try
            {

                _dbContext.LeaveBalanceModel.Update(leaveBalance);
                return await _dbContext.SaveChangesAsync() > 0;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> incrementAllEmployeeLeaveRequest()
        {
            var balance = await _dbContext.LeaveBalanceModel.ToListAsync();

            foreach (var b in balance)
            {
                b.LeaveBalance += 1;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<LeaveTypeCountDto> GetLeaveTypeCounts(int employeeId)
        {
            var result = await _dbContext.LeaveRequest
                .Where(x => x.RequesterId == employeeId
                         && x.IsDeleted == 0
                         && x.IsCompleted == 1) 
                .GroupBy(x => 1)
                .Select(g => new LeaveTypeCountDto
                {
                    PaidLeave = g.Count(x => x.LeaveType == 1),
                    UnpaidLeave = g.Count(x => x.LeaveType == 2)
                })
                .FirstOrDefaultAsync();

            return result ?? new LeaveTypeCountDto();
        }
        public async Task<List<LeaveRequestModel>> getLeaveRequestBySupervisorId(int supervisorId, int max)
        {
            var result = await _dbContext.LeaveRequest
                .Where(x => x.SupervisorId == supervisorId && x.IsDeleted == 0)
                .OrderBy(x => x.LeaveStatus)
                .ThenBy(x => x.CreatedUtcDate)
                .Take(max)
                .ToListAsync();


            return result;
        }

        public async Task<List<LeaveTimelineDto>> GetLeaveTimeline(int leaveId)
        {
            var timeline = new List<LeaveTimelineDto>();

            var leaveRequest = await _dbContext.LeaveRequest
                .FirstOrDefaultAsync(x => x.LeaveId == leaveId && x.IsDeleted == 0);

            if (leaveRequest == null)
                return timeline;

            // Request Created
            timeline.Add(new LeaveTimelineDto
            {
                Status = "Created",
                ModifiedUtcDate = leaveRequest.CreatedUtcDate
            });

            // Edited History
            if (leaveRequest.IsEdited == 1)
            {
                var histories = await _dbContext.LeaveRequestHistory
                .Where(x => x.LeaveId == leaveId)
                .OrderBy(x => x.HistoryId)
                .ToListAsync();

                foreach (var history in histories)
                {
                    timeline.Add(new LeaveTimelineDto
                    {
                        Status = "Edited",
                        ModifiedUtcDate = history.ModifiedUtcDate
                    });
                }
            }

            // Approved / Rejected
            if (leaveRequest.LeaveStatus == 2)
            {
                timeline.Add(new LeaveTimelineDto
                {
                    Status = "Approved",
                    ModifiedUtcDate = leaveRequest.ModifiedUtcDate
                });
            }
            else if (leaveRequest.LeaveStatus == 3)
            {
                timeline.Add(new LeaveTimelineDto
                {
                    Status = "Rejected",
                    ModifiedUtcDate = leaveRequest.ModifiedUtcDate
                });
            }

            return timeline;
        }
    }
}
