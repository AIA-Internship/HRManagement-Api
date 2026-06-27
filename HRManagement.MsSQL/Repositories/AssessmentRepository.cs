using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;


namespace HRManagement.MsSQL.Repositories
{
    public class AssessmentRepository
    : BaseRepository<Assessment>, IAssessmentRepository
    {
        public AssessmentRepository(AppDbContext dbContext)
            : base(dbContext) { }
        
        public async Task<List<SelfAssessmentDto>> GetSelfAssessmentsByPlanIdAsync(int planId,CancellationToken cancellationToken)
        {
            return await _sqldbContext.Assessments
                .AsNoTracking()
                .Where(x =>
                    x.PlanId == planId &&
                    x.AssessmentType == "self-assessment" &&
                    !x.IsDeleted)
                .Select(x => new SelfAssessmentDto(
                    x.Id,
                    x.SubjectJobTitle,
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
                .ToListAsync(cancellationToken);
        }

        public async Task<List<PeerReviewDto>> GetPeerReviewsByPlanIdAsync(int planId,CancellationToken cancellationToken)
        {
            return await _sqldbContext.Assessments
                .AsNoTracking()
                .Where(x =>
                    x.PlanId == planId &&
                    x.AssessmentType == "peer-review" &&
                    !x.IsDeleted)
                .Select(x => new PeerReviewDto(
                    x.Id,
                    x.SubjectJobTitle,
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
                                    m.Employee.EmploymentInformation != null
                                        ? m.Employee.EmploymentInformation.DisplayId ?? ""
                                        : "",
                                    m.Employee.FullName
                                ))
                                .ToList()
                        ))
                        .ToList()
                ))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SupervisorAssessmentDto>>
    GetSupervisorAssessmentsByPlanIdAsync(
        int planId,
        CancellationToken cancellationToken)
        {
            return await _sqldbContext.Assessments
                .AsNoTracking()
                .Where(x =>
                    x.PlanId == planId &&
                    x.AssessmentType == "supervisor-assessment" &&
                    !x.IsDeleted)
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
                .ToListAsync(cancellationToken);
        }

    }

}
