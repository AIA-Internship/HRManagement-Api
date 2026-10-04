using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Repositories
{
    public class AssignmentRepository
    : BaseRepository<FillAssignment>, IAssignmentRepository
    {
        public AssignmentRepository(AppDbContext dbContext)
            : base(dbContext) { }

        public async Task AddReviewAssignmentAsync(
        ReviewAssignment reviewAssignment,
        CancellationToken cancellationToken)
        {
            await _sqldbContext.ReviewAssignments.AddAsync(reviewAssignment, cancellationToken);
        }

        public async Task<List<AssessmentAnswer>> GetAnswersByIdsAsync(List<int> answerIds, CancellationToken cancellationToken)
        {
            if (answerIds == null || !answerIds.Any())
                return new List<AssessmentAnswer>();

            return await _sqldbContext.AssessmentAnswers
                .Where(a => !a.IsDeleted && answerIds.Contains(a.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<PeerReviewMatrixResponseDto?> GetPeerReviewsBySubjectAsync(int supervisorId, int intervalId, int subjectId, CancellationToken cancellationToken)
        {
            // 1. Fetch all peer-review fill assignments targeting this subject in the specific interval
            var peerAssignments = await _sqldbContext.FillAssignments
                .AsNoTracking()
                .Include(fa => fa.Assessment)
                    .ThenInclude(a => a.Questions)
                .Include(fa => fa.Filler)
                .Include(fa => fa.Subject)
                .Where(fa => fa.IntervalId == intervalId
                         && fa.SubjectId == subjectId
                         && fa.Assessment.AssessmentType == "peer-review"
                         && !fa.IsDeleted)
                .ToListAsync(cancellationToken);

            if (!peerAssignments.Any())
                return null;

            var subjectUser = peerAssignments.First().Subject;
            var assignmentIds = peerAssignments.Select(fa => fa.Id).ToList();

            // 2. Fetch questions from the associated assessment template
            var questions = peerAssignments
                .SelectMany(fa => fa.Assessment.Questions)
                .Where(q => !q.IsDeleted)
                .DistinctBy(q => q.Id)
                .OrderBy(q => q.QuestionOrder)
                .Select(q => new QuestionHeaderDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionOrder = q.QuestionOrder,
                    QuestionType = q.QuestionType
                })
                .ToList();

            // 3. Fetch all answers submitted by the peers for these assignments
            var answers = await _sqldbContext.AssessmentAnswers
                .AsNoTracking()
                .Where(aa => assignmentIds.Contains(aa.AssignmentId) && !aa.IsDeleted)
                .ToListAsync(cancellationToken);

            var answerIds = answers.Select(a => a.Id).ToList();

            // 4. Fetch the supervisor's ReviewAssignments linked to each of these peer fill assignments
            var reviewAssignments = await _sqldbContext.ReviewAssignments
                .AsNoTracking()
                .Where(ra => !ra.IsDeleted
                          && ra.IntervalId == intervalId
                          && assignmentIds.Contains(ra.AssignmentId)
                          && ra.ReviewerId == supervisorId)
                .ToListAsync(cancellationToken);

            var reviewAssignmentIds = reviewAssignments.Select(ra => ra.Id).ToList();

            // 5. Fetch supervisor's assessment answer scores tied to these review assignments
            var answerScores = await _sqldbContext.AssessmentAnswerScores
                .AsNoTracking()
                .Where(s => reviewAssignmentIds.Contains(s.ReviewAssignmentId) && !s.IsDeleted && s.ReviewerId == supervisorId)
                .ToDictionaryAsync(s => s.AssessmentAnswerId, s => (decimal?)s.Score, cancellationToken);

            // Create a dictionary map for quick status lookup per peer assignment ID: AssignmentId -> Review Status
            var reviewStatusMap = reviewAssignments
                .ToDictionary(ra => ra.AssignmentId, ra => ra.Status);

            // 6. Map matrix response
            var response = new PeerReviewMatrixResponseDto
            {
                SubjectId = subjectId,
                SubjectName = subjectUser.FullName,
                IntervalId = intervalId,
                Questions = questions,
                Reviewers = peerAssignments.Select(fa =>
                {
                    // Determine supervisor's review status for this specific peer assignment (fallback to "not started")
                    string supervisorStatus = reviewStatusMap.TryGetValue(fa.Id, out var stat) ? stat : "not started";

                    return new PeerReviewerAnswersDto
                    {
                        AssignmentId = fa.Id,
                        FillerId = fa.FillerId,
                        FillerName = fa.Filler.FullName,

                        // CRITICAL: Use the supervisor's review status instead of the peer's fill status ("done")
                        Status = supervisorStatus,

                        Answers = answers
                            .Where(a => a.AssignmentId == fa.Id)
                            .Select(a => new AnswerDetailDto
                            {
                                AnswerId = a.Id,
                                QuestionId = a.AssessmentQuestionId,
                                TextValue = a.TextValue,
                                RatingValue = a.RatingValue,
                                // Pull supervisor's review score from dictionary
                                ReviewScore = answerScores.TryGetValue(a.Id, out var score) ? score : null
                            })
                            .ToList()
                    };
                }).ToList()
            };

            return response;
        }

        public async Task<FillAssignment?> FindAssignmentAsync(
        int planId,
        int intervalId,
        int fillerId,
        int subjectId,
        int assessmentId,
        CancellationToken cancellationToken)
        {
            return await _sqldbContext.FillAssignments
                .FirstOrDefaultAsync(fa => !fa.IsDeleted
                                        && fa.PlanId == planId
                                        && fa.IntervalId == intervalId
                                        && fa.FillerId == fillerId
                                        && fa.SubjectId == subjectId
                                        && fa.AssessmentId == assessmentId,
                                     cancellationToken);
        }

        public async Task<FillAssignment?> GetAssignmentByIdAsync(int assignmentId, CancellationToken cancellationToken)
        {
            return await _sqldbContext.FillAssignments
                .FirstOrDefaultAsync(fa => !fa.IsDeleted && fa.Id == assignmentId, cancellationToken);
        }

        public async Task AddAssignmentAsync(FillAssignment assignment, CancellationToken cancellationToken)
        {
            await _sqldbContext.FillAssignments.AddAsync(assignment, cancellationToken);
        }

        public async Task<FillAssignmentDetailResponseDto?> GetSupervisorAssignmentBySubjectAsync(
        int supervisorId,
        int subjectId,
        int intervalId,
        CancellationToken cancellationToken)
        {
            // Step 1: Check if an assignment already exists
            var existingAssignment = await _sqldbContext.FillAssignments
                .AsNoTracking()
                .Where(fa => !fa.IsDeleted
                          && fa.FillerId == supervisorId
                          && fa.SubjectId == subjectId
                          && fa.IntervalId == intervalId
                          && fa.Assessment.AssessmentType == "supervisor-assessment")
                .Select(fa => new FillAssignmentDetailResponseDto
                {
                    AssignmentId = fa.Id,
                    PlanId = fa.PlanId,
                    IntervalId = fa.IntervalId,
                    FillerId = fa.FillerId,
                    SubjectId = fa.SubjectId,
                    AssessmentId = fa.AssessmentId,
                    Status = fa.Status,
                    ModifiedBy = fa.Modifier != null ? fa.Modifier.FullName : "",
                    ModifiedUtcDate = fa.ModifiedUtcDate,

                    Subject = fa.Subject != null && !fa.Subject.IsDeleted ? new EmployeeMinimalInfoResponseDto
                    {
                        Id = fa.Subject.Id,
                        FullName = fa.Subject.FullName
                    } : null,

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

            if (existingAssignment != null)
            {
                return existingAssignment;
            }

            // Step 2: Fetch Interval and Subject Employee
            var interval = await _sqldbContext.PerformanceReviewPlanIntervals
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == intervalId && !i.IsDeleted, cancellationToken);

            if (interval == null) return null;

            var subjectEmployee = await _sqldbContext.Employees
                .AsNoTracking()
                .Include(e => e.EmploymentInformation)
                .FirstOrDefaultAsync(e => e.Id == subjectId && !e.IsDeleted, cancellationToken);

            if (subjectEmployee == null) return null;

            // Step 3: Fetch exact matching Assessment Template (by SubjectRoleId / SubjectJobTitle if present)
            var assessmentTemplate = await _sqldbContext.Assessments
                .AsNoTracking()
                .Where(a => !a.IsDeleted
                         && a.PlanId == interval.PlanId
                         && a.AssessmentType == "supervisor-assessment"
                         && (a.SubjectRoleId == null || a.SubjectRoleId == subjectEmployee.RoleId)
                         && (a.SubjectJobTitle == null || a.SubjectJobTitle == subjectEmployee.EmploymentInformation.PositionName))
                .Select(a => new AssessmentDetailResponseDto
                {
                    Id = a.Id,
                    AnswerType = a.AnswerType,
                    AssessmentType = a.AssessmentType,
                    FillerRoleId = a.FillerRoleId,
                    FillerJobTitle = a.FillerJobTitle,
                    SubjectRoleId = a.SubjectRoleId,
                    SubjectJobTitle = a.SubjectJobTitle,

                    Questions = a.Questions
                        .Where(q => !q.IsDeleted)
                        .OrderBy(q => q.QuestionOrder)
                        .Select(q => new AssessmentQuestionResponseDto
                        (
                            q.Id,
                            q.AssessmentId,
                            q.QuestionText,
                            q.QuestionOrder,
                            q.QuestionType,
                            null
                        ))
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (assessmentTemplate == null) return null;

            // Step 4: Return synthesized response (AssignmentId = 0)
            return new FillAssignmentDetailResponseDto
            {
                AssignmentId = 0,
                PlanId = interval.PlanId,
                IntervalId = intervalId,
                FillerId = supervisorId,
                SubjectId = subjectId,
                AssessmentId = assessmentTemplate.Id,
                Status = "not started",

                Subject = new EmployeeMinimalInfoResponseDto
                {
                    Id = subjectEmployee.Id,
                    FullName = subjectEmployee.FullName
                },

                Assessment = assessmentTemplate
            };
        }

        public async Task<FillAssignmentDetailResponseDto?> GetSupervisorSelfAssessmentReviewAsync(int supervisorId, int subjectId, int intervalId, CancellationToken cancellationToken)
        {
            // 1. Find the employee's self-assessment fill assignment
            var employeeAssignment = await _sqldbContext.FillAssignments
                .AsNoTracking()
                .Include(fa => fa.Subject)
                .Include(fa => fa.Modifier)
                .Include(fa => fa.Assessment)
                    .ThenInclude(a => a.Questions.Where(q => !q.IsDeleted))
                        .ThenInclude(q => q.Answers.Where(ans => !ans.IsDeleted))
                .FirstOrDefaultAsync(fa => !fa.IsDeleted
                                        && fa.IntervalId == intervalId
                                        && fa.SubjectId == subjectId
                                        && fa.FillerId == subjectId
                                        && fa.Assessment.AssessmentType == "self-assessment",
                                        cancellationToken);

            if (employeeAssignment == null)
            {
                // Fallback if no self-assessment assignment exists yet: check template & interval to return "not started" shell
                return await GetUnstartedSelfAssessmentShellAsync(supervisorId, subjectId, intervalId, cancellationToken);
            }

            // 2. Find the supervisor's review assignment linked to this fill assignment
            var reviewAssignment = await _sqldbContext.ReviewAssignments
                .AsNoTracking()
                .FirstOrDefaultAsync(ra => !ra.IsDeleted
                                        && ra.IntervalId == intervalId
                                        && ra.AssignmentId == employeeAssignment.Id
                                        && ra.ReviewerId == supervisorId,
                                        cancellationToken);

            // 3. Fetch existing answer scores directly from AssessmentAnswerScores table
            var answerScores = new List<AssessmentAnswerScore>();
            if (reviewAssignment != null)
            {
                answerScores = await _sqldbContext.AssessmentAnswerScores
                    .AsNoTracking()
                    .Where(s => !s.IsDeleted && s.ReviewAssignmentId == reviewAssignment.Id && s.ReviewerId == supervisorId)
                    .ToListAsync(cancellationToken);
            }

            // Determine the review status for the supervisor (fallback to "not started")
            string supervisorReviewStatus = reviewAssignment?.Status ?? "not started";

            // Build a lookup map for supervisor scores: AssessmentAnswerId -> Supervisor Rating Score
            var supervisorScoresMap = answerScores
                .ToDictionary(s => s.AssessmentAnswerId, s => s.Score);

            // 3. Map everything into the response DTO
            return new FillAssignmentDetailResponseDto
            {
                AssignmentId = employeeAssignment.Id,
                PlanId = employeeAssignment.PlanId,
                IntervalId = employeeAssignment.IntervalId,
                FillerId = employeeAssignment.FillerId,
                SubjectId = employeeAssignment.SubjectId,
                AssessmentId = employeeAssignment.AssessmentId,

                // CRITICAL FIX: Use the supervisor's review status instead of employee's "done" status
                Status = supervisorReviewStatus,

                ModifiedBy = reviewAssignment?.Reviewer?.FullName ?? (employeeAssignment.Modifier != null ? employeeAssignment.Modifier.FullName : ""),
                ModifiedUtcDate = reviewAssignment?.ModifiedUtcDate ?? employeeAssignment.ModifiedUtcDate,

                Subject = employeeAssignment.Subject != null && !employeeAssignment.Subject.IsDeleted ? new EmployeeMinimalInfoResponseDto
                {
                    Id = employeeAssignment.Subject.Id,
                    FullName = employeeAssignment.Subject.FullName
                } : null,

                Assessment = employeeAssignment.Assessment != null && !employeeAssignment.Assessment.IsDeleted ? new AssessmentDetailResponseDto
                {
                    Id = employeeAssignment.Assessment.Id,
                    AnswerType = employeeAssignment.Assessment.AnswerType,
                    AssessmentType = employeeAssignment.Assessment.AssessmentType,
                    FillerRoleId = employeeAssignment.Assessment.FillerRoleId,
                    FillerJobTitle = employeeAssignment.Assessment.FillerJobTitle,
                    SubjectRoleId = employeeAssignment.Assessment.SubjectRoleId,
                    SubjectJobTitle = employeeAssignment.Assessment.SubjectJobTitle,

                    Questions = employeeAssignment.Assessment.Questions
                        .Where(q => !q.IsDeleted)
                        .OrderBy(q => q.QuestionOrder)
                        .Select(q =>
                        {
                            // Get employee's answer for this question
                            var employeeAnswer = q.Answers
                                .FirstOrDefault(ans => !ans.IsDeleted && ans.AssignmentId == employeeAssignment.Id);

                            // Find if supervisor already scored this specific answer
                            decimal? supervisorRating = null;
                            if (employeeAnswer != null && supervisorScoresMap.TryGetValue(employeeAnswer.Id, out var scoreVal))
                            {
                                supervisorRating = scoreVal;
                            }

                            return new AssessmentQuestionResponseDto(
                                q.Id,
                                q.AssessmentId,
                                q.QuestionText,
                                q.QuestionOrder,
                                q.QuestionType,
                                employeeAnswer != null ? new AssessmentAnswerResponseDto
                                {
                                    Id = employeeAnswer.Id,
                                    TextValue = employeeAnswer.TextValue,
                                    RatingValue = employeeAnswer.RatingValue,
                                    // Pass supervisor rating down to the DTO if your AssessmentAnswerResponseDto supports it
                                    SupervisorRating = supervisorRating
                                } : null
                            );
                        })
                        .ToList()
                } : null
            };
        }

        // Helper method for fallback shell when no employee assignment is found yet
        private async Task<FillAssignmentDetailResponseDto?> GetUnstartedSelfAssessmentShellAsync(int supervisorId, int subjectId, int intervalId, CancellationToken cancellationToken)
        {
            var interval = await _sqldbContext.PerformanceReviewPlanIntervals
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == intervalId && !i.IsDeleted, cancellationToken);

            if (interval == null) return null;

            var subjectEmployee = await _sqldbContext.Employees
                .AsNoTracking()
                .Include(e => e.EmploymentInformation)
                .FirstOrDefaultAsync(e => e.Id == subjectId && !e.IsDeleted, cancellationToken);

            if (subjectEmployee == null) return null;

            var assessmentTemplate = await _sqldbContext.Assessments
                .AsNoTracking()
                .Where(a => !a.IsDeleted
                         && a.PlanId == interval.PlanId
                         && a.AssessmentType == "self-assessment"
                         && (a.SubjectRoleId == null || a.SubjectRoleId == subjectEmployee.RoleId)
                         && (a.SubjectJobTitle == null || a.SubjectJobTitle == subjectEmployee.EmploymentInformation.PositionName))
                .Select(a => new AssessmentDetailResponseDto
                {
                    Id = a.Id,
                    AnswerType = a.AnswerType,
                    AssessmentType = a.AssessmentType,
                    FillerRoleId = a.FillerRoleId,
                    FillerJobTitle = a.FillerJobTitle,
                    SubjectRoleId = a.SubjectRoleId,
                    SubjectJobTitle = a.SubjectJobTitle,

                    Questions = a.Questions
                        .Where(q => !q.IsDeleted)
                        .OrderBy(q => q.QuestionOrder)
                        .Select(q => new AssessmentQuestionResponseDto(
                            q.Id,
                            q.AssessmentId,
                            q.QuestionText,
                            q.QuestionOrder,
                            q.QuestionType,
                            null
                        ))
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (assessmentTemplate == null) return null;

            return new FillAssignmentDetailResponseDto
            {
                AssignmentId = 0,
                PlanId = interval.PlanId,
                IntervalId = intervalId,
                FillerId = subjectId,
                SubjectId = subjectId,
                AssessmentId = assessmentTemplate.Id,
                Status = "not started",
                Subject = new EmployeeMinimalInfoResponseDto
                {
                    Id = subjectEmployee.Id,
                    FullName = subjectEmployee.FullName
                },
                Assessment = assessmentTemplate
            };
        }

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
                    ModifiedBy = fa.Modifier != null ? fa.Modifier.FullName : "",
                    ModifiedUtcDate = fa.ModifiedUtcDate,

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

        public async Task<List<AssignmentValidationResponseDto>> GetAssignmentsForValidationAsync(
        int intervalId,
        int fillerId,
        CancellationToken cancellationToken)
        {
            // 1. Fetch raw projected assignments and question lists from database
            var rawData = await _sqldbContext.FillAssignments
                .AsNoTracking()
                .Where(fa => !fa.IsDeleted
                          && fa.FillerId == fillerId
                          && fa.IntervalId == intervalId)
                .Select(fa => new
                {
                    fa.Id,
                    fa.Status,
                    fa.FillerId,
                    AnswerType = fa.Assessment.AnswerType,
                    Questions = fa.Assessment.Questions
                        .Where(q => !q.IsDeleted)
                        .Select(q => new { q.Id, q.QuestionType })
                        .ToList() // Fetches questions as a list into memory
                })
                .ToListAsync(cancellationToken);

            // 2. Transform into AssignmentValidationResponseDto in memory
            return rawData.Select(item => new AssignmentValidationResponseDto(
                item.Id,
                item.Status,
                item.FillerId,
                item.AnswerType,
                item.Questions.ToDictionary(q => q.Id, q => q.QuestionType) // Builds Dictionary in memory
            )).ToList();
        }

        public async Task<AssignmentValidationResponseDto?> GetAssignmentForValidationAsync(
        int assignmentId,
        CancellationToken cancellationToken)
        {
            // 1. Fetch raw flattened or projected data from database
            var result = await _sqldbContext.FillAssignments
                .AsNoTracking()
                .Where(fa => !fa.IsDeleted && fa.Id == assignmentId)
                .Select(fa => new
                {
                    fa.Id,
                    fa.Status,
                    fa.FillerId,
                    AnswerType = fa.Assessment.AnswerType,
                    Questions = fa.Assessment.Questions
                        .Where(q => !q.IsDeleted)
                        .Select(q => new { q.Id, q.QuestionType })
                        .ToList() // Client evaluation boundary for questions list
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null) return null;

            // 2. Build the Dictionary safely in memory
            var questionsDict = result.Questions
                .ToDictionary(q => q.Id, q => q.QuestionType);

            // 3. Return your DTO
            return new AssignmentValidationResponseDto(
                result.Id,
                result.Status,
                result.FillerId,
                result.AnswerType,
                questionsDict
            );
        }

        public async Task UpsertAnswersAsync(
        List<AssessmentAnswer> incomingAnswers,
        Dictionary<int, string> assignmentAnswerTypes,
        CancellationToken cancellationToken)
        {
            if (incomingAnswers == null || !incomingAnswers.Any()) return;

            var assignmentIds = incomingAnswers.Select(a => a.AssignmentId).Distinct().ToList();
            var questionIds = incomingAnswers.Select(a => a.AssessmentQuestionId).Distinct().ToList();

            // 1. Load existing answers
            var existingAnswers = await _sqldbContext.AssessmentAnswers
                .Where(a => assignmentIds.Contains(a.AssignmentId) && questionIds.Contains(a.AssessmentQuestionId))
                .ToListAsync(cancellationToken);

            // 2. Fetch specific QuestionTypes from AssessmentQuestions
            var questionTypes = await _sqldbContext.AssessmentQuestions
                .Where(q => questionIds.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id, q => q.QuestionType, cancellationToken);

            foreach (var incoming in incomingAnswers)
            {
                // 3. Resolve AnswerType: First check QuestionType, fallback to Assignment level AnswerType
                questionTypes.TryGetValue(incoming.AssessmentQuestionId, out var questionType);
                assignmentAnswerTypes.TryGetValue(incoming.AssignmentId, out var assignmentType);

                string resolvedType = !string.IsNullOrWhiteSpace(questionType)
                    ? questionType
                    : (assignmentType ?? "text");

                var existing = existingAnswers.FirstOrDefault(a =>
                    a.AssignmentId == incoming.AssignmentId &&
                    a.AssessmentQuestionId == incoming.AssessmentQuestionId);

                if (existing != null)
                {
                    existing.ApplyUpdate(incoming.TextValue, incoming.RatingValue, resolvedType, incoming.ModifiedBy);
                }
                else
                {
                    // 4. Ensure incoming new records also apply the type formatting rules before insert
                    incoming.ApplyUpdate(incoming.TextValue, incoming.RatingValue, resolvedType, incoming.CreatedBy);
                    await _sqldbContext.AssessmentAnswers.AddAsync(incoming, cancellationToken);
                }
            }
        }

        public async Task<List<FillAssignment>> GetAssignmentsWithAssessmentsAsync(
        List<int> assignmentIds,
        CancellationToken cancellationToken)
        {
            if (assignmentIds == null || !assignmentIds.Any())
                return new List<FillAssignment>();

            return await _sqldbContext.FillAssignments
                .Include(fa => fa.Assessment)
                .Where(fa => assignmentIds.Contains(fa.Id) && !fa.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SupervisorFillAssignmentResponseDto>> GetSupervisorAssignmentStatusesAsync(
        int? intervalId,
        CancellationToken cancellationToken)
        {
            int targetIntervalId;

            if (intervalId.HasValue)
            {
                targetIntervalId = intervalId.Value;
            }
            else
            {
                var currentDate = DateTime.UtcNow.Date;

                // Find the active ongoing interval based on current date
                var ongoingIntervalId = await _sqldbContext.PerformanceReviewPlanIntervals
                    .AsNoTracking()
                    .Where(i => !i.IsDeleted
                                && i.PerformanceReviewPlan.Status == "ongoing"
                                && !i.PerformanceReviewPlan.IsDeleted
                                && currentDate >= i.StartDate
                                && currentDate <= i.EndDate)
                    .Select(i => i.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ongoingIntervalId == 0)
                {
                    // No ongoing interval active right now
                    return new List<SupervisorFillAssignmentResponseDto>();
                }

                targetIntervalId = ongoingIntervalId;
            }

            // Execute data projection using the targetIntervalId
            return await _sqldbContext.FillAssignments
                .AsNoTracking()
                .Where(fa => fa.IntervalId == targetIntervalId && !fa.IsDeleted)
                .Select(fa => new SupervisorFillAssignmentResponseDto
                {
                    PlanId = fa.PlanId,
                    IntervalId = fa.IntervalId,
                    AssignmentId = fa.Id,
                    SubjectId = fa.SubjectId,
                    AssessmentType = fa.Assessment.AssessmentType,
                    FillerId = fa.FillerId,
                    AnswerType = fa.Assessment.AnswerType,
                    AssignmentStatus = fa.Status,

                    Subject = new SubjectResponseDto
                    {
                        Id = fa.SubjectId,
                        Name = fa.Subject.FullName,
                        JobTitle = fa.Subject.EmploymentInformation!.PositionName
                    },

                    Reviewers = _sqldbContext.ReviewAssignments
                        .Where(ra => ra.AssignmentId == fa.Id && !ra.IsDeleted)
                        .Select(ra => new ReviewerResponseDto
                        {
                            Id = ra.Reviewer!.Id,
                            Name = ra.Reviewer.FullName,
                            Status = ra.Status
                        }).ToList()
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<FillAssignmentDetailResponseDto>> GetPeerAssignmentDetailsByIntervalAsync(
        int fillerId,
        int intervalId,
        CancellationToken cancellationToken)
        {
            return await _sqldbContext.FillAssignments
                .AsNoTracking()
                .Where(fa => !fa.IsDeleted
                          && fa.FillerId == fillerId
                          && fa.IntervalId == intervalId
                          && fa.Assessment.AssessmentType == "peer-review")
                .Select(fa => new FillAssignmentDetailResponseDto
                {
                    AssignmentId = fa.Id,
                    PlanId = fa.PlanId,
                    IntervalId = fa.IntervalId,
                    FillerId = fa.FillerId,
                    SubjectId = fa.SubjectId,
                    AssessmentId = fa.AssessmentId,
                    Status = fa.Status,
                    ModifiedBy = fa.Modifier != null ? fa.Modifier.FullName : "",
                    ModifiedUtcDate = fa.ModifiedUtcDate,

                    Subject = fa.Subject != null && !fa.Subject.IsDeleted ? new EmployeeMinimalInfoResponseDto
                    {
                        Id = fa.Subject.Id,
                        FullName = fa.Subject.FullName
                    } : null,

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
                .ToListAsync(cancellationToken);
        }

        public async Task<ReviewAssignment?> GetReviewAssignmentAsync(
        int planId, 
        int intervalId, 
        int assignmentId, 
        int reviewerId, 
        CancellationToken cancellationToken)
        {
            return await _sqldbContext.ReviewAssignments
                .FirstOrDefaultAsync(r => !r.IsDeleted
                                       && r.PlanId == planId
                                       && r.IntervalId == intervalId
                                       && r.AssignmentId == assignmentId
                                       && r.ReviewerId == reviewerId,
                                     cancellationToken);
        }

        public async Task<List<AssessmentAnswer>> GetTextAnswersForAssignmentAsync(
            int fillAssignmentId, 
            CancellationToken cancellationToken)
        {
            return await _sqldbContext.AssessmentAnswers
                .Include(a => a.AssessmentQuestion)
                .Where(a => !a.IsDeleted 
                         && a.AssignmentId == fillAssignmentId 
                         && a.TextValue != null 
                         && (a.AssessmentQuestion.QuestionType.Contains("text") || a.AssessmentQuestion.QuestionType == null))
                .ToListAsync(cancellationToken);
        }

        public async Task UpsertAnswerScoresAsync(List<AssessmentAnswerScore> incomingScores, CancellationToken cancellationToken)
        {
            if (incomingScores == null || !incomingScores.Any()) return;

            var reviewAssignmentIds = incomingScores.Select(s => s.ReviewAssignmentId).Distinct().ToList();
            var answerIds = incomingScores.Select(s => s.AssessmentAnswerId).Distinct().ToList();

            var existingScores = await _sqldbContext.AssessmentAnswerScores
                .Where(s => !s.IsDeleted
                         && reviewAssignmentIds.Contains(s.ReviewAssignmentId)
                         && answerIds.Contains(s.AssessmentAnswerId))
                .ToListAsync(cancellationToken);

            foreach (var incoming in incomingScores)
            {
                var existing = existingScores.FirstOrDefault(s =>
                    s.ReviewAssignmentId == incoming.ReviewAssignmentId &&
                    s.AssessmentAnswerId == incoming.AssessmentAnswerId);

                if (existing != null)
                {
                    // Call the public domain method instead of assigning private setters directly
                    existing.ApplyUpdate(
                        score: incoming.Score,
                        reviewerId: incoming.ReviewerId,
                        actionerId: incoming.ModifiedBy
                    );
                }
                else
                {
                    await _sqldbContext.AssessmentAnswerScores.AddAsync(incoming, cancellationToken);
                }
            }
        }

        public async Task<FillAssignment?> FindSelfAssessmentFillAssignmentAsync(int intervalId, int subjectId, CancellationToken cancellationToken)
        {
            return await _sqldbContext.FillAssignments
                .Include(fa => fa.Assessment)
                .FirstOrDefaultAsync(fa => !fa.IsDeleted
                                        && fa.IntervalId == intervalId
                                        && fa.SubjectId == subjectId
                                        && fa.FillerId == subjectId
                                        && fa.Assessment.AssessmentType == "self-assessment",
                                   cancellationToken);
        }

        public async Task<ReviewAssignment?> FindReviewAssignmentAsync(int planId, int intervalId, int assignmentId, int reviewerId, CancellationToken cancellationToken)
        {
            return await _sqldbContext.ReviewAssignments
                .FirstOrDefaultAsync(ra => !ra.IsDeleted
                                        && ra.PlanId == planId
                                        && ra.IntervalId == intervalId
                                        && ra.AssignmentId == assignmentId
                                        && ra.ReviewerId == reviewerId,
                                   cancellationToken);
        }

        public async Task<List<AssessmentAnswer>> GetAnswersByAssignmentIdAsync(int assignmentId, CancellationToken cancellationToken)
        {
            return await _sqldbContext.AssessmentAnswers
                .Where(a => !a.IsDeleted && a.AssignmentId == assignmentId)
                .ToListAsync(cancellationToken);
        }

        public async Task UpsertAnswerScoresAsync(int reviewAssignmentId, List<AssessmentAnswerScore> newScores, CancellationToken cancellationToken)
        {
            var existingScores = await _sqldbContext.AssessmentAnswerScores
                .Where(s => !s.IsDeleted && s.ReviewAssignmentId == reviewAssignmentId)
                .ToListAsync(cancellationToken);

            foreach (var incoming in newScores)
            {
                var existing = existingScores.FirstOrDefault(s => s.AssessmentAnswerId == incoming.AssessmentAnswerId);
                if (existing != null)
                {
                    existing.UpdateScore(incoming.Score, incoming.ModifiedBy);
                }
                else
                {
                    await _sqldbContext.AssessmentAnswerScores.AddAsync(incoming, cancellationToken);
                }
            }
        }

        public async Task<(int? RoleId, string? JobTitle)> GetReviewerMetadataAsync(int employeeId, CancellationToken cancellationToken = default)
        {
            var result = await _sqldbContext.Employee
                .AsNoTracking()
                .Where(e => e.Id == employeeId && e.IsDeleted == false)
                .Select(e => new
                {
                    e.RoleId,
                    JobTitle = e.EmploymentInformation != null ? e.EmploymentInformation.PositionName : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null) return (null, null);

            return (result.RoleId, result.JobTitle);
        }

        public async Task<List<FillAssignment>> FindPeerReviewFillAssignmentsAsync(int intervalId, int subjectId, CancellationToken cancellationToken)
        {
            return await _sqldbContext.FillAssignments
                .Where(fa => !fa.IsDeleted
                          && fa.IntervalId == intervalId
                          && fa.SubjectId == subjectId
                          && fa.Assessment.AssessmentType == "peer-review")
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AssessmentAnswer>> GetAnswersByAssignmentIdsAsync(List<int> assignmentIds, CancellationToken cancellationToken)
        {
            return await _sqldbContext.AssessmentAnswers
                .Where(a => !a.IsDeleted && assignmentIds.Contains(a.AssignmentId))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ReviewAssignment>> FindReviewAssignmentsByAssignmentIdsAsync(List<int> assignmentIds, int reviewerId, CancellationToken cancellationToken)
        {
            return await _sqldbContext.ReviewAssignments
                .Where(ra => !ra.IsDeleted
                          && assignmentIds.Contains(ra.AssignmentId)
                          && ra.ReviewerId == reviewerId)
                .ToListAsync(cancellationToken);
        }

    }
}