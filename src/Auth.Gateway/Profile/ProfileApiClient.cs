using System.Text;
using System.Text.Json;

namespace Auth.Gateway.Profile;

public class ProfileApiClient
{
    private readonly HttpClient _client;

    public ProfileApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> ForwardJsonAsync<T>(HttpMethod method, string upstreamPath, T body, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.SendAsync(new HttpRequestMessage(method, upstreamPath) { Content = content }, ct);
    }

    public Task<HttpResponseMessage> SetNamesAsync(ProfileDto body, CancellationToken ct)
        => ForwardJsonAsync(HttpMethod.Post, "/api/profiles/names", body, ct);
}
