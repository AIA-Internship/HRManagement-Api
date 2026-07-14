using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningBatches")]
    public class BatchModel
    {
        [Key]
        [Column("batch_id")]
        public int BatchId { get; set; }

        [Column("program_id")]
        public int ProgramId { get; set; }

        [Column("batch_name")]
        public string BatchName { get; set; } = null!;

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }
    }
}