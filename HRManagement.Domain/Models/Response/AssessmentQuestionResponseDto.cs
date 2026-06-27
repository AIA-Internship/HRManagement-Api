using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Response
{
    public record AssessmentQuestionResponseDto
    (
        int Id,
        int AssessmentId,
        string QuestionText,
        int QuestionOrder,
        string QuestionType
    );
}
