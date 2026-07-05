namespace HRManagement.Domain.Models.Response
{
    public class EmployeeOngoingPerformanceReviewPlanResponseDto
    {
        public int PlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PeriodType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Changed from Assignments to Intervals
        public List<PerformanceReviewPlanIntervalResponseDto> Intervals { get; set; } = new();
    }

    public class PerformanceReviewPlanIntervalResponseDto
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public int IntervalNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;

        // Nested assignments inside the interval
        public List<FillAssignmentResponseDto> Assignments { get; set; } = new();
    }

    public class FillAssignmentResponseDto
    {
        public int AssignmentId { get; set; }
        public int SubjectId { get; set; }
        public int AssessmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public AssessmentBriefResponseDto? Assessment { get; set; }
    }
}