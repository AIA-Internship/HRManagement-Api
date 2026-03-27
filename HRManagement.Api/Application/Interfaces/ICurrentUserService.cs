namespace HRManagement.Api.Application.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    string? Email  { get; }
}