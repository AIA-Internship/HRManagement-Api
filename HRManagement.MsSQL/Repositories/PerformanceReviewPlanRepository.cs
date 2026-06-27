using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace HRManagement.MsSQL.Repositories;

public class PerformanceReviewPlanRepository : BaseRepository<PerformanceReviewPlan>, IPerformanceReviewPlanRepository
{
    public PerformanceReviewPlanRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<PerformanceReviewPlanDetailResponseDto?> GetPlanByIdAsync(int planId, CancellationToken cancellationToken = default)
    {
        var plan = await _sqldbContext.PerformanceReviewPlans
            .AsNoTracking()
            .Include(x => x.Assessments)
                .ThenInclude(x => x.Questions)
            .Include(x => x.Assessments)
                .ThenInclude(x => x.Groups)
                    .ThenInclude(x => x.Members)
                        .ThenInclude(x => x.Employee)
                            .ThenInclude(x => x.EmploymentInformation)
            .Include(x => x.PlanScoreWeights)
            .FirstOrDefaultAsync(
                x => x.Id == planId && !x.IsDeleted,
                cancellationToken);

        if (plan is null)
            return null;

        var selfAssessments = plan.Assessments
            .Where(x => x.AssessmentType == "self-assessment")
            .Select(x => new SelfAssessmentDto(
                x.Id,
                x.SubjectJobTitle ?? "",
                x.AnswerType,
                x.RatingDescription,

                x.Questions
                    .Where(q => !q.IsDeleted)
                    .OrderBy(q => q.QuestionOrder)
                    .Select(q => new AssessmentQuestionResponseDto(
                        q.Id,
                        q.AssessmentId,
                        q.QuestionText,
                        q.QuestionOrder,
                        q.QuestionType
                    ))
                    .ToList(),

                _sqldbContext.Set<EmploymentInformation>()
                    .Where(e =>
                        !e.IsDeleted &&
                        e.StatusCode == 1 &&
                        e.PositionName == x.SubjectJobTitle)
                    .Select(e => new EmployeeListResponseDto(
                        e.Employee.FullName,
                        e.DisplayId ?? "",
                        "",
                        e.DepartmentName ?? "",
                        e.PositionName ?? ""
                    ))
                    .ToList()
            ))
            .ToList();

        var peerReviews = plan.Assessments
            .Where(x => x.AssessmentType == "peer-review")
            .Select(x => new PeerReviewDto(
                x.Id,
                x.SubjectJobTitle ?? "",
                x.AnswerType,
                x.RatingDescription,

                x.Questions
                    .Where(q => !q.IsDeleted)
                    .OrderBy(q => q.QuestionOrder)
                    .Select(q => new AssessmentQuestionResponseDto(
                        q.Id,
                        q.AssessmentId,
                        q.QuestionText,
                        q.QuestionOrder,
                        q.QuestionType
                    ))
                    .ToList(),

                x.Groups
                    .Where(g => !g.IsDeleted)
                    .Select(g => new AssessmentGroupDto(
                        g.Id,
                        g.Name,
                        g.Description ?? "",

                        g.Members
                            .Where(m => !m.IsDeleted)
                            .Select(m => new AssessmentGroupMemberDto(
                                m.EmployeeId,
                                m.Employee.EmploymentInformation?.DisplayId ?? "",
                                m.Employee.FullName
                            ))
                            .ToList()
                    ))
                    .ToList()
            ))
            .ToList();

        var supervisorAssessments = plan.Assessments
            .Where(x => x.AssessmentType == "supervisor-assessment")
            .Select(x => new SupervisorAssessmentDto(
                x.Id,
                x.SubjectJobTitle ?? "",
                x.AnswerType,
                x.RatingDescription,

                x.Questions
                    .Where(q => !q.IsDeleted)
                    .OrderBy(q => q.QuestionOrder)
                    .Select(q => new AssessmentQuestionResponseDto(
                        q.Id,
                        q.AssessmentId,
                        q.QuestionText,
                        q.QuestionOrder,
                        q.QuestionType
                    ))
                    .ToList(),

                _sqldbContext.Set<EmploymentInformation>()
                    .Where(e =>
                        !e.IsDeleted &&
                        e.StatusCode == 1 &&
                        e.PositionName == "Supervisor")
                    .Select(e => new EmployeeListResponseDto(
                        e.Employee.FullName,
                        e.DisplayId ?? "",
                        "",
                        e.DepartmentName ?? "",
                        e.PositionName ?? ""
                    ))
                    .ToList()
            ))
            .ToList();

        var scoreWeights = plan.PlanScoreWeights
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.SubjectJobTitle)
            .Select(g => new PlanScoreWeightResponseDto(
                g.Key ?? "",
                g.Select(x => new ScoreWeightItemDto(
                    x.ScoreType,
                    x.Weights
                ))
                .ToList()
            ))
            .ToList();

        return new PerformanceReviewPlanDetailResponseDto(
            plan.Id,
            plan.Name,
            plan.PeriodType,
            plan.StartDate,
            plan.EndDate,
            plan.MinReviewDurationInDays,
            plan.DurationInMonth,
            plan.Status,

            selfAssessments,
            peerReviews,
            supervisorAssessments,
            scoreWeights
        );
    }

    public async Task<List<PerformanceReviewPlanResponseDto>> GetAllPlansAsync(CancellationToken cancellationToken = default)
    {
        return await _sqldbContext.PerformanceReviewPlans
            .Where(x => !x.IsDeleted)
            .Select(x => new PerformanceReviewPlanResponseDto(
                x.Id,
                x.Name,
                x.PeriodType,
                x.StartDate,
                x.EndDate,
                x.MinReviewDurationInDays,
                x.DurationInMonth,
                x.Status
            ))
            .ToListAsync(cancellationToken);
    }



    public async Task<List<PlanScoreWeightResponseDto>> GetByPlanIdAndJobTitleAsync(
    int planId,
    string? jobTitle,
    CancellationToken cancellationToken)
    {
        return await _sqldbContext.PlanScoreWeights
            .Where(x =>
                x.PlanId == planId &&
                !x.IsDeleted &&
                (string.IsNullOrWhiteSpace(jobTitle) || x.SubjectJobTitle == jobTitle)
            )
            .GroupBy(x => x.SubjectJobTitle)
            .Select(g => new PlanScoreWeightResponseDto(
                g.Key!,
                g.Select(x => new ScoreWeightItemDto(
                    x.ScoreType,
                    x.Weights
                )).ToList()
            ))
            .ToListAsync(cancellationToken);
    }


    public async Task<List<PlanScoreWeightResponseDto>> GetScoreWeightConfigurationsAsync(
        int planId,
        CancellationToken cancellationToken)
    {
        return await _sqldbContext.PlanScoreWeights
            .AsNoTracking()
            .Where(x =>
                x.PlanId == planId &&
                !x.IsDeleted)
            .GroupBy(x => x.SubjectJobTitle)
            .Select(g => new PlanScoreWeightResponseDto(
                g.Key ?? "",
                g.Select(x => new ScoreWeightItemDto(
                    x.ScoreType,
                    x.Weights
                ))
                .ToList()
            ))
            .OrderBy(x => x.JobTitle)
            .ToListAsync(cancellationToken);
    }
}