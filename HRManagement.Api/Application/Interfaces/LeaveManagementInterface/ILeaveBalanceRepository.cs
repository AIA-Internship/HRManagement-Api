using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveBalance;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveResponse;

namespace HRManagement.Api.Application.Interfaces.LeaveManagementInterface
{
    public interface ILeaveBalanceRepository
    {
        public Task<LeaveBalanceModel> getLeaveBalanceById(int id);
        public Task<bool> createLeaveBalance(LeaveBalanceModel leaveRequest);
        public Task<bool> updateLeaveBalance(LeaveBalanceModel leaveRequest);
        public Task<bool> deleteLeaveBalance(int id);
    }
}
