using System.ComponentModel.DataAnnotations;

namespace Auth.Gateway.Authentication.Client;

public sealed class JwtApiOptions
{
    public const string SectionName = "JwtApi";

    [Required]
    [Url]
    public required string BaseUrl { get; init; }

    [Required]
    public required string ApiKey { get; init; }
}
