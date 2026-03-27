namespace HRManagement.Api.Application.EmployeeDtos.Queries.Dto;

public class SupervisorLookupDto
{
    public string DisplayId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public SupervisorLookupDto() { }
    public SupervisorLookupDto(string displayId, string name)
    {
        DisplayId = displayId;
        Name = name;
    }
}
