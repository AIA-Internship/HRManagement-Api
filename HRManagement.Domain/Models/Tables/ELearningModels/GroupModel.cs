using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningGroups")]
    public class GroupModel
    {
        [Key]
        [Column("group_id")]
        public int GroupId { get; set; }

        [Column("group_name")]
        public string GroupName { get; set; } = null!;
    }
}