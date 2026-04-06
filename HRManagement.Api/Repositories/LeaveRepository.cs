using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces.LeaveManagementInterface;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveResponse;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

namespace HRManagement.Api.Repositories
{
    public class LeaveRepository : BaseRepository, ILeaveRepository
    {
        private readonly AppDbContext _dbContext;
        public LeaveRepository(AppDbContext dbContext) : base(dbContext)
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
                join e in _dbContext.Employees on lr.RequesterId equals e.Id
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

        public async Task<LeaveTableCOnfig> getLeaveTableCOnfig()
        {
            return await _dbContext.LeaveTableCOnfig.FirstOrDefaultAsync();
        }

    }
}
