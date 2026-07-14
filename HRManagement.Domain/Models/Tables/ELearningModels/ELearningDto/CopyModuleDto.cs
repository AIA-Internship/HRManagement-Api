namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class CopyModuleDto
    {
        public int sourceModuleId { get; set; } 
        public int targetBatchId { get; set; }  
        public int currentUserId { get; set; }  
    }
}