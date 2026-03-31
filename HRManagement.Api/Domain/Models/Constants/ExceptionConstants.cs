namespace HRManagement.Api.Domain.Models.Constants
{
    public static class ExceptionConstants
    {
        public const string NotAuthorizedExcepction = "Unauthorized. Access token is missing or invalid.";
        public const string Forbidden = "You do not have the required permissions to perform this action.";
        public const string NotFound = "The requested resource could not be found.";
        public const string BadRequest = "The request was invalid. Please check your data and try again.";
        public const string InternalServerError = "An unexpected error occurred on the server. Please try again later.";
        public const string EmployeeNotFound = "Employee not found.";
        public const string LookupCategoryNotFound = "No active lookup category found for {0}.";
        public const string UpdateRequestNotFound = "No update request found";
        public const string UserNotFound = "User not found";
        public const string NotAuthorized = "Invalid email or password";
        public const string Conflict = "You cannot approve your own request.";
        public const string ForbiddenUpload = "You are not authorized to upload files to another employee's profile.";
        public const string BadRequestUpload = "No files were provided for upload.";
    }
}
