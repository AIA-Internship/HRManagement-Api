namespace HRManagement.Domain.Models.Response.Shared
{
    public class FileDownloadResult
    {
        public byte[] Content { get; set; } = [];
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}