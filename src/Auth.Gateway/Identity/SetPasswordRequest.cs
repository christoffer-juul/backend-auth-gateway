namespace Auth.Gateway.Identity;

public sealed record SetPasswordRequest(
    string Password,
    string FirstName,
    string LastName
);