using HRManagement.Api.Domain.Models.Config;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

using System.Data;

namespace HRManagement.Api.Repositories.Base
{
    public class BaseQueryRepository
    {
        private readonly IConfiguration configuration;
        private readonly AppSetting appSetting;

        protected IDbConnection SqlConnDB => new SqlConnection(appSetting.DBConnectionString);

        public BaseQueryRepository(IConfiguration configuration, IOptions<AppSetting> _appSetting)
        {
            this.configuration = configuration;
            this.appSetting = _appSetting.Value;
        }
    }
}