using HRManagement.Api.Domain.Interfaces.NewFolder;
using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

namespace HRManagement.Api.Repositories.LeaveManagementRepositories
{
    public class LeaveRequestRepository : BaseRepository, ILeaveRequestRepository
    {
        private readonly SqlDbContext _dbContext;
        public LeaveRequestRepository(SqlDbContext dbContext) : base(dbContext)
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
                 .FirstOrDefaultAsync(x => x.LeaveId == id && x.IsDeleted == 0 && x.IsEdit == 0);

        }

        public async Task<List<LeaveRequestModel>> getLeaveRequestsByRequesterId(int requesterId, int max)
        {
            return await _dbContext.LeaveRequest
            .Where(x => x.RequesterId == requesterId && x.IsDeleted == 0 && x.IsEdit == 0)
            .OrderByDescending(x => x.CreatedUtcDate)
            .Take(max)
            .ToListAsync();
        }

        public async Task<bool> updateLeaveRequest(LeaveRequestModel data)
        {
            _dbContext.LeaveRequest.Update(data);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<List<LeaveRequestModel>> getAllEditById(int leaveId)
        {
            return await _dbContext.LeaveRequest
                .Where(x => x.InitialRequestId == leaveId && x.IsDeleted == 0 && x.IsEdit == 0)
                .ToListAsync();
        }


        public async Task<bool> softDelete(int id)
        {
            var entity = await _dbContext.LeaveRequest.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = 1;

            return await _dbContext.SaveChangesAsync() > 0;
        }

        //public async Task<List<LeaveRequestModel>> getLeaveRequestByMonthRage(int year, int month)
        //{
        //    var startDate = new DateTime(year, month, 1);
        //    var endDate = startDate.AddMonths(1);

        //    return await _dbContext.LeaveRequest
        //        .Where(x => x.LeaveStartDate >= startDate
        //            && x.LeaveStartDate < endDate
        //            && x.IsDeleted == 0
        //            )
        //        .ToListAsync();
        //}
    }
}
