namespace HRManagement.Application.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    string? Email  { get; }
    string? Role { get; }
}