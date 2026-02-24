using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;

namespace HRManagement.Api.Domain.Interfaces.NewFolder
{
    public interface ILeaveRequestRepository
    {
        public Task<LeaveRequestModel> getLeaveRequestById(int id);
        public Task<List<LeaveRequestModel>> getLeaveRequestsByRequesterId(int requesterId);
        public Task<bool> createLeaveRequest(LeaveRequestModel leaveRequest);
        public Task<bool> updateLeaveRequest(LeaveRequestModel leaveRequest);
    }
}
