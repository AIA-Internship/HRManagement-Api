using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Response
{
    public record AssessmentResponseDto
    (
        int Id,
        int PlanId,
        string AnswerType,
        string AssessmentType,
        int? FillerRoleId,
        string? FillerJobTitle,
        int? SubjectRoleId,
        string? SubjectJobTitle,
        string? RatingDescription,
        List<AssessmentQuestionResponseDto> Questions
    );

    public class AssessmentBriefResponseDto
    {
        public int Id { get; set; }
        public string AnswerType { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
        public int? FillerRoleId { get; set; }
        public string? FillerJobTitle { get; set; }
        public int? SubjectRoleId { get; set; }
        public string? SubjectJobTitle { get; set; }
    }
}
