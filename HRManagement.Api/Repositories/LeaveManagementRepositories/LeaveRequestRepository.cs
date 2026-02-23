using HRManagement.Api.Domain.Interfaces.NewFolder;
using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

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
            try
            {

                var request = await _dbContext.LeaveRequest.FirstOrDefaultAsync(leave => leave.leaveId == id && leave.IsDeleted == false);
                var leaveRequest = new LeaveRequestModel
                {
                    leaveId = request.leaveId,
                    requesterId = request.requesterId,
                    supervisorId = request.supervisorId,
                    leaveDescription = request.leaveDescription,
                    leaveStatus = request.leaveStatus,
                    leaveStartDate = request.leaveStartDate,
                    dayAmount = request.dayAmount,
                    leaveType = request.leaveType,
                    initialRequestDate = request.initialRequestDate,
                    lastUpdated = request.lastUpdated,
                    isDeleted = request.isDeleted,
                    isCompleted = request.isCompleted,
                    isEdit = request.isEdit,
                    initialRequestId = request.initialRequestId,
                    attachmentPath = request.attachmentPath
                };

                return leaveRequest;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            
        }

        public Task<List<LeaveRequestModel>> getLeaveRequestsByRequesterId(int requesterId)
        {
            throw new NotImplementedException();
        }

        public Task updateLeaveRequest(int requestId, LeaveRequestModel leaveRequest)
        {
            throw new NotImplementedException();
        }
    }
}
