using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;

namespace HRManagement.Api.Application.Interfaces
{
    public interface ILeaveRepository
    {
        public Task<LeaveRequestModel> getLeaveRequestById(int id);
        public Task<List<LeaveRequestModel>> getLeaveRequestsByRequesterId(int requesterId, int max);
        public Task<int> createLeaveRequest(LeaveRequestModel leaveRequest);
        public Task<bool> updateLeaveRequest(LeaveRequestModel leaveRequest);
        public Task<bool> softDelete(int id);
        public Task<List<GetLeaveRequestByMonthRangeDto>> getLeaveRequestByMonthRage(int year, int month);
        public Task<List<LeaveRequestModel>> getLeaveRequestBySupervisorId(int supervisorId, int max);
        //public Task<LeaveDetailDto> getLeaveDetailById(int leaveId);

        

        public  Task<List<LeaveRequestHistory>> getAllEditById(int leaveId);
        public  Task<bool> createLeaveRequestHistory(LeaveRequestHistory data);
        public Task<LeaveTableConfig> getLeaveTableConfig();
        public Task<List<LeaveRequestModel>> getAllRequestNeedsReminder();
        public Task<bool> incrementAllEmployeeLeaveRequest();
        public Task<LeaveTypeCountDto> GetLeaveTypeCounts(int employeeId);


        public Task<LeaveBalanceModel> getLeaveBalanceById(int id);
        public Task<bool> createLeaveBalance(LeaveBalanceModel leaveRequest);
        public Task<bool> updateLeaveBalance(LeaveBalanceModel leaveRequest);
        public Task<bool> deleteLeaveBalance(int id);
        public Task AddLeaveAttachmentsAsync(List<LeaveAttachment> entities, CancellationToken ct);
        public Task<LeaveAttachment?> GetAttachmentByIdAsync(int attachmentId);
        public Task DeleteLeaveAttachmentByIdAsync(int attachmentId, CancellationToken ct);
        public Task<List<LeaveAttachment>> getLeaveAttachmentsByLeaveId(int leaveId);

    }
}
