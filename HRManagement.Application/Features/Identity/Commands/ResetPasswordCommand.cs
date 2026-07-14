using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.SeedWork;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.Identity.Commands;

public record ResetPasswordCommand(ResetPasswordPayload Payload) : IRequest<Result>;

internal sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    ILogger<ResetPasswordCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(ResetPasswordCommandHandler));

        var user = await userRepository.GetUserByEmailAsync(request.Payload.Email, cancellationToken);

        if (user == null)
            return Result.Failure("User not found.");

        var newHashPassword = BCrypt.Net.BCrypt.HashPassword(request.Payload.NewPassword);

        user.ChangePassword(newHashPassword, user.Id);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}