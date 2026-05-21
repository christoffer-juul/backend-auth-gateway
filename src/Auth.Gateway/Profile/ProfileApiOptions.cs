using System.ComponentModel.DataAnnotations;

namespace Auth.Gateway.Profile;

public sealed class ProfileApiOptions
{
    public const string SectionName = "ProfileApi";

    [Required]
    [Url]
    public required string BaseUrl { get; init; }

    [Required]
    public required string ApiKey { get; init; }
}
