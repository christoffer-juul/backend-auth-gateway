namespace Auth.Gateway.Dtos;

public sealed record VerifyCodeRequest
(
    string Email, 
    string Code
);