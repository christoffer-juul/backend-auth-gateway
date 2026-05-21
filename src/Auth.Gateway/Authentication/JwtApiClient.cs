namespace Auth.Gateway.Authentication;

public class JwtApiClient
{
    private readonly HttpClient _client;

    public JwtApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> GenerateTokenAsync(string email, string userId, string role, CancellationToken ct)
    {
        var payload = new
        {
            email = email,
            userId = userId,
            role = role
        };

        return await _client.PostAsJsonAsync("/api/token/generate", payload, ct);
    }
}
