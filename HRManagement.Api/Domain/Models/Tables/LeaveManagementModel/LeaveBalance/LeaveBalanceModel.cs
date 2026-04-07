using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveBalance
{
    [Table("LeaveBalance")]
    public class LeaveBalanceModel
    {
        [Key ]
        [Column("employee_id")]
        public int EmployeeId { get; set; }
        [Column("leave_balance")]
        public decimal LeaveBalance { get; set; }

    }
}
