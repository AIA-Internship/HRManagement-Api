using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables
{
    public partial class LeaveTableConfig
    {
        [Key]

        [Column("email")]
        public string email { get; set; }

        [Column("password")]
        public string password { get; set; }

        [Column("redirect_link")]
        public string redirect_link { get; set; }

    }

}
