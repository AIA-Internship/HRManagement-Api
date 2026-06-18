using HRManagement.Domain.Models.Config;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using System.Data;

namespace HRManagement.MsSQL.Base
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