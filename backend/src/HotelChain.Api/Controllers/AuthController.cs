using HotelChain.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HotelChain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>Signs in as administrator and returns a JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        var result = auth.Login(request);
        return result is null
            ? Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Usuario o contraseña incorrectos.")
            : Ok(result);
    }
}
