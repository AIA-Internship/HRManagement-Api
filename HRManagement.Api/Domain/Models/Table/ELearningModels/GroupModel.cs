using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels
{
    [Table("ELearningGroups")]
    public class GroupModel
    {
        [Column("group_id")]
        public int GroupId { get; set; }

        [Column("group_name")]
        public string GroupName { get; set; } = null!;
    }
}