namespace Auth.Gateway.Identity.Dtos;

public class IdentityUserDto
{
    public string Id { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public string Email { get; set; } = string.Empty;
}