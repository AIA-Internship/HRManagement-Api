namespace HRManagement.Application.Models.DTOs.AssessmentAnswerScores;

public record AssessmentAnswerScoreResponseDto(
    int Id,
    int AssessmentAnswerId,
    decimal Score,
    int ReviewerId
);