namespace Auth.Gateway.EmailVerification.Requests;

public sealed record VerifyCodeRequest
(
    string Email, 
    string Code
);