using HRManagement.Domain.Models.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningModuleProgress")]
    public class ProgressModel : BaseTableModel
    {
        [Column("progress_id")]
        public int ProgressId { get; set; }

        [Column("employee_id")] 
        public int EmployeeId { get; set; }

        [Column("module_id")] 
        public int ModuleId { get; set; }

        [Column("progress_status")]
        public string ProgressStatus { get; set; } = "Not Started"; 

        [Column("completed_utc_date")]
        public System.DateTime? CompletedUtcDate { get; set; }
    }
}