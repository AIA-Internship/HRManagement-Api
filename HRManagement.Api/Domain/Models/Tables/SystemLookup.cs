namespace HRManagement.Api.Domain.Models.Tables;

public class SystemLookup
{
    public int Id { get; set; }
    public string Category { get; set; }
    public int Value { get; set; }
    public string DisplayName { get; set; }
    public bool IsActive { get; set; } = true;
}