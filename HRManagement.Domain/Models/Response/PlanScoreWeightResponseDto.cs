namespace HRManagement.Domain.Models.Response;

public record PlanScoreWeightResponseDto
(
    string JobTitle,
    List<ScoreWeightItemDto> Scores
);

public record ScoreWeightItemDto
(
    string ScoreType,
    decimal Weight
);