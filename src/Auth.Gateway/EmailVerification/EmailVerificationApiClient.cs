using System.Net;

namespace Auth.Gateway.EmailVerification;

public class EmailVerificationApiClient
{
    private readonly HttpClient _client;

    public EmailVerificationApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> VerifyCodeAsync(string email, string code, CancellationToken ct)
    {
        var upstreamPath = "/api/VerifyCode";

        return await _client.PostAsJsonAsync(upstreamPath, new { email, code }, ct);
    }
}