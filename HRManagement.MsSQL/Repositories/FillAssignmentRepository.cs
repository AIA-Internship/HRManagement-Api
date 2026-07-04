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

                    Assessment = fa.Assessment != null && !fa.Assessment.IsDeleted ? new AssessmentDetailResponseDto
                    {
                        Id = fa.Assessment.Id,
                        AnswerType = fa.Assessment.AnswerType,
                        AssessmentType = fa.Assessment.AssessmentType,
                        FillerRoleId = fa.Assessment.FillerRoleId,
                        FillerJobTitle = fa.Assessment.FillerJobTitle,
                        SubjectRoleId = fa.Assessment.SubjectRoleId,
                        SubjectJobTitle = fa.Assessment.SubjectJobTitle,

                        Questions = fa.Assessment.Questions
                            .Where(q => !q.IsDeleted)
                            .OrderBy(q => q.QuestionOrder)
                            .Select(q => new AssessmentQuestionResponseDto
                            (
                                q.Id,
                                q.AssessmentId,
                                q.QuestionText,
                                q.QuestionOrder,
                                q.QuestionType,

                                q.Answers
                                    .Where(ans => !ans.IsDeleted && ans.AssignmentId == fa.Id)
                                    .Select(ans => new AssessmentAnswerResponseDto
                                    {
                                        Id = ans.Id,
                                        TextValue = ans.TextValue,
                                        RatingValue = ans.RatingValue
                                    })
                                    .FirstOrDefault()
                            ))
                            .ToList()
                    } : null
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        //public async Task<List<FillAssignmentDetailResponseDto>> GetPeerAssignmentDetailsByIntervalAsync(
        //int fillerId,
        //int intervalId,
        //CancellationToken cancellationToken)
        //{
        //    return await _sqldbContext.FillAssignments
        //        .AsNoTracking()
        //        .Where(fa => !fa.IsDeleted
        //                  && fa.FillerId == fillerId
        //                  && fa.IntervalId == intervalId
        //                  && fa.Assessment.AssessmentType == "peer-review")
        //        .Select(fa => new FillAssignmentDetailResponseDto
        //        {
        //            AssignmentId = fa.Id,
        //            PlanId = fa.PlanId,
        //            IntervalId = fa.IntervalId,
        //            FillerId = fa.FillerId,
        //            SubjectId = fa.SubjectId,
        //            AssessmentId = fa.AssessmentId,
        //            Status = fa.Status,

        //            Subject = fa.Subject != null && !fa.Subject.IsDeleted ? new EmployeeMinimalInfoResponseDto
        //            {
        //                Id = fa.Subject.Id,
        //                FullName = fa.Subject.FullName
        //            } : null,

        //            Assessment = fa.Assessment != null && !fa.Assessment.IsDeleted ? new AssessmentDetailResponseDto
        //            {
        //                Id = fa.Assessment.Id,
        //                AnswerType = fa.Assessment.AnswerType,
        //                AssessmentType = fa.Assessment.AssessmentType,
        //                FillerRoleId = fa.Assessment.FillerRoleId,
        //                FillerJobTitle = fa.Assessment.FillerJobTitle,
        //                SubjectRoleId = fa.Assessment.SubjectRoleId,
        //                SubjectJobTitle = fa.Assessment.SubjectJobTitle,

        //                Questions = fa.Assessment.Questions
        //                    .Where(q => !q.IsDeleted)
        //                    .OrderBy(q => q.QuestionOrder)
        //                    .Select(q => new AssessmentQuestionResponseDto
        //                    (
        //                        q.Id,
        //                        q.AssessmentId,
        //                        q.QuestionText,
        //                        q.QuestionOrder,
        //                        q.QuestionType,

        //                        q.Answers
        //                            .Where(ans => !ans.IsDeleted && ans.AssignmentId == fa.Id)
        //                            .Select(ans => new AssessmentAnswerResponseDto
        //                            {
        //                                Id = ans.Id,
        //                                TextValue = ans.TextValue,
        //                                RatingValue = ans.RatingValue
        //                            })
        //                            .FirstOrDefault()
        //                    ))
        //                    .ToList()
        //            } : null
        //        })
        //        .ToListAsync(cancellationToken);
        //}

        public async Task<List<FillAssignmentDetailResponseDto>> GetPeerAssignmentDetailsByIntervalAsync(int fillerId, int intervalId, CancellationToken cancellationToken)
        {
            // Prevent CS1998 warning for lack of await keywords in temporary mock
            await Task.CompletedTask;

            return new List<FillAssignmentDetailResponseDto>
            {
                // 1. Owen Doe
                new FillAssignmentDetailResponseDto
                {
                    AssignmentId = 8,
                    PlanId = 2,
                    IntervalId = intervalId,
                    FillerId = fillerId,
                    SubjectId = 13,
                    AssessmentId = 5,
                    Status = "not started",
                    Subject = new EmployeeMinimalInfoResponseDto
                    {
                        Id = 13,
                        FullName = "Owen Doe"
                    },
                    Assessment = GetMockAssessment(5, "text") // Change to "rating" to test the 1-5 layout view
                },

                // 2. Jane Doe
                new FillAssignmentDetailResponseDto
                {
                    AssignmentId = 9,
                    PlanId = 2,
                    IntervalId = intervalId,
                    FillerId = fillerId,
                    SubjectId = 14,
                    AssessmentId = 5,
                    Status = "not started",
                    Subject = new EmployeeMinimalInfoResponseDto
                    {
                        Id = 14,
                        FullName = "Jane Doe"
                    },
                    Assessment = GetMockAssessment(5, "text")
                },

                // 3. Kuru Doe
                new FillAssignmentDetailResponseDto
                {
                    AssignmentId = 10,
                    PlanId = 2,
                    IntervalId = intervalId,
                    FillerId = fillerId,
                    SubjectId = 15,
                    AssessmentId = 5,
                    Status = "not started",
                    Subject = new EmployeeMinimalInfoResponseDto
                    {
                        Id = 15,
                        FullName = "Kuru Doe"
                    },
                    Assessment = GetMockAssessment(5, "text")
                },

                // 4. John Doe
                new FillAssignmentDetailResponseDto
                {
                    AssignmentId = 11,
                    PlanId = 2,
                    IntervalId = intervalId,
                    FillerId = fillerId,
                    SubjectId = 16,
                    AssessmentId = 5,
                    Status = "not started",
                    Subject = new EmployeeMinimalInfoResponseDto
                    {
                        Id = 16,
                        FullName = "John Doe"
                    },
                    Assessment = GetMockAssessment(5, "text")
                }
            };
        }

        // Helper method to keep code clean and reuse the same questions across all 4 subjects
        private AssessmentDetailResponseDto GetMockAssessment(int assessmentId, string answerType)
        {
            return new AssessmentDetailResponseDto
            {
                Id = assessmentId,
                AnswerType = answerType,
                AssessmentType = "peer-review",
                FillerRoleId = 2,
                FillerJobTitle = null,
                SubjectRoleId = 2,
                SubjectJobTitle = null,
                Questions = new List<AssessmentQuestionResponseDto>
                {
                    new AssessmentQuestionResponseDto
                    (
                        11,
                        assessmentId,
                        "How well does this peer collaborate with cross-functional team members?",
                        1,
                        null,
                        null
                    ),
                    new AssessmentQuestionResponseDto
                    (
                        12,
                        assessmentId,
                        "Rate the code quality and technical rigor demonstrated by this peer.",
                        2,
                        null,
                        null
                    ),
                    new AssessmentQuestionResponseDto
                    (
                        12,
                        assessmentId,
                        "Rate the code quality rigor demonstrated by this peer.",
                        2,
                        null,
                        null
                    )
                }
            };
        }
    }
}