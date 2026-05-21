namespace Auth.Gateway.Identity;

public sealed record CreateUserRequest
(
    string Email,
    string Role
);