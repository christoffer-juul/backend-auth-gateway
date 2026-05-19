namespace Auth.Gateway.Dtos;

public sealed record LoginRequest
(
    string Email, 
    string Password,
    bool? RememberMe
);
