using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables
{
    [Table("LeaveTableConfig")]
    public class LeaveConfig
    {
        [Column("email")]
        public string Email { get; set; } = null!;
        [Column("password")]
        public string Password { get; set; } = null!;
        [Column("redirect_link")]
        public string RedirectLink { get; set; } = null!;
    }
}
