using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;

using System.Linq.Expressions;

namespace HRManagement.Domain.Interfaces
{
    public interface IAssessmentGroupRepository:IBaseRepository<AssessmentGroup>
    {
        Task<List<AssessmentGroupDto>>GetByAssessmentIdAsync(int assessmentId,CancellationToken cancellationToken);

    }
}
