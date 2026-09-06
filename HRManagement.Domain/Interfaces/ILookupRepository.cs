using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;

namespace HRManagement.Domain.Interfaces;

public interface ILookupRepository : IBaseRepository<Lookup>
{
    Task<List<LookupResponseDto>> GetLookupListAsync(string category, CancellationToken cancellationToken = default);
}
