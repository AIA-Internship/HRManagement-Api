using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningMapping
{
    public class InternProfileMapping
    {
        public static ReadInternProfileDto MapToReadDto(InternProfileModel model)
        {
            return new ReadInternProfileDto
            {
                userId = model.UserId,
                internRole = model.InternRole,
                internTeam = model.InternTeam
            };
        }
    }
}
