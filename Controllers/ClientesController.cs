using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.Models;
using SecurityPRO.ERP.Web.Repositories;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly ClienteRepository _repo;
    public ClientesController(ClienteRepository repo) => _repo = repo;

    [HttpGet]
    public IActionResult Listar([FromQuery] string? search) => Ok(_repo.Listar(search));

    [HttpGet("{id}")]
    public IActionResult Obtener(int id)
    {
        var c = _repo.ObtenerPorId(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpGet("por-ruc/{ruc}")]
    public IActionResult PorRuc(string ruc)
    {
        var c = _repo.ObtenerPorRuc(ruc);
        return c == null ? NotFound(new { message = "Cliente no encontrado" }) : Ok(c);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Comercial")]
    public IActionResult Crear([FromBody] Cliente c) => Ok(new { id = _repo.Crear(c) });

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Comercial")]
    public IActionResult Actualizar(int id, [FromBody] Cliente c)
    {
        c.IdCliente = id;
        _repo.Actualizar(c);
        return Ok();
    }
}