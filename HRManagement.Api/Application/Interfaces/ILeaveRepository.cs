using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;

namespace HRManagement.Api.Application.Interfaces
{
    public interface ILeaveRepository
    {
        public Task<LeaveRequestModel> getLeaveRequestById(int id);
        public Task<List<LeaveRequestModel>> getLeaveRequestsByRequesterId(int requesterId, int max);
        public Task<bool> createLeaveRequest(LeaveRequestModel leaveRequest);
        public Task<bool> updateLeaveRequest(LeaveRequestModel leaveRequest);
        public Task<bool> softDelete(int id);
        public Task<List<GetLeaveRequestByMonthRangeDto>> getLeaveRequestByMonthRage(int year, int month);
        public Task<LeaveConfig> getLeaveConfig();
        public Task<bool> incrementAllEmployeeLeaveRequest();
        public Task<List<LeaveRequestModel>> getAllRequestNeedsReminder();


    }
}
