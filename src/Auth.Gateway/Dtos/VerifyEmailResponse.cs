namespace Auth.Gateway.Dtos;

public sealed record VerifyEmailResponse
(
    string Email, 
    bool EmailConfirmed
);