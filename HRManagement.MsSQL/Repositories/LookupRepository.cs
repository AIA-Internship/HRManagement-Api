using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Repositories;

public class LookupRepository : BaseRepository<Lookup>, ILookupRepository
{
    public LookupRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<List<LookupResponseDto>> GetLookupListAsync(string category, CancellationToken cancellationToken = default)
    {
        var query = _dbContext
            .AsNoTracking()
            .Where(p => p.IsActive && p.Category.ToLower() == category.ToLower());

        var response = await query
            .Select(p => new LookupResponseDto(
                p.Value,
                p.DisplayName
            ))
            .ToListAsync(cancellationToken);

        return response;
    }
}
