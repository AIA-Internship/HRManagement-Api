namespace HRManagement.Api.Application.EmployeeDtos.Commands.Dto;

public class ReviewUpdateRequestDto
{
    public int RequestId { get; set; }
    public bool IsApproved { get; set; }
    public string? Reason { get; set; }
}
