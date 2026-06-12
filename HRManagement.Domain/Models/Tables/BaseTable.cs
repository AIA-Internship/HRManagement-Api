namespace HRManagement.Domain.Models.Tables
{
    public class BaseTable
    {
        public virtual Users? User { get; set; }
        public bool IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedUtcDate { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedUtcDate { get; set; }

        public BaseTable()
        {
            IsDeleted = false;
            CreatedBy = 1;
            CreatedUtcDate = DateTime.UtcNow;
            ModifiedBy = 1;
            ModifiedUtcDate = DateTime.UtcNow;
        }

        public void SetDelete(int actioner)
        {
            IsDeleted = true;
            ModifiedBy = actioner;
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