using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Extensions
{
    public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

        public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            
            _logger.LogInformation("CQRS: Handling {RequestName}", requestName);
            
            var stopwatch = Stopwatch.StartNew();
            
            var response = await next();
            
            stopwatch.Stop();
            
            _logger.LogInformation("CQRS: Handled {RequestName} in {Elapsed}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
    }
}
