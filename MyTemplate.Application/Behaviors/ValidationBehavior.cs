using FluentValidation;
using MediatR;
using MyTemplate.Application.Exceptions;

namespace MyTemplate.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errors = failures
            .GroupBy(failure => ToCamelCase(failure.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

        throw new ValidationFailedException(errors);
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "request";

        return char.IsLower(name[0])
            ? name
            : char.ToLowerInvariant(name[0]) + name[1..];
    }
}
