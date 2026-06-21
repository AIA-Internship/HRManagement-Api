using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HRManagement.Api.Controllers;

public abstract class ValidateController<T> : ControllerBase
{
    protected readonly IMediator _mediator;
    protected readonly ILogger<T> _logger;

    protected ValidateController(IMediator mediator, ILogger<T> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
}
