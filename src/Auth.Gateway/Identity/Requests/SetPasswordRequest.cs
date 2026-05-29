namespace Auth.Gateway.Identity.Requests;

public sealed record SetPasswordRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName
);