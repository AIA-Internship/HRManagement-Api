using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System.Net.NetworkInformation;
using System.Numerics;

namespace HRManagement.MsSQL.Repositories;

public class PerformanceReviewPlanRepository : BaseRepository<PerformanceReviewPlan>, IPerformanceReviewPlanRepository
{
    private readonly ILogger<PerformanceReviewPlanRepository> _logger;

    public PerformanceReviewPlanRepository(AppDbContext dbContext, ILogger<PerformanceReviewPlanRepository> logger)
        : base(dbContext)
    {
        _logger = logger;
    }

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

    public async Task<List<PerformanceReviewPlanScoreWeightResponseDto>> GetScoreWeightConfigurationsAsync(int planId,CancellationToken cancellationToken)
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
        _logger.LogInformation("Current fillerId: {FillerId}", fillerId);
        var currentDate = DateTime.UtcNow.Date;

        var planDetail = await _sqldbContext.PerformanceReviewPlans
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                     && p.Status == "ongoing"
                     && currentDate >= p.StartDate
                     && currentDate <= p.EndDate
                     )
            .Select(p => new EmployeeOngoingPerformanceReviewPlanResponseDto
            {
                PlanId = p.Id,
                Name = p.Name,
                Status = p.Status,
                PeriodType = p.PeriodType,
                StartDate = p.StartDate,
                EndDate = p.EndDate,


                // 1. Navigate from Plan -> Intervals (Sorted chronologically on DB level)
                Intervals = p.Intervals
                    .Where(i => !i.IsDeleted)
                    .OrderBy(i => i.IntervalNumber)
                    .Select(i => new PerformanceReviewPlanIntervalResponseDto
                    {
                        Id = i.Id,
                        PlanId = i.PlanId,
                        IntervalNumber = i.IntervalNumber,
                        StartDate = i.StartDate,
                        DueDate = i.DueDate,
                        EndDate = i.EndDate,
                        Status = i.Status,

                        Assignments = i.FillAssignments
                            .Where(fa => !fa.IsDeleted && fa.FillerId == fillerId)
                            .Select(fa => new FillAssignmentResponseDto
                            {
                                AssignmentId = fa.Id,
                                SubjectId = fa.SubjectId,
                                AssessmentId = fa.AssessmentId,
                                Status = fa.Status,

                                Assessment = fa.Assessment != null && !fa.Assessment.IsDeleted
                                    ? new AssessmentBriefResponseDto
                                    {
                                        Id = fa.Assessment.Id,
                                        AnswerType = fa.Assessment.AnswerType,
                                        AssessmentType = fa.Assessment.AssessmentType,
                                        FillerRoleId = fa.Assessment.FillerRoleId,
                                        FillerJobTitle = fa.Assessment.FillerJobTitle,
                                        SubjectRoleId = fa.Assessment.SubjectRoleId,
                                        SubjectJobTitle = fa.Assessment.SubjectJobTitle
                                    }
                                    : null
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .AsSplitQuery()
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

    public async Task UpdatePerformanceReviewPlan(int planId, UpdatePerformanceReviewPlanPayload payload, int actionerId, CancellationToken cancellationToken)
    {
        var plan = await _sqldbContext.PerformanceReviewPlans
            .Include(p => p.Assessments)
                .ThenInclude(a => a.Questions)
            .Include(p => p.Assessments)
                .ThenInclude(a => a.Receivers)
            .Include(p => p.Assessments)
                .ThenInclude(a => a.Groups)
                    .ThenInclude(g => g.Members)
            .Include(p => p.PerformanceReviewPlanScoreWeights)
            .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

        if (plan == null) return;

        // update main plan properties
        plan.ApplyUpdate(
            payload.Name,
            payload.PeriodType,
            payload.DurationInMonth,
            payload.MinReviewDurationInDays,
            payload.StartDate,
            payload.EndDate,
            payload.Status,
            actionerId
        );

        // mark existing assessments and their children as deleted
        foreach (var existingAssessment in plan.Assessments.ToList())
        {
            foreach (var q in existingAssessment.Questions.ToList())
            {
                q.SetDelete(actionerId);
            }

            foreach (var r in existingAssessment.Receivers.ToList())
            {
                r.SetDelete(actionerId);
            }

            foreach (var g in existingAssessment.Groups.ToList())
            {
                foreach (var m in g.Members.ToList())
                {
                    m.SetDelete(actionerId);
                }

                g.SetDelete(actionerId);
            }

            existingAssessment.SetDelete(actionerId);
        }

        // add new assessments from payload
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

        // mark existing score weights as deleted
        foreach (var existingWeight in plan.PerformanceReviewPlanScoreWeights.ToList())
        {
            existingWeight.SetDelete(actionerId);
        }

        // add new score weights
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
    }

    public async Task CopyPerformanceReviewPlan(int planId,CopyPerformanceReviewPlanPayload payload,int actionerId,CancellationToken cancellationToken)
    {
        var existingPlan = await _sqldbContext.PerformanceReviewPlans
            .AsNoTracking()
            .Include(p => p.Assessments)
                .ThenInclude(a => a.Questions)
            .Include(p => p.Assessments)
                .ThenInclude(a => a.Receivers)
            .Include(p => p.Assessments)
                .ThenInclude(a => a.Groups)
                    .ThenInclude(g => g.Members)
            .Include(p => p.PerformanceReviewPlanScoreWeights)
            .FirstOrDefaultAsync(
                p => p.Id == planId && !p.IsDeleted,
                cancellationToken);

        if (existingPlan == null)
            return;

        var newPlan = new PerformanceReviewPlan(
            payload.Name,
            payload.PeriodType,
            payload.DurationInMonth,
            existingPlan.MinReviewDurationInDays,
            payload.StartDate,
            payload.EndDate,
            "drafted",
            actionerId
        );

        foreach (var existingAssessment in existingPlan.Assessments
            .Where(a => !a.IsDeleted))
        {
            var newAssessment = new Assessment(
                0,
                existingAssessment.AnswerType,
                existingAssessment.AssessmentType,
                existingAssessment.FillerRoleId,
                existingAssessment.FillerJobTitle,
                existingAssessment.SubjectRoleId,
                existingAssessment.SubjectJobTitle,
                actionerId,
                existingAssessment.RatingDescription
            );

            newPlan.Assessments.Add(newAssessment);

            foreach (var existingQuestion in existingAssessment.Questions
                .Where(q => !q.IsDeleted))
            {
                var newQuestion = new AssessmentQuestion(
                    0,
                    existingQuestion.QuestionText,
                    existingQuestion.QuestionOrder,
                    actionerId,
                    existingQuestion.QuestionType
                );

                newAssessment.Questions.Add(newQuestion);
            }

            foreach (var existingReceiver in existingAssessment.Receivers
                .Where(r => !r.IsDeleted))
            {
                var newReceiver = new AssessmentReceiver(
                    0,
                    existingReceiver.EmployeeId,
                    existingReceiver.ReceiverType,
                    actionerId
                );

                newAssessment.Receivers.Add(newReceiver);
            }

            foreach (var existingGroup in existingAssessment.Groups
                .Where(g => !g.IsDeleted))
            {
                var newGroup = new AssessmentGroup(
                    0,
                    existingGroup.Name,
                    existingGroup.Description,
                    actionerId
                );

                newAssessment.Groups.Add(newGroup);

                foreach (var existingMember in existingGroup.Members
                    .Where(m => !m.IsDeleted))
                {
                    var newMember = new AssessmentGroupMember(
                        0,
                        existingMember.EmployeeId,
                        actionerId
                    );

                    newGroup.Members.Add(newMember);
                }
            }
        }

        foreach (var existingWeight in existingPlan.PerformanceReviewPlanScoreWeights
            .Where(sw => !sw.IsDeleted))
        {
            var newWeight = new PerformanceReviewPlanScoreWeight(
                0,
                existingWeight.SubjectRoleId,
                existingWeight.SubjectJobTitle,
                existingWeight.ScoreType,
                existingWeight.Weights,
                actionerId
            );

            newPlan.PerformanceReviewPlanScoreWeights.Add(newWeight);
        }

        await _sqldbContext.PerformanceReviewPlans.AddAsync(
            newPlan,
            cancellationToken);
    }

    public async Task DeletePerformanceReviewPlan(int planId,int actionerId,CancellationToken cancellationToken)
    {
        var plan = await _sqldbContext.PerformanceReviewPlans
            .Include(p => p.Assessments)
                .ThenInclude(a => a.Questions)
            .Include(p => p.Assessments)
                .ThenInclude(a => a.Receivers)
            .Include(p => p.Assessments)
                .ThenInclude(a => a.Groups)
                    .ThenInclude(g => g.Members)
            .Include(p => p.PerformanceReviewPlanScoreWeights)
            .FirstOrDefaultAsync(
                p => p.Id == planId && !p.IsDeleted,
                cancellationToken);

        if (plan == null)
            return;

        foreach (var assessment in plan.Assessments)
        {
            foreach (var question in assessment.Questions)
            {
                question.SetDelete(actionerId);
            }

            foreach (var receiver in assessment.Receivers)
            {
                receiver.SetDelete(actionerId);
            }

            foreach (var group in assessment.Groups)
            {
                foreach (var member in group.Members)
                {
                    member.SetDelete(actionerId);
                }

                group.SetDelete(actionerId);
            }

            assessment.SetDelete(actionerId);
        }

        foreach (var scoreWeight in plan.PerformanceReviewPlanScoreWeights)
        {
            scoreWeight.SetDelete(actionerId);
        }

        plan.SetDelete(actionerId);
    }

    public async Task<bool> ActivatePerformanceReviewPlan(int planId,int actionerId,CancellationToken cancellationToken)
    {
        // =========================================================
        // 1. Load plan and all required relationships
        // =========================================================

        var plan = await _sqldbContext.PerformanceReviewPlans
            .Include(p => p.Assessments)
                .ThenInclude(a => a.Receivers)
                    .ThenInclude(r => r.Employee)
                        .ThenInclude(e => e.EmploymentInformation)

            .Include(p => p.Assessments)
                .ThenInclude(a => a.Groups)
                    .ThenInclude(g => g.Members)

            .Include(p => p.Intervals)

            .FirstOrDefaultAsync(
                p => p.Id == planId && !p.IsDeleted,
                cancellationToken);

        if (plan == null)
        {
            _logger.LogWarning(
                "Activation failed. Plan {PlanId} not found.",
                planId);

            return false;
        }


        // =========================================================
        // 2. Validate plan status
        // =========================================================

        // Only drafted plans can be activated.
        if (!string.Equals(
                plan.Status,
                "drafted",
                StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Activation failed. Plan {PlanId} has status {Status}.",
                planId,
                plan.Status);

            return false;
        }


        // =========================================================
        // 3. Validate plan dates
        // =========================================================

        if (plan.StartDate.Date >= plan.EndDate.Date)
        {
            _logger.LogWarning(
                "Activation failed. Plan {PlanId}: StartDate={StartDate}, EndDate={EndDate}.",
                planId,
                plan.StartDate,
                plan.EndDate);

            return false;
        }


        // =========================================================
        // 4. Validate duration
        // =========================================================

        if (plan.DurationInMonth <= 0)
        {
            _logger.LogWarning(
                "Activation failed. Plan {PlanId}: DurationInMonth={Duration}.",
                planId,
                plan.DurationInMonth);

            return false;
        }


        // =========================================================
        // 5. Determine months per interval
        // =========================================================

        var monthsPerInterval =
            GetMonthsPerInterval(plan.PeriodType);

        if (monthsPerInterval == 0)
        {
            _logger.LogWarning(
                "Activation failed. Plan {PlanId}: unsupported PeriodType={PeriodType}.",
                planId,
                plan.PeriodType);

            return false;
        }


        // =========================================================
        // 6. Validate duration against period
        // =========================================================

        if (plan.DurationInMonth % monthsPerInterval != 0)
        {
            _logger.LogWarning(
                "Activation failed. Plan {PlanId}: DurationInMonth={Duration} is not divisible by {MonthsPerInterval}.",
                planId,
                plan.DurationInMonth,
                monthsPerInterval);

            return false;
        }


        // =========================================================
        // 7. Validate EndDate
        // =========================================================

        var expectedEndDate = plan.StartDate.Date
            .AddMonths(plan.DurationInMonth)
            .AddDays(-1);

        if (expectedEndDate != plan.EndDate.Date)
        {
            _logger.LogWarning(
                "Activation failed. Plan {PlanId}: expected EndDate={ExpectedEndDate}, actual EndDate={ActualEndDate}.",
                planId,
                expectedEndDate,
                plan.EndDate.Date);

            return false;
        }


        // =========================================================
        // 8. Prevent duplicate intervals
        // =========================================================

        var activeIntervals = plan.Intervals
            .Where(i => !i.IsDeleted)
            .ToList();

        if (activeIntervals.Any())
        {
            _logger.LogWarning(
                "Activation failed. Plan {PlanId} already has {IntervalCount} active intervals.",
                planId,
                activeIntervals.Count);

            return false;
        }


        // =========================================================
        // 9. Get active assessments
        // =========================================================

        var activeAssessments = plan.Assessments
            .Where(a => !a.IsDeleted)
            .ToList();

        if (!activeAssessments.Any())
        {
            _logger.LogWarning(
                "Activation failed. Plan {PlanId} has no active assessments.",
                planId);

            return false;
        }


        // =========================================================
        // 10. Prepare assignment definitions
        //
        // receiverAssignments:
        //     self-assessment
        //     supervisor-assessment
        //
        // peerReviewAssignments:
        //     peer-review
        // =========================================================

        var receiverAssignments =
            new List<(
                Assessment Assessment,
                int FillerId,
                int SubjectId
            )>();

        var peerReviewAssignments =
            new List<(
                Assessment Assessment,
                int FillerId,
                int SubjectId
            )>();


        // =========================================================
        // 11. Build assignments from assessments
        // =========================================================

        foreach (var assessment in activeAssessments)
        {
            // =====================================================
            // PEER REVIEW
            // =====================================================

            if (string.Equals(
                    assessment.AssessmentType,
                    "peer-review",
                    StringComparison.OrdinalIgnoreCase))
            {
                var activeGroups = assessment.Groups
                    .Where(g => !g.IsDeleted)
                    .ToList();

                if (!activeGroups.Any())
                {
                    _logger.LogWarning(
                        "Activation failed. Peer-review assessment {AssessmentId} has no active groups.",
                        assessment.Id);

                    return false;
                }


                foreach (var group in activeGroups)
                {
                    var members = group.Members
                        .Where(m => !m.IsDeleted)
                        .ToList();

                    // A peer-review group must have at least 2 members.
                    if (members.Count < 2)
                    {
                        _logger.LogWarning(
                            "Activation failed. Peer-review group {GroupId} has only {MemberCount} active members.",
                            group.Id,
                            members.Count);

                        return false;
                    }


                    // Every member reviews every OTHER member
                    // inside the same group.
                    foreach (var filler in members)
                    {
                        foreach (var subject in members)
                        {
                            // An employee cannot review themselves.
                            if (filler.EmployeeId == subject.EmployeeId)
                                continue;

                            peerReviewAssignments.Add(
                                (
                                    assessment,
                                    filler.EmployeeId,
                                    subject.EmployeeId
                                ));
                        }
                    }
                }

                continue;
            }


            // =====================================================
            // SELF / SUPERVISOR ASSESSMENT
            // =====================================================

            var receivers = assessment.Receivers
                .Where(r => !r.IsDeleted)
                .ToList();

            if (!receivers.Any())
            {
                _logger.LogWarning(
                    "Activation failed. Assessment {AssessmentId} ({AssessmentType}) has no active receivers.",
                    assessment.Id,
                    assessment.AssessmentType);

                return false;
            }


            foreach (var receiver in receivers)
            {
                var subjectId = receiver.EmployeeId;

                int fillerId;


                // =================================================
                // SELF-ASSESSMENT
                // =================================================

                if (string.Equals(
                        assessment.AssessmentType,
                        "self-assessment",
                        StringComparison.OrdinalIgnoreCase))
                {
                    fillerId = subjectId;
                }


                // =================================================
                // SUPERVISOR-ASSESSMENT
                // =================================================

                else if (string.Equals(
                             assessment.AssessmentType,
                             "supervisor-assessment",
                             StringComparison.OrdinalIgnoreCase))
                {
                    var supervisorId =
                        receiver.Employee?
                            .EmploymentInformation?
                            .SupervisorId;

                    if (!supervisorId.HasValue)
                    {
                        _logger.LogWarning(
                            "Activation failed. Employee {EmployeeId} has no SupervisorId for assessment {AssessmentId}.",
                            subjectId,
                            assessment.Id);

                        return false;
                    }

                    fillerId = supervisorId.Value;
                }


                // =================================================
                // UNKNOWN ASSESSMENT TYPE
                // =================================================

                else
                {
                    _logger.LogWarning(
                        "Activation skipped unsupported assessment type {AssessmentType} for AssessmentId {AssessmentId}.",
                        assessment.AssessmentType,
                        assessment.Id);

                    continue;
                }


                receiverAssignments.Add(
                    (
                        assessment,
                        fillerId,
                        subjectId
                    ));
            }
        }


        // =========================================================
        // 12. Validate that we actually have assignments
        // =========================================================

        if (!receiverAssignments.Any() &&
            !peerReviewAssignments.Any())
        {
            _logger.LogWarning(
                "Activation failed. Plan {PlanId} generated no assignments.",
                planId);

            return false;
        }


        // =========================================================
        // 13. Generate interval definitions
        // =========================================================

        var today = DateTime.UtcNow.Date;

        var intervalCount =
            plan.DurationInMonth / monthsPerInterval;

        var intervalDefinitions =
            new List<(
                int Number,
                DateTime StartDate,
                DateTime DueDate,
                DateTime EndDate,
                string Status
            )>();


        var intervalStartDate =
            plan.StartDate.Date;


        // =========================================================
        // Example:
        //
        // Quarterly + 12 months
        //
        // Q1 = 2026-01-01 -> 2026-03-31
        // Q2 = 2026-04-01 -> 2026-06-30
        // Q3 = 2026-07-01 -> 2026-09-30
        // Q4 = 2026-10-01 -> 2026-12-31
        // =========================================================

        for (
            var intervalNumber = 1;
            intervalNumber <= intervalCount;
            intervalNumber++)
        {
            var nextIntervalStartDate =
                intervalStartDate.AddMonths(monthsPerInterval);

            var intervalEndDate =
                nextIntervalStartDate.AddDays(-1);


            // Never exceed the plan's EndDate.
            if (intervalEndDate > plan.EndDate.Date)
            {
                intervalEndDate =
                    plan.EndDate.Date;
            }


            // =====================================================
            // DueDate
            //
            // EndDate - MinReviewDurationInDays
            // =====================================================

            var dueDate =
                intervalEndDate.AddDays(
                    -plan.MinReviewDurationInDays);


            if (dueDate < intervalStartDate)
            {
                _logger.LogWarning(
                    "Activation failed. Plan {PlanId}: invalid DueDate for interval {IntervalNumber}.",
                    planId,
                    intervalNumber);

                return false;
            }


            intervalDefinitions.Add(
                (
                    intervalNumber,
                    intervalStartDate,
                    dueDate,
                    intervalEndDate,
                    GetIntervalStatus(
                        today,
                        intervalStartDate,
                        intervalEndDate)
                ));


            intervalStartDate =
                nextIntervalStartDate;
        }


        // =========================================================
        // 14. Create intervals + FillAssignments
        // =========================================================

        foreach (var definition in intervalDefinitions)
        {
            var interval =
                new PerformanceReviewPlanInterval(
                    plan.Id,
                    definition.Number,
                    definition.StartDate,
                    definition.DueDate,
                    definition.EndDate,
                    definition.Status,
                    actionerId);


            plan.Intervals.Add(interval);


            // =====================================================
            // SELF + SUPERVISOR ASSIGNMENTS
            // =====================================================

            foreach (var receiverAssignment
                     in receiverAssignments)
            {
                var assignment =
                    new FillAssignment(
                        plan.Id,
                        0,
                        receiverAssignment.FillerId,
                        receiverAssignment.SubjectId,
                        receiverAssignment.Assessment.Id,
                        "not started",
                        actionerId)
                    {
                        Interval = interval
                    };


                interval.FillAssignments.Add(
                    assignment);
            }


            // =====================================================
            // PEER REVIEW ASSIGNMENTS
            // =====================================================

            foreach (var peerReviewAssignment
                     in peerReviewAssignments)
            {
                var assignment =
                    new FillAssignment(
                        plan.Id,
                        0,
                        peerReviewAssignment.FillerId,
                        peerReviewAssignment.SubjectId,
                        peerReviewAssignment.Assessment.Id,
                        "not started",
                        actionerId)
                    {
                        Interval = interval
                    };


                interval.FillAssignments.Add(
                    assignment);
            }
        }


        // =========================================================
        // 15. Deactivate existing ongoing plans
        // =========================================================

        var existingOngoingPlans = await _sqldbContext.PerformanceReviewPlans
            .Where(p =>
                p.Id != plan.Id &&
                !p.IsDeleted &&
                p.Status.ToLower() == "ongoing")
            .ToListAsync(cancellationToken);

        foreach (var existingPlan in existingOngoingPlans)
        {
            existingPlan.ApplyUpdate(
                existingPlan.Name,
                existingPlan.PeriodType,
                existingPlan.DurationInMonth,
                existingPlan.MinReviewDurationInDays,
                existingPlan.StartDate,
                existingPlan.EndDate,
                "done",
                actionerId);
        }


        // =========================================================
        // 16. Finally activate the selected plan
        // =========================================================

        plan.ApplyUpdate(
            plan.Name,
            plan.PeriodType,
            plan.DurationInMonth,
            plan.MinReviewDurationInDays,
            plan.StartDate,
            plan.EndDate,
            "ongoing",
            actionerId);

        return true;
    }

    private static int GetMonthsPerInterval(string periodType)
    {
        return periodType.Trim().ToLowerInvariant() switch
        {
            "monthly" => 1,
            "quarterly" => 3,
            _ => 0
        };
    }

    private static string GetIntervalStatus(DateTime today,DateTime startDate,DateTime endDate)
    {
        if (today > endDate)
            return "done";

        if (today >= startDate && today <= endDate)
            return "ongoing";

        return "locked";
    }

    public async Task<List<string>> GetPlanRolesAsync(int planId, CancellationToken cancellationToken = default)
    {
        return await _sqldbContext.PerformanceReviewPlans
            .AsNoTracking()
            .Where(p => p.Id == planId && !p.IsDeleted)
            .SelectMany(p => p.Assessments
                .Where(a => !a.IsDeleted)
                .SelectMany(a => a.Receivers
                    .Where(r => !r.IsDeleted)
                    .Select(r => r.Employee.EmploymentInformation!.PositionName)))
            .Where(x => !string.IsNullOrWhiteSpace(x) &&
                        x.ToUpper() != "SUPERVISOR") // use ToUpper() or ToLower()
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken); 
    }

}