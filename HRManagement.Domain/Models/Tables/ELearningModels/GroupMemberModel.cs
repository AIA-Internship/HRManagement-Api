using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningGroupMembers")]
    public class GroupMemberModel
    {
        [Column("group_id")]
        public int GroupId { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; } 
    }
}