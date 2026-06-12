namespace HRManagement.Application.EmployeeDtos.Commands.Dto;

public class ReviewUpdateRequestDto
{
    /// <summary>
    /// 1
    /// </summary>
    public int RequestId { get; set; }
    /// <summary>
    /// true
    /// </summary>
    public bool IsApproved { get; set; }
    /// <summary>
    /// It is inappropriate
    /// </summary>
    public string? Reason { get; set; }
}
