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
}
