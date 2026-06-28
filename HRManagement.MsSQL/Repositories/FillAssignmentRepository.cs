using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Repositories
{
    public class FillAssignmentRepository
    : BaseRepository<FillAssignment>, IFillAssignmentRepository
    {
        public FillAssignmentRepository(AppDbContext dbContext)
            : base(dbContext) { }

        public async Task<FillAssignmentDetailResponseDto?> GetAssignmentDetailByIdAsync(int assignmentId, CancellationToken cancellationToken)
        {
            return await _sqldbContext.FillAssignments
                .AsNoTracking()
                .Where(fa => !fa.IsDeleted && fa.Id == assignmentId)
                .Select(fa => new FillAssignmentDetailResponseDto
                {
                    AssignmentId = fa.Id,
                    PlanId = fa.PlanId,
                    IntervalId = fa.IntervalId,
                    FillerId = fa.FillerId,
                    SubjectId = fa.SubjectId,
                    AssessmentId = fa.AssessmentId,
                    Status = fa.Status,

                    Assessment = _sqldbContext.Assessments
                        .Where(a => !a.IsDeleted && a.Id == fa.AssessmentId)
                        .Select(a => new AssessmentDetailResponseDto
                        {
                            Id = a.Id,
                            AnswerType = a.AnswerType,
                            AssessmentType = a.AssessmentType,
                            FillerRoleId = a.FillerRoleId,
                            FillerJobTitle = a.FillerJobTitle,
                            SubjectRoleId = a.SubjectRoleId,
                            SubjectJobTitle = a.SubjectJobTitle,

                            Questions = _sqldbContext.AssessmentQuestions
                                .Where(q => !q.IsDeleted && q.AssessmentId == a.Id)
                                .OrderBy(q => q.QuestionOrder)
                                .Select(q => new AssessmentQuestionResponseDto
                                (
                                    q.Id,
                                    q.AssessmentId,
                                    q.QuestionText,
                                    q.QuestionOrder,
                                    q.QuestionType,

                                    _sqldbContext.AssessmentAnswers
                                        .Where(ans => !ans.IsDeleted
                                                   && ans.AssignmentId == fa.Id
                                                   && ans.AssessmentQuestionId == q.Id)
                                        .Select(ans => new AssessmentAnswerResponseDto
                                        {
                                            Id = ans.Id,
                                            TextValue = ans.TextValue,
                                            RatingValue = ans.RatingValue
                                        })
                                        .FirstOrDefault()
                                ))
                                .ToList()
                        })
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}