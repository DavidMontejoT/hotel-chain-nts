using System.ComponentModel.DataAnnotations;

namespace HotelChain.Application.Auth;

public sealed class LoginRequest
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public sealed record LoginResponse(string Token, DateTime ExpiresAt, string Username, string Role);

public interface IAuthService
{
    /// <summary>Returns a token when the credentials are valid; otherwise null.</summary>
    LoginResponse? Login(LoginRequest request);
}

public static class Roles
{
    public const string Admin = "Admin";
}
