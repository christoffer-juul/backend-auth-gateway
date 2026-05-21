namespace Auth.Gateway.EmailVerification;

public sealed record VerifyCodeRequest
(
    string Email, 
    string Code
);