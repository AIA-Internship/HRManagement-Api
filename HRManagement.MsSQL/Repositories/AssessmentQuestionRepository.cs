using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Repositories;

public class AssessmentQuestionRepository
    : BaseRepository<AssessmentQuestion>, IAssessmentQuestionRepository
{
    public AssessmentQuestionRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<List<AssessmentQuestionResponseDto>> GetByAssessmentIdAsync(
        int assessmentId,
        CancellationToken cancellationToken)
    {
        return await _sqldbContext.AssessmentQuestions
            .Where(x => x.AssessmentId == assessmentId && !x.IsDeleted)
            .OrderBy(x => x.QuestionOrder)
            .Select(x => new AssessmentQuestionResponseDto(
                x.Id,
                x.AssessmentId,
                x.QuestionText,
                x.QuestionOrder
            ))
            .ToListAsync(cancellationToken);
    }
}