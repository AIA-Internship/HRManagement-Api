using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables
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
