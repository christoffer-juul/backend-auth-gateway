namespace Auth.Gateway.Authentication;

    public sealed record LoginResponse(
        bool Success,
        string? Token = null,
        string? ErrorMessage = null,
        string? UserId = null
    );
