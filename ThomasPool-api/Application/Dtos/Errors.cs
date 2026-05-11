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
    public static class Admin
    {
        public static readonly Error NotFound = new ("a_nf", "Account not found", ErrorType.NotFound);
        public static readonly Error NotApproved = new ("a_na", "Not approved", ErrorType.Validation);
        public static readonly Error Invalid = new ("a_iv", "Invalid account", ErrorType.Validation);
        public static readonly Error Conflict = new ("a_cf", "Id already exists", ErrorType.Conflict);
    }

    public static class Profile
    {
        public static readonly Error NotFound = new ("p_nf", "Profile not found", ErrorType.NotFound);
        public static readonly Error Invalid = new ("p_iv", "Invalid profile", ErrorType.Validation);
        public static readonly Error Conflict = new ("p_cf", "Profile already exists", ErrorType.Conflict);
    }

    public static class ProfileForm
    {
        public static readonly Error NotFound = new ("pf_nf", "Profile form not found", ErrorType.NotFound);
        public static readonly Error Invalid = new ("pf_iv", "Invalid profile form", ErrorType.Validation);
        public static readonly Error Conflict = new ("pf_cf", "Version conflict, please retry", ErrorType.Conflict);
    }

    public static class Photo
    {
        public static readonly Error NotFound = new("ph_nf", "Photo not found", ErrorType.NotFound);
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