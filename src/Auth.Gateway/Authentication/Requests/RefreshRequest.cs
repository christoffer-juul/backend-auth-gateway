namespace Auth.Gateway.Authentication.Requests;

public sealed record RefreshRequest
(
    string RefreshToken
);