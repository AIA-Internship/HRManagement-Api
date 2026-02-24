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
           return  await _dbContext.LeaveRequest
                .FirstOrDefaultAsync(x => x.LeaveId == id && x.IsDeleted == 0);

        }

        public async Task<List<LeaveRequestModel>> getLeaveRequestsByRequesterId(int requesterId)
        {
            return await _dbContext.LeaveRequest
            .Where(x => x.RequesterId == requesterId && x.IsDeleted == 0)
            .ToListAsync();
        }

        public async Task<bool> updateLeaveRequest(LeaveRequestModel data)
        {
             _dbContext.LeaveRequest.Update(data);

            return true;
        }

    }
}
