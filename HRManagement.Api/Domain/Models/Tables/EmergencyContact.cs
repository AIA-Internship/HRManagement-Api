using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Tables;

public class EmergencyContact : BaseTableModel
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    
    // Relationship status: Logic-only (No navigation property to prevent physical FK creation).
    [NotMapped]
    public Employee? Employee { get; set; }
    
    public string Name { get; set; } =  string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
}