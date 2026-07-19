using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningContentProgress")]
    public class ContentProgressModel
    {
        [Key]
        [Column("content_progress_id")]
        public int ContentProgressId { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("content_id")]
        public int ContentId { get; set; }

        [Column("opened_utc_date")]
        public DateTime OpenedUtcDate { get; set; }
    }
}
