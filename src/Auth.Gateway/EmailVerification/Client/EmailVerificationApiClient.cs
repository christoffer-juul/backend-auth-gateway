using System.Net;
using System.Text.Json;

namespace Auth.Gateway.EmailVerification.Client;

public class EmailVerificationApiClient
{
    private readonly HttpClient _client;

    public EmailVerificationApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> VerifyCodeAsync(string email, string code, CancellationToken ct)
    {
        var upstreamPath = "api/VerifyCode";

        var payload = new
        {
            email,
            code
        };

        return await _client.PostAsJsonAsync(upstreamPath, payload, ct);
    }
}