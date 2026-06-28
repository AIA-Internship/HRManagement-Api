using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Response
{
    public class FillAssignmentDetailResponseDto
    {
        public int AssignmentId { get; set; }
        public int PlanId { get; set; }
        public int IntervalId { get; set; }
        public int FillerId { get; set; }
        public int SubjectId { get; set; }
        public int AssessmentId { get; set; }
        public string Status { get; set; } = string.Empty;

        public PerformanceReviewPlanIntervalResponseDto? Interval { get; set; }
        public AssessmentDetailResponseDto? Assessment { get; set; }
    }

    public class AssessmentDetailResponseDto
    {
        public int Id { get; set; }
        public string AnswerType { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
        public int? FillerRoleId { get; set; }
        public string? FillerJobTitle { get; set; }
        public int? SubjectRoleId { get; set; }
        public string? SubjectJobTitle { get; set; }

        public List<AssessmentQuestionResponseDto> Questions { get; set; } = new();
    }

    public class AssessmentAnswerResponseDto
    {
        public int Id { get; set; }
        public string? TextValue { get; set; }
        public int? RatingValue { get; set; }
    }
}
