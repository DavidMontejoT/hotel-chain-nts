namespace HotelChain.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "HotelChain.Api";
    public string Audience { get; set; } = "HotelChain.Client";

    /// <summary>Symmetric signing key (at least 32 characters). In production it must come from an environment variable or secret store.</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;
}

public sealed class AdminUserOptions
{
    public const string SectionName = "AdminUser";

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
