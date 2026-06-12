using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.MsSQL.Base;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Repositories;

public class AuthorizationRepository : BaseRepository<Users>, IAuthorizationRepository
{
    public AuthorizationRepository(AppDbContext dbContext) : base(dbContext) { }

    
}