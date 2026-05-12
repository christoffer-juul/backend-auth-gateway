namespace Auth.Gateway.Dtos;

public sealed record CreateUserRequest
(
    string Email,
    string Role
);