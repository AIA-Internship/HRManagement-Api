using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
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
            .Where(x => x.Id == planId && !x.IsDeleted)
            .Select(plan => new PerformanceReviewPlanDetailResponseDto(
                plan.Id,
                plan.Name,
                plan.PeriodType,
                plan.StartDate,
                plan.EndDate,
                plan.MinReviewDurationInDays,
                plan.DurationInMonth,
                plan.Status,

                // self assessments
                plan.Assessments
                    .Where(a => !a.IsDeleted && a.AssessmentType == "self-assessment")
                    .Select(a => new SelfAssessmentDto(
                        a.Id,
                        a.SubjectJobTitle ?? "",
                        a.AnswerType,
                        a.RatingDescription,

                        a.Questions
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

                            // Employee List
                            a.Receivers
                                .Where(r =>
                                    !r.IsDeleted &&
                                    r.ReceiverType == "self-assessment")
                                    .Select(r => new EmployeeListResponseDto(
                                        r.Employee.FullName,
                                        r.Employee.EmploymentInformation != null
                                            ? r.Employee.EmploymentInformation.DisplayId
                                            : "",
                                        "",
                                        r.Employee.EmploymentInformation != null
                                            ? r.Employee.EmploymentInformation.DepartmentName
                                            : "",
                                        r.Employee.EmploymentInformation != null
                                            ? r.Employee.EmploymentInformation.PositionName
                                            : ""
                                    ))
                                .ToList()
                    ))
                    .ToList(),

                // peer reviews
                plan.Assessments
                    .Where(a => a.AssessmentType == "peer-review" && !a.IsDeleted)
                    .Select(a => new PeerReviewDto(
                        a.Id,
                        a.SubjectJobTitle ?? "",
                        a.AnswerType,
                        a.RatingDescription,

                        // Questions
                        a.Questions
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

                        // Groups
                        a.Groups
                            .Where(g => !g.IsDeleted)
                            .Select(g => new AssessmentGroupDto(
                                g.Id,
                                g.Name,
                                g.Description ?? "",

                                // Members
                                g.Members
                                    .Where(m => !m.IsDeleted)
                                    .Select(m => new AssessmentGroupMemberDto(
                                        m.EmployeeId,

                                        // Employment Information
                                        m.Employee.EmploymentInformation != null
                                            ? m.Employee.EmploymentInformation.DisplayId ?? ""
                                            : "",

                                        m.Employee.FullName
                                    ))
                                    .ToList()
                            ))
                            .ToList()
                    ))
                    .ToList(),

                // supervisor
                plan.Assessments
                    .Where(a => !a.IsDeleted && a.AssessmentType == "supervisor-assessment")
                    .Select(a => new SupervisorAssessmentDto(
                        a.Id,
                        a.SubjectJobTitle ?? "",
                        a.AnswerType,
                        a.RatingDescription,

                        a.Questions
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

                            a.Receivers
                                .Where(r =>
                                    !r.IsDeleted &&
                                    r.ReceiverType == "supervisor-assessment")
                                    .Select(r => new EmployeeListResponseDto(
                                        r.Employee.FullName,
                                        r.Employee.EmploymentInformation != null
                                            ? r.Employee.EmploymentInformation.DisplayId
                                            : "",
                                        "",
                                        r.Employee.EmploymentInformation != null
                                            ? r.Employee.EmploymentInformation.DepartmentName
                                            : "",
                                        r.Employee.EmploymentInformation != null
                                            ? r.Employee.EmploymentInformation.PositionName
                                            : ""
                                    ))
                                .ToList()
                    ))
                    .ToList(),

                // score weight
                plan.PerformanceReviewPlanScoreWeights
                    .Where(sw => !sw.IsDeleted)
                    .GroupBy(sw => sw.SubjectJobTitle)
                    .Select(g => new PerformanceReviewPlanScoreWeightResponseDto(
                        g.Key ?? "",
                        g.Select(sw => new ScoreWeightItemDto(
                            sw.ScoreType,
                            sw.Weights
                        ))
                        .ToList()
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);


        return plan;

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

    public async Task AddPerformanceReviewPlan(CreatePerformanceReviewPlanPayload payload, int actionerId, CancellationToken cancellationToken)
    {
        var plan =
            new PerformanceReviewPlan(
                payload.Name,
                payload.PeriodType,
                payload.DurationInMonth,
                payload.MinReviewDurationInDays,
                payload.StartDate,
                payload.EndDate,
                payload.Status,
                actionerId
            );

        foreach (var assessmentPayload in payload.Assessments)
        {
            var assessment =
                new Assessment(
                    0,
                    assessmentPayload.AnswerType,
                    assessmentPayload.AssessmentType,
                    assessmentPayload.FillerRoleId,
                    assessmentPayload.FillerJobTitle,
                    assessmentPayload.SubjectRoleId,
                    assessmentPayload.SubjectJobTitle,
                    actionerId,
                    assessmentPayload.RatingDescription
                );

            plan.Assessments.Add(assessment);

            foreach (var questionPayload in assessmentPayload.Questions)
            {
                var question =
                    new AssessmentQuestion(
                        0,
                        questionPayload.QuestionText,
                        questionPayload.QuestionOrder,
                        actionerId,
                        questionPayload.QuestionType
                    );

                assessment.Questions.Add(question);
            }

            foreach (var receiverId in assessmentPayload.ReceiverIds)
            {
                var receiver =
                    new AssessmentReceiver(
                        0,
                        receiverId,
                        assessmentPayload.AssessmentType,
                        actionerId
                    );

                assessment.Receivers.Add(receiver);
            }


            if (assessmentPayload.Groups?.Any() == true)
            {
                foreach (var groupPayload in assessmentPayload.Groups)
                {
                    var group =
                        new AssessmentGroup(
                            0,
                            groupPayload.Name,
                            groupPayload.Description,
                            actionerId
                        );

                    assessment.Groups.Add(group);

                    foreach (var memberId in groupPayload.MemberIds)
                    {
                        var member =
                            new AssessmentGroupMember(
                                0,
                                memberId,
                                actionerId
                            );

                        group.Members.Add(member);
                    }
                }

            }


        }

        foreach (var scoreWeightPayload in payload.ScoreWeights)
        {
            var scoreWeight =
                new PerformanceReviewPlanScoreWeight(
                    0,
                    scoreWeightPayload.SubjectRoleId,
                    scoreWeightPayload.SubjectJobTitle,
                    scoreWeightPayload.ScoreType,
                    scoreWeightPayload.Weight,
                    actionerId
                );

            plan.PerformanceReviewPlanScoreWeights.Add(scoreWeight);

        }

        await _sqldbContext.PerformanceReviewPlans.AddAsync(plan, cancellationToken);

    }
}