using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels
{
    [Table("ELearningPrograms")]
    public class ProgramModel
    {
        [Column("program_id")]
        public int ProgramId { get; set; }

        [Column("program_name")]
        public string ProgramName { get; set; } = null!;

        [Column("group_id")]
        public int GroupId { get; set; }
    }
}