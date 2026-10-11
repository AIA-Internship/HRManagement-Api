namespace HRManagement.Domain.Models.Constants
{
    public static class AssessmentTypeConstant
    {
        /// <summary>
        /// Self-assessment submitted by an employee for their own evaluation.
        /// </summary>
        public const string SelfAssessment = "self-assessment";

        /// <summary>
        /// Peer review assessment submitted by co-workers/peers.
        /// </summary>
        public const string PeerReview = "peer-review";

        /// <summary>
        /// Assessment performed directly by a supervisor or manager.
        /// </summary>
        public const string SupervisorAssessment = "supervisor-assessment";
    }
}