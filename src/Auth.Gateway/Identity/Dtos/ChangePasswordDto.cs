using System.Text.Json.Serialization;

namespace Auth.Gateway.Identity.Dtos;

public sealed record ChangePasswordDto(
    string Email,
    string CurrentPassword,
    string NewPassword
);
