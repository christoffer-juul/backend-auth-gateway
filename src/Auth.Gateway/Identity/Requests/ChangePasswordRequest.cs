namespace Auth.Gateway.Identity.Requests;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
