using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;

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
        public async Task<bool> createLeaveRequest(LeaveRequestModel leaveRequest)
        {
            try
            {
                await _dbContext.LeaveRequest.AddAsync(leaveRequest);

                var affectedRows = await _dbContext.SaveChangesAsync();

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public async Task<LeaveRequestModel> getLeaveRequestById(int id)
        {
            return await _dbContext.LeaveRequest
                 .FirstOrDefaultAsync(x => x.LeaveId == id && x.IsDeleted == 0 );

        }

        public async Task<List<LeaveRequestModel>> getLeaveRequestsByRequesterId(int requesterId, int max)
        {
            try
            {
                return await _dbContext.LeaveRequest
                    .Where(x => x.RequesterId == requesterId && x.IsDeleted == 0)
                    .OrderByDescending(x => x.CreatedUtcDate)
                    .Take(max)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new List<LeaveRequestModel>();
            }

        }

        public async Task<bool> updateLeaveRequest(LeaveRequestModel data)
        {
            _dbContext.LeaveRequest.Update(data);
            return await _dbContext.SaveChangesAsync() > 0;
        }



        public async Task<List<LeaveRequestHistory>> getAllEditById(int leaveId)
        {
            return await _dbContext.LeaveRequestHistory
                .Where(x => x.InitialRequestId == leaveId )
                .ToListAsync();
        }

        public async Task<bool> createLeaveRequestHistory(LeaveRequestHistory data)
        {
            await _dbContext.LeaveRequestHistory.AddAsync(data);

            var affectedRows = await _dbContext.SaveChangesAsync();

            return affectedRows > 0;
        }

        public async Task<bool> softDelete(int id)
        {
            var entity = await _dbContext.LeaveRequest.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = 1;

            return await _dbContext.SaveChangesAsync() > 0;
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
                    lr.AttachmentPath,
                    lr.CreatedUtcDate,
                    e.FullName
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
    }
}
