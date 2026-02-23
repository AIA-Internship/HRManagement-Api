namespace LeaveManagement.Api.Domain.Models.Table
{
    public class BaseTableModel
    {
        public bool IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedUtcDate { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedUtcDate { get; set; }

        public BaseTableModel()
        {
            IsDeleted = false;
            CreatedBy = 0;
            CreatedUtcDate = DateTime.UtcNow;
            ModifiedBy = 0;
            ModifiedUtcDate = DateTime.UtcNow;
        }
    }
}
