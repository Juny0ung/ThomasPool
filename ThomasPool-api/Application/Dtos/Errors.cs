namespace ThomasPool.Application.Dtos;

public enum ErrorType
{
    Validation,     // 400 Bad Request
    NotFound,       // 404 Not Found
    Conflict,       // 409 Conflict
    Failure         // 500 Internal Server Error
}

public record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new Error("", "", ErrorType.Failure);
}

public static class Errors
{
    public static class Profile
    {
        public static readonly Error NotFound = new ("p_nf", "Profile not found", ErrorType.NotFound);
        public static readonly Error Invalid = new ("p_iv", "Invalid profile", ErrorType.Validation);
    }

    public static class ProfileForm
    {
        public static readonly Error NotFound = new ("pf_nf", "Profile form not found", ErrorType.NotFound);
    }

    public static class General
    {
        public static readonly Error Unknown = new ("uk", "Unknown error", ErrorType.Failure);
    }
}

public static class ErrorTypeExtensions
{
    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
}