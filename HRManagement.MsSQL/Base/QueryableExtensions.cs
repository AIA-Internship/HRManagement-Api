using HRManagement.Api.Domain.Models.Response.Shared;

using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace HRManagement.MsSQL.Base
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> queryable,
            bool condition,
            Expression<Func<T, bool>> predicate)
        {
            if (condition)
                return queryable.Where(predicate);

            return queryable;

        }

        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            // Pastikan pageNumber minimal 1
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
