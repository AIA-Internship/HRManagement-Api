using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table
{
    [Table("EmploymentInformation")]
    [Keyless]
    public class EmploymentInformationModel
    {
        public int EmployeeId { get; set; }
        public string? PositionName { get; set; }
    }
}
