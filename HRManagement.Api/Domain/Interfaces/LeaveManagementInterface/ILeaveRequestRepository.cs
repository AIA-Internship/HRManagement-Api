using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;

namespace HRManagement.Api.Domain.Interfaces.NewFolder
{
    public interface ILeaveRequestRepository
    {
        //Read operartions
        public Task<LeaveRequestModel> getLeaveRequestById(int id);
        public Task<List<LeaveRequestModel>> getLeaveRequestsByRequesterId(int requesterId);


        //Create operations
        public Task<bool> createLeaveRequest(LeaveRequestModel leaveRequest);

        //Update operations
        public Task<bool> updateLeaveRequest(int requestId, LeaveRequestModel leaveRequest);
    }
}
