using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Net.NetworkInformation;
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
            .Include(x => x.PerformanceReviewPlanScoreWeights)
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

        var scoreWeights = plan.PerformanceReviewPlanScoreWeights
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.SubjectJobTitle)
            .Select(g => new PerformanceReviewPlanScoreWeightResponseDto(
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

    public async Task<List<PerformanceReviewPlanScoreWeightResponseDto>> GetScoreWeightConfigurationsAsync(
        int planId,
        CancellationToken cancellationToken)
    {
        return await _sqldbContext.PerformanceReviewPlanScoreWeights
            .AsNoTracking()
            .Where(x =>
                x.PlanId == planId &&
                !x.IsDeleted)
            .GroupBy(x => x.SubjectJobTitle)
            .Select(g => new PerformanceReviewPlanScoreWeightResponseDto(
                g.Key ?? "",
                g.Select(x => new ScoreWeightItemDto(
                    x.ScoreType,
                    x.Weights
                )).ToList()
            ))
            .OrderBy(x => x.JobTitle)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeOngoingPerformanceReviewPlanResponseDto?> GetEmployeeOngoingPerformanceReviewPlanAsync(int fillerId, CancellationToken cancellationToken)
    {
        var currentDate = DateTime.UtcNow.Date;

        var planDetail = await _sqldbContext.PerformanceReviewPlans
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                     && p.Status == "ongoing"
                     && currentDate >= p.StartDate
                     && currentDate <= p.EndDate)
            .Select(p => new EmployeeOngoingPerformanceReviewPlanResponseDto
            {
                PlanId = p.Id,
                Name = p.Name,
                Status = p.Status,
                PeriodType = p.PeriodType,
                StartDate = p.StartDate,
                EndDate = p.EndDate,

                Assignments = _sqldbContext.FillAssignments
                    .Where(fa => !fa.IsDeleted
                              && fa.PlanId == p.Id
                              && fa.FillerId == fillerId)
                    .Select(fa => new FillAssignmentResponseDto
                    {
                        AssignmentId = fa.Id,
                        IntervalId = fa.IntervalId,
                        SubjectId = fa.SubjectId,
                        AssessmentId = fa.AssessmentId,
                        Status = fa.Status,

                        // NESTED INTERVAL INFORMATION
                        // Correlates the assignment's IntervalId back to the Plan's concrete Interval details
                        Interval = p.Intervals
                            .Where(i => !i.IsDeleted && i.Id == fa.IntervalId)
                            .Select(i => new PerformanceReviewPlanIntervalResponseDto(
                                i.Id,
                                i.PlanId,
                                i.IntervalNumber,
                                i.StartDate,
                                i.DueDate,
                                i.EndDate,
                                i.Status
                            ))
                            .FirstOrDefault(),

                        Assessment = _sqldbContext.Assessments
                            .Where(a => !a.IsDeleted && a.Id == fa.AssessmentId)
                            .Select(a => new AssessmentBriefResponseDto
                            {
                                Id = a.Id,
                                AnswerType = a.AnswerType,
                                AssessmentType = a.AssessmentType,
                                FillerRoleId = a.FillerRoleId,
                                FillerJobTitle = a.FillerJobTitle,
                                SubjectRoleId = a.SubjectRoleId,
                                SubjectJobTitle = a.SubjectJobTitle
                            })
                            .FirstOrDefault()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return planDetail;
    }
}