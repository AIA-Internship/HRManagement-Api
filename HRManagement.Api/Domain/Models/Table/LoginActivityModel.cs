using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table
{
    [Table("LoginActivity")]
    public class LoginActivityModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string? UserMobile { get; set; }
        public string? Token { get; set; }
        public DateTime? RequestTime { get; set; }
        public DateTime? TokenExpireTime { get; set; }
        public bool? IsActive { get; set; }

        public LoginActivityModel() { }

        public LoginActivityModel(string? userMobile, string? token)
        {
            UserMobile = userMobile;
            Token = token;
            RequestTime = DateTime.UtcNow.AddHours(7);
            TokenExpireTime = DateTime.UtcNow.AddHours(7).AddMinutes(5);
            IsActive = true;
        }

        public void SetExpireToken()
        {
            IsActive = false;
        }
    }
}
