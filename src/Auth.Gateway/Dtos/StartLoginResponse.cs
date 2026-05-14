namespace Auth.Gateway.Dtos;

public sealed record StartLoginResponse
(
    string Email, 
    bool EmailConfirmed
);