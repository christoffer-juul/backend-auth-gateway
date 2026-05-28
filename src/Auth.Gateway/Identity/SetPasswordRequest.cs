namespace Auth.Gateway.Identity;

public sealed record SetPasswordRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName
);