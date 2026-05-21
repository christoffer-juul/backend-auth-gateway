using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Auth.Gateway.Identity;

public sealed record PasswordDto(

    [property: JsonPropertyName("password")]

    [property: Required(ErrorMessage = "Password is required.")]

    [property: RegularExpression(
        @"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and include an uppercase letter, a number, and a special character."
    )]

    string Password
);