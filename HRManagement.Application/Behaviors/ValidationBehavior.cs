using CSharpFunctionalExtensions;

using FluentValidation;

using MediatR;

namespace HRManagement.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    // Inject semua validator yang ada
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(); // Lanjut ke Handler jika tidak ada aturan validasi

        var context = new ValidationContext<TRequest>(request);

        // Jalankan semua validasi secara paralel
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Kumpulkan semua error
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            // Gabungkan pesan error
            var errorMessage = string.Join(" | ", failures.Select(f => f.ErrorMessage));

            // Karena sistemmu menggunakan CSharpFunctionalExtensions (Result<T>),
            // Kita gunakan Reflection untuk mengembalikan Result.Failure<T>(errorMessage) secara dinamis
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse).GetGenericArguments()[0];
                var failureMethod = typeof(Result)
                    .GetMethods()
                    .First(m => m.Name == "Failure" && m.IsGenericMethod)
                    .MakeGenericMethod(resultType);

                return (TResponse)failureMethod.Invoke(null, new object[] { errorMessage })!;
            }

            // Fallback jika kembaliannya bukan Result<>
            throw new ValidationException(failures);
        }

        // Jika tidak ada error validasi, teruskan ke Handler utama
        return await next();
    }
}
