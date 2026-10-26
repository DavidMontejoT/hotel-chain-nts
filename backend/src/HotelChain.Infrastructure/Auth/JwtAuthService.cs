using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HotelChain.Application.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HotelChain.Infrastructure.Auth;

/// <summary>
/// Authenticates the administrator against configured credentials and issues a JWT.
/// It can be swapped for a real identity provider without touching the upper layers.
/// </summary>
public sealed class JwtAuthService(IOptions<JwtOptions> jwt, IOptions<AdminUserOptions> admin, TimeProvider clock) : IAuthService
{
    public LoginResponse? Login(LoginRequest request)
    {
        var expected = admin.Value;
        if (string.IsNullOrEmpty(expected.Username) || string.IsNullOrEmpty(expected.Password))
            return null;

        if (!FixedEquals(request.Username, expected.Username) || !FixedEquals(request.Password, expected.Password))
            return null;

        var options = jwt.Value;
        var expires = clock.GetUtcNow().UtcDateTime.AddMinutes(options.ExpirationMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, expected.Username),
                new Claim(ClaimTypes.Name, expected.Username),
                new Claim(ClaimTypes.Role, Roles.Admin),
            ],
            expires: expires,
            signingCredentials: credentials);

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expires, expected.Username, Roles.Admin);
    }

    private static bool FixedEquals(string a, string b)
        => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
