namespace HRManagement.Domain.Models.Tables;

public class Lookup
{
    public int Id { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public int Value { get; private set; } = 0;
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public Lookup() { }
}