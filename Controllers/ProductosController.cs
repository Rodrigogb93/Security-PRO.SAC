using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.Models;
using SecurityPRO.ERP.Web.Repositories;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly ProductoRepository _repo;
    public ProductosController(ProductoRepository repo) => _repo = repo;

    [HttpGet]
    public IActionResult Listar([FromQuery] string? categoria, [FromQuery] string? marca, [FromQuery] string? normativa)
        => Ok(_repo.Listar(categoria, marca, normativa));

    [HttpGet("{id}")]
    public IActionResult Obtener(int id)
    {
        var p = _repo.ObtenerPorId(id);
        return p == null ? NotFound() : Ok(p);
    }

    [HttpGet("categorias")]
    public IActionResult Categorias() => Ok(_repo.ListarCategorias());

    [HttpPost]
    [Authorize(Roles = "Administrador,Almacén")]
    public IActionResult Crear([FromBody] Producto p) => Ok(new { id = _repo.Crear(p) });

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Almacén")]
    public IActionResult Actualizar(int id, [FromBody] Producto p)
    {
        p.IdProducto = id;
        _repo.Actualizar(p);
        return Ok();
    }
}