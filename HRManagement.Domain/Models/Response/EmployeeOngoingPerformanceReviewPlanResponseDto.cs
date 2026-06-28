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

        public List<FillAssignmentResponseDto> Assignments { get; set; } = new();
    }
}