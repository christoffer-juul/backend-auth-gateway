using System.ComponentModel.DataAnnotations;

namespace Auth.Gateway.EmailVerification.Client;

public sealed class EmailVerificationApiOptions
{
    public const string SectionName = "EmailVerificationApi";

    [Required]
    [Url]
    public required string BaseUrl { get; init; }

    [Required]
    public required string FunctionKey { get; init; }
}