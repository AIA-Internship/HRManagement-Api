using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveResponse;

namespace HRManagement.Api.Application.Interfaces.LeaveManagementInterface
{
    public interface ILeaveRequestRepository
    {
        public Task<LeaveRequestModel> getLeaveRequestById(int id);
        public Task<List<LeaveRequestModel>> getLeaveRequestsByRequesterId(int requesterId, int max);
        public Task<bool> createLeaveRequest(LeaveRequestModel leaveRequest);
        public Task<bool> updateLeaveRequest(LeaveRequestModel leaveRequest);
        public Task<bool> softDelete(int id);
        public Task<List<GetLeaveRequestByMonthRangeDto>> getLeaveRequestByMonthRage(int year, int month);
        //public Task<LeaveDetailDto> getLeaveDetailById(int leaveId);

        public  Task<List<LeaveRequestHistory>> getAllEditById(int leaveId);
        public  Task<bool> createLeaveRequestHistory(LeaveRequestHistory data);
        public Task<LeaveTableCOnfig> getLeaveTableCOnfig();

    }
}
