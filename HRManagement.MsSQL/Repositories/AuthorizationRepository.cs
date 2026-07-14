using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;

namespace HRManagement.MsSQL.Repositories;

public class AuthorizationRepository : BaseRepository<Users>, IAuthorizationRepository
{
    public AuthorizationRepository(AppDbContext dbContext) : base(dbContext) { }

    
}