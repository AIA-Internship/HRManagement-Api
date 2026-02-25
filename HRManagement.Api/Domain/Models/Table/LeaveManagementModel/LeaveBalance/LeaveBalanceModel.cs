using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveBalance
{
    
    public class LeaveBalanceModel
    {
        [Key]
        [Column("employee_id")]
        public int EmployeeId { get; set; }
        [Column("leave_balance")]
        public float LeaveBalance { get; set; }
    }
}
