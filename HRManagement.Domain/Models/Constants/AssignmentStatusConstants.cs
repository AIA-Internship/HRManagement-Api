namespace HRManagement.Domain.Models.Constants
{
    public static class AssignmentStatusConstant
    {
        /// <summary>
        /// The assignment is locked because the evaluation period start date has not been reached yet.
        /// </summary>
        public const string Locked = "Locked";

        /// <summary>
        /// The assignment is unlocked, but the user has not interacted with it or written anything yet.
        /// </summary>
        public const string NotStarted = "Not Started";

        /// <summary>
        /// The user has started writing/answering, saved draft/partial answers, but has not submitted yet.
        /// </summary>
        public const string InProgress = "In Progress";

        /// <summary>
        /// The user has submitted a free-text assessment, and it is now waiting for a supervisor to review it.
        /// </summary>
        public const string PendingReview = "Pending Review";

        /// <summary>
        /// A supervisor has started reviewing the free-text answers (partial review in progress).
        /// </summary>
        public const string OnReview = "On Review";

        /// <summary>
        /// A supervisor has successfully finished and submitted all reviews for a free-text assessment.
        /// </summary>
        public const string Reviewed = "Reviewed";

        /// <summary>
        /// Final end state for rating-system assignments (auto-completed upon submission) or fully finalized workflows.
        /// </summary>
        public const string Completed = "Completed";
    }
}