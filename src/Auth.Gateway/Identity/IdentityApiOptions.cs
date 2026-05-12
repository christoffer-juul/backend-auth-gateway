using System.ComponentModel.DataAnnotations;

namespace Auth.Gateway.Identity;

public sealed class IdentityApiOptions
{
    public const string SectionName = "IdentityApi";

    [Required]
    [Url]
    public required string BaseUrl { get; init; }
}