using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningMapping
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
