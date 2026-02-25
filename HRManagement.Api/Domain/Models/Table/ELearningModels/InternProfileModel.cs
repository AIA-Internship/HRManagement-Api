using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels
{
    [Table("InternProfiles")] 
    public class InternProfileModel : BaseTableModel
    {
        [Key]
        [Column("profile_id")] 
        public int ProfileId { get; set; }

        [Column("user_id")] 
        public int UserId { get; set; }

        [Column("intern_role")] 
        public string InternRole { get; set; } = null!;

        [Column("intern_team")] 
        public string InternTeam { get; set; } = null!;
    }
}
