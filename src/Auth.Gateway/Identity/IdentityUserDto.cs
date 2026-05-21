namespace Auth.Gateway.Identity;

public class IdentityUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}