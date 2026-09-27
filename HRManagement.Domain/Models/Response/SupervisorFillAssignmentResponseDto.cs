namespace HRManagement.Domain.Models.Response;

public class SupervisorDashboardResponseDto
{
    public int? CurrentPlanId { get; set; }
    public int? CurrentIntervalId { get; set; }
    public List<DashboardDataDto> Assignments { get; set; } = new();
}

public record SupervisorFillAssignmentResponseDto
{
    public int PlanId { get; init; }
    public int IntervalId { get; init; }
    public int AssignmentId { get; init; }
    public int SubjectId { get; set; }
    public string AssessmentType { get; set; } = string.Empty;
    public string AnswerType { get; set; } = string.Empty;
    public int FillerId { get; set; }
    public string AssignmentStatus { get; set; } = string.Empty;
    public List<ReviewerResponseDto> Reviewers { get; set; } = new();
    public SubjectResponseDto Subject { get; set; } = null!;
}

public class SubjectResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? JobTitle { get; set; } = null!;
}

public record ReviewerResponseDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public class DashboardDataDto
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public AssessmentDetailDto? SupervisorAssessment { get; set; }
    public List<AssessmentDetailDto> PeerReviews { get; set; } = new();
    public AssessmentDetailDto? SelfAssessment { get; set; }
}

public class AssessmentDetailDto
{
    public int AssignmentId { get; set; }
    public string AnswerType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<ReviewerDto> Reviewers { get; set; } = new();
}

public class ReviewerDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
}