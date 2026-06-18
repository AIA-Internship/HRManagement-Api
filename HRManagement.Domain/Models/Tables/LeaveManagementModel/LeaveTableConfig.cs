using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Tables.LeaveManagementModel
{
    [Keyless]
    public partial class LeaveTableConfig
    {
        [Column("email")]
        public string email { get; set; }

        [Column("password")]
        public string password { get; set; }

        [Column("redirect_link")]
        public string redirect_link { get; set; }

    }

}
