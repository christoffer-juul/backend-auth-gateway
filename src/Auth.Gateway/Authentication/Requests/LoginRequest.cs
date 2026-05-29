namespace Auth.Gateway.Authentication.Requests;

public sealed record LoginRequest
(
    string Email,
    string Password,
    bool? RememberMe
);
