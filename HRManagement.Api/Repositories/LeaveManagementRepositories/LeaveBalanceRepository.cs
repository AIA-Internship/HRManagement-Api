using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces.LeaveManagementInterface;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveBalance;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HRManagement.Api.Repositories.LeaveManagementRepositories
{
    public class LeaveBalanceRepository : BaseRepository, ILeaveBalanceRepository
    {
        private readonly AppDbContext _dbContext;
        public LeaveBalanceRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> createLeaveBalance(LeaveBalanceModel leaveBalance)
        {
            try
            {
                await _dbContext.leaveBalanceModels.AddAsync(leaveBalance);

                var affectedRows = await _dbContext.SaveChangesAsync();

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> deleteLeaveBalance(int id)
        {
            try
            {
                var res = await _dbContext.leaveBalanceModels.FindAsync(id);
                if (res == null) return false;

                _dbContext.leaveBalanceModels.Remove(res);

                var affectedRows = await _dbContext.SaveChangesAsync();

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<LeaveBalanceModel> getLeaveBalanceById(int id)
        {
            try
            {
                var res = await _dbContext.leaveBalanceModels.FirstOrDefaultAsync(x => x.EmployeeId == id);
                if (res == null) return null;

                return res;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> updateLeaveBalance(LeaveBalanceModel leaveBalance)
        {
            try
            {
                _dbContext.leaveBalanceModels.Update(leaveBalance);
                return await _dbContext.SaveChangesAsync() > 0;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
