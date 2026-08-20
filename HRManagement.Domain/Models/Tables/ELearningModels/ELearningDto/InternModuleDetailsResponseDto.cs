using System.Collections.Generic;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class InternModuleDetailsResponseDto
    {
        public string InternName { get; set; } = null!;
        public List<InternModuleDetailDto> Modules { get; set; } = new List<InternModuleDetailDto>();
    }
}
