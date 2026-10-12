using SecurityPRO.ERP.Web.DTOs;
using SecurityPRO.ERP.Web.Models;
using SecurityPRO.ERP.Web.Repositories;

namespace SecurityPRO.ERP.Web.Services;

public class AuthService
{
    private readonly UsuarioRepository _usuarioRepo;
    private readonly JwtService _jwtService;
    private readonly AuditoriaService _auditoria;

    public AuthService(UsuarioRepository usuarioRepo, JwtService jwtService, AuditoriaService auditoria)
    {
        _usuarioRepo = usuarioRepo;
        _jwtService = jwtService;
        _auditoria = auditoria;
    }

    public LoginResponse? Login(LoginRequest request, string ip)
    {
        var usuario = _usuarioRepo.ObtenerPorEmail(request.Email);
        if (usuario == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash)) return null;

        var token = _jwtService.GenerarToken(usuario);
        _auditoria.Registrar(usuario.IdUsuario, ip, "Auth", "Login exitoso", null, null);

        return new LoginResponse
        {
            Token = token,
            IdUsuario = usuario.IdUsuario,
            Nombres = usuario.Nombres,
            Email = usuario.Email,
            Rol = usuario.NombreRol
        };
    }
}