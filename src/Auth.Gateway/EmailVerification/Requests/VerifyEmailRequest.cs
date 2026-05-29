namespace Auth.Gateway.EmailVerification.Requests;

public sealed record VerifyEmailRequest
(
    string Email
);