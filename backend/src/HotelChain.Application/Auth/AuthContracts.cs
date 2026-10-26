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
    /// <summary>Devuelve un token si las credenciales son válidas; null en caso contrario.</summary>
    LoginResponse? Login(LoginRequest request);
}

public static class Roles
{
    public const string Admin = "Admin";
}
