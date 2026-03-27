namespace HRManagement.Api.Domain.Models.Tables
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
        
        public void MarkAsModified(long actionerId)
        {
            this.ModifiedBy = actionerId;
            this.ModifiedUtcDate = DateTime.UtcNow.AddHours(7);
        }

        public void MarkAsCreated(long actionerId)
        {
            this.CreatedBy = actionerId;
            this.CreatedUtcDate = DateTime.UtcNow.AddHours(7);
        }
    }
}