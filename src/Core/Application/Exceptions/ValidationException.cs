namespace Application.Exceptions;

public class ValidationException : InvalidOperationException
{
    public required Dictionary<string, string[]> Errors { get; init; }
}
