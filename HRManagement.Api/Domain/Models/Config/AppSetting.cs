namespace HRManagement.Api.Domain.Models.Config
{
    public class AppSetting
    {
        public bool SwaggerEnabled { get; set; }
        public bool UseHttpsRedirection { get; set; }
        public string? CorsOrigin { get; set; }
        public string? DbConnectionString { get; set; }
        public string? FileLogPath { get; set; }
        public string? FileStoragePath { get; set; }
        public string? AuthenticationCookiePath { get; set; }
        public int AuthenticationCookieExpireTimeSpan { get; set; }
        public bool IsDeployedOnAnz { get; set; }
        public AppSettingNotification Notification { get; set; } = new();
        public AppSettingJwt Jwt { get; set; } = new();
        public BaseUrl BaseUrl { get; set; } = new();

        public AppSettingStorage AzureStorage { get; set; } = new();
        public string? DataReportExportJobName { get; set; }
        public string? ApiKey { get; set; }
    }

    public class AppSettingJwt
    {
        public string? Key { get; set; }
        public int? DurationInMinutes { get; set; }
        public string? Issuer { get; set; }
        public string? AudienceWeb { get; set; }
        public string? Audience1 { get; set; }
        public string? Audience2 { get; set; }
        public string? Audience3 { get; set; }
        public string? Audience4 { get; set; }
    }

    public class AppSettingNotification
    {
        public string? ServiceUrl { get; set; }
        public string? Application { get; set; }
        public string? MessageFrom { get; set; }
        public string? BaseReferenceUrl { get; set; }
    }

    public class BaseUrl
    {
        public string? Backend { get; set; }
        public string? Frontend { get; set; }
    }

    public class AppSettingStorage
    {
        public string? ConnectionString { get; set; }
        public string? ContainerName { get; set; }
    }
}
