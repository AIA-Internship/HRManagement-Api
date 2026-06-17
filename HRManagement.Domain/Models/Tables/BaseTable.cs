namespace HRManagement.Domain.Models.Tables
{
    public class BaseTable
    {
        public virtual Users? User { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedUtcDate { get; set; }
        public int ModifiedBy { get; set; }
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

        public void MarkAsModified(int actionerId)
        {
            this.ModifiedBy = actionerId;
            this.ModifiedUtcDate = DateTime.UtcNow.AddHours(7);
        }

        public void MarkAsCreated(int actionerId)
        {
            this.CreatedBy = actionerId;
            this.CreatedUtcDate = DateTime.UtcNow.AddHours(7);
        }

        public static bool IsChanged(string? newValue, string? currentValue)
        {
            if (string.IsNullOrWhiteSpace(newValue)) return false;
            return !string.Equals(newValue.Trim(), currentValue?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsChanged<T>(T? newValue, T currentValue) where T : struct
        {
            if (!newValue.HasValue) return false;
            return !newValue.Value.Equals(currentValue);
        }

        public static string UseIfProvided(string? newValue, string currentValue) => string.IsNullOrWhiteSpace(newValue) ? currentValue : newValue;
    }
}