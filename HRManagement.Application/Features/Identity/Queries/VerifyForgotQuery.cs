using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.SeedWork;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.Identity.Queries;

public record VerifyForgotResponse(string Result);
public record VerifyForgotQuery(VerifyForgotPayload Payload) : IRequest<Result>;

internal sealed class VerifyForgotQueryHandler(
    IUserRepository userRepository,
    ILogger<VerifyForgotQueryHandler> logger) : IRequestHandler<VerifyForgotQuery, Result>
{
    public async Task<Result> Handle(VerifyForgotQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(VerifyForgotQueryHandler));

        var payload = request.Payload;

        var response = await userRepository.IsUserVerifiedAsync(payload.Email, payload.DateOfBirth, cancellationToken);

        if (response)
            return Result.Success(new VerifyForgotResponse("Identity verified"));
        else                                                                      
            return Result.Failure("Verification failed. Information does not match our records.");
    }
}