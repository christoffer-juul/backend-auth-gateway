using System.Text.Json.Serialization;

namespace Auth.Gateway.Identity;

public sealed record ChangePasswordDto(
    string Email,
    string CurrentPassword,
    string NewPassword
);
