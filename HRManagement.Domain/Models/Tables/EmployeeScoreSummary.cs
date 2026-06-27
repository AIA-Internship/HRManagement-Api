using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Tables
{
    public class EmployeeScoreSummary : BaseTable
    {
        public long Id { get; private set; }
        public long InternId { get; private set; }
        public long PlanerId { get; private set; }
        public string InternRole { get; private set; } = string.Empty;

        public int Period { get; private set; }

        public decimal TechScore { get; private set; }

        public decimal SoftSkillScore { get; private set; }

        public decimal SelfAssessmentScore { get; private set; }

        public decimal PeerReviewScore { get; private set; }

        protected EmployeeScoreSummary() { }

        public EmployeeScoreSummary(
            long internId,
            long planerId,
            string internRole,
            int period,
            decimal techScore,
            decimal softSkillScore,
            decimal selfAssessmentScore,
            decimal peerReviewScore,
            int actionerId)
        {
            InternId = internId;
            PlanerId = planerId;
            InternRole = internRole;
            Period = period;
            TechScore = techScore;
            SoftSkillScore = softSkillScore;
            SelfAssessmentScore = selfAssessmentScore;
            PeerReviewScore = peerReviewScore;

            MarkAsCreated(actionerId);
            MarkAsModified(actionerId);
        }

        public void ApplyUpdate(
            decimal? techScore,
            decimal? softSkillScore,
            decimal? selfAssessmentScore,
            decimal? peerReviewScore,
            int actionerId)
        {
            TechScore = techScore ?? TechScore;
            SoftSkillScore = softSkillScore ?? SoftSkillScore;
            SelfAssessmentScore = selfAssessmentScore ?? SelfAssessmentScore;
            PeerReviewScore = peerReviewScore ?? PeerReviewScore;

            MarkAsModified(actionerId);
        }
    }
}
