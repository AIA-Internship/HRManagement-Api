namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class CreateQuizConfigurationDto
    {
        public int moduleId { get; set; }
        public int mcCount { get; set; }
        public int essayCount { get; set; }
        public int mcWeight { get; set; }
        public int essayWeight { get; set; }
        public int minimumPassingScore { get; set; }
        public List<SaveQuestionDto> questions { get; set; } = new();
    }
    public class SaveQuestionDto
    {
        public string questionText { get; set; } = null!;
        public string questionType { get; set; } = null!; 
        public int sortOrder { get; set; }
        public List<SaveOptionDto> options { get; set; } = new(); 
    }

    public class SaveOptionDto
    {
        public string optionLetter { get; set; } = null!; 
        public string optionText { get; set; } = null!;
        public bool isCorrect { get; set; }
    }
}
