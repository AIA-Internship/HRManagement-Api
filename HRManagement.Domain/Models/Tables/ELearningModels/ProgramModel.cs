using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningPrograms")]
    public class ProgramModel
    {
        [Key]
        [Column("program_id")]
        public int ProgramId { get; set; }

        [Column("program_name")]
        public string ProgramName { get; set; } = null!;

        [Column("group_id")]
        public int GroupId { get; set; }
    }
}