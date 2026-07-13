using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels
{
    [Table("ELearningGroupMembers")]
    [Keyless]
    public class GroupMemberModel
    {
        [Column("group_id")]
        public int GroupId { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; } 
    }
}