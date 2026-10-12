using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.DTOs;
using SecurityPRO.ERP.Web.Services;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = _authService.Login(request, ip);
        if (result == null) return Unauthorized(new { message = "Credenciales inválidas" });
        return Ok(result);
    }

    [HttpGet("validar")]
    [Authorize]
    public IActionResult Validar() => Ok(new { valido = true, rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value });
}