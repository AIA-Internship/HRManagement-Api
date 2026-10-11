namespace HRManagement.Domain.Models.Response;

public class PeerReviewMatrixResponseDto
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int IntervalId { get; set; }

    // 👉 Add these two flags to give you full control
    public bool IsAllCompleted { get; set; }
    public bool IsSubmitted { get; set; }   // True only if everything is finalized/submitted

    public List<QuestionHeaderDto> Questions { get; set; } = [];
    public List<PeerReviewerAnswersDto> Reviewers { get; set; } = [];
}

public class QuestionHeaderDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int QuestionOrder { get; set; }
    public string? QuestionType { get; set; }
}

public class PeerReviewerAnswersDto
{
    public int AssignmentId { get; set; }
    public int FillerId { get; set; }
    public string FillerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<AnswerDetailDto> Answers { get; set; } = new();
}

public class AnswerDetailDto
{
    public int AnswerId { get; set; }
    public int QuestionId { get; set; }
    public string? TextValue { get; set; }
    public int? RatingValue { get; set; }
    public decimal? ReviewScore { get; set; }
}