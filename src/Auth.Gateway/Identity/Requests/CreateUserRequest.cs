namespace Auth.Gateway.Identity.Requests;

public sealed record CreateUserRequest
(
    string Email,
    string Role
);