using System.Text.Json.Serialization;

namespace Auth.Gateway.EmailVerification.Responses;

public sealed record VerifyEmailResponse
(
    [property: JsonPropertyName("confirmed")]
    bool Confirmed
);