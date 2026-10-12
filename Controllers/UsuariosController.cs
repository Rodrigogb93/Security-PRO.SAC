using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.DTOs;
using SecurityPRO.ERP.Web.Models;
using SecurityPRO.ERP.Web.Repositories;
using SecurityPRO.ERP.Web.Services;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioRepository _repo;
    private readonly AuditoriaService _auditoria;

    public UsuariosController(UsuarioRepository repo, AuditoriaService auditoria)
    {
        _repo = repo;
        _auditoria = auditoria;
    }

    [HttpGet]
    public IActionResult Listar() => Ok(_repo.ListarTodos());

    [HttpGet("roles")]
    public IActionResult Roles() => Ok(_repo.ListarRoles());

    [HttpPost]
    public IActionResult Crear([FromBody] CrearUsuarioRequest request)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);
        var u = new Usuario
        {
            IdRol = request.IdRol,
            Nombres = request.Nombres,
            Email = request.Email,
            PasswordHash = hash,
            Telefono = request.Telefono,
            Dni = request.Dni
        };
        var id = _repo.Crear(u);
        _auditoria.Registrar(int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value),
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "", "Usuarios", $"Creó usuario {request.Email}", null, null);
        return Ok(new { id });
    }

    [HttpPut("{id}")]
    public IActionResult Actualizar(int id, [FromBody] Usuario u)
    {
        u.IdUsuario = id;
        _repo.Actualizar(u);
        return Ok();
    }

    [HttpPut("{id}/password")]
    public IActionResult CambiarPassword(int id, [FromBody] string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password, 12);
        _repo.ActualizarPassword(id, hash);
        return Ok();
    }
}