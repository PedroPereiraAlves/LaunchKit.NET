namespace MyTemplate.Application.Exceptions;

public sealed class ValidationFailedException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationFailedException(IDictionary<string, string[]> errors)
        : base("Dados inválidos.")
    {
        Errors = errors;
    }
}
