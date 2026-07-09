
namespace HRManagement.Domain.Models.Tables;
    public class AssessmentReceiver : BaseTable
    {
        public int Id { get; private set; }

        public int AssessmentId { get; private set; }

        public int EmployeeId { get; private set; }

        public string ReceiverType { get; private set; } = string.Empty;

        public Assessment Assessment { get; private set; } = null!;

        public Employee Employee { get; private set; } = null!;

        protected AssessmentReceiver() { }

        public AssessmentReceiver(
            int assessmentId,
            int employeeId,
            string receiverType,
            int actionerId)
        {
            AssessmentId = assessmentId;
            EmployeeId = employeeId;
            ReceiverType = receiverType;

            MarkAsCreated(actionerId);
            MarkAsModified(actionerId);
        }

        public void ApplyUpdate(
            int? employeeId,
            string? receiverType,
            int actionerId)
        {
            EmployeeId = employeeId ?? EmployeeId;
            ReceiverType = UseIfProvided(receiverType, ReceiverType);

            MarkAsModified(actionerId);
        }
    }
