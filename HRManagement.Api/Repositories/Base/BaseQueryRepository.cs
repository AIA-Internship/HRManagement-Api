using System.Data;
using HRManagement.Api.Domain.Models.Config;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HRManagement.Api.Repositories.Base
{
    public class BaseQueryRepository
    {
        private readonly IConfiguration configuration;
        private readonly AppSetting appSetting;

        protected IDbConnection SqlConnDB => new SqlConnection(appSetting.DbConnectionString);

        public BaseQueryRepository(IConfiguration configuration, IOptions<AppSetting> _appSetting)
        {
            this.configuration = configuration;
            this.appSetting = _appSetting.Value;
        }
    }
}