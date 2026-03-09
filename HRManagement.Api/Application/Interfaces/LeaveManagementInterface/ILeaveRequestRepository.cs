using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;

namespace HRManagement.Api.Application.Interfaces.LeaveManagementInterface
{
    public interface ILeaveRequestRepository
    {
        public Task<LeaveRequestModel> getLeaveRequestById(int id);
        public Task<List<LeaveRequestModel>> getLeaveRequestsByRequesterId(int requesterId, int max);
        public Task<bool> createLeaveRequest(LeaveRequestModel leaveRequest);
        public Task<bool> updateLeaveRequest(LeaveRequestModel leaveRequest);
        public Task<bool> softDelete(int id);
        public Task<List<LeaveRequestModel>> getLeaveRequestByMonthRage(int year, int month);
    }
}
