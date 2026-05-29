namespace Auth.Gateway.Authentication.Responses;

    public sealed record LoginResponse(
        bool Success,
        string? Token = null,
        string? ErrorMessage = null,
        string? UserId = null
    );
