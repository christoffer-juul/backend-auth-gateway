namespace Auth.Gateway.Identity;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
