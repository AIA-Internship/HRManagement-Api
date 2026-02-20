using FluentValidation;

using MediatR;

namespace HRManagement.Api.Extensions
{
    public class ValidationPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationPipelineBehaviour(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);

            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            //Get Error Message 
            var errorMessage = string.Join("\n", failures.Select(e => e.ErrorMessage));

            if (failures.Count > 0)
                throw new ApplicationException(errorMessage);

            return await next();
        }

    }
}
