using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.Repositories;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SsomaController : ControllerBase
{
    private readonly SsomaRepository _repo;

    public SsomaController(SsomaRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("resumen")]
    public IActionResult Resumen()
    {
        return Ok(_repo.Resumen());
    }

    [HttpGet("fichas")]
    public IActionResult Fichas()
    {
        return Ok(_repo.ListarFichas());
    }

    [HttpPost("fichas")]
    public IActionResult CrearFicha([FromBody] CrearFichaRequest request)
    {
        try
        {
            var id = _repo.CrearFicha(
                request.IdProducto,
                request.NombreArchivo,
                request.UrlArchivo,
                request.Version
            );

            return Ok(new
            {
                id,
                message = "Ficha técnica registrada correctamente"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("capacitaciones")]
    public IActionResult Capacitaciones()
    {
        return Ok(_repo.ListarCapacitaciones());
    }

    [HttpPost("capacitaciones")]
    public IActionResult CrearCapacitacion(
        [FromBody] CrearCapacitacionRequest request)
    {
        try
        {
            var id = _repo.CrearCapacitacion(
                request.IdCliente,
                request.IdUsuarioInstructor,
                request.Tema,
                request.FechaCapacitacion,
                request.Lugar,
                request.HoraInicio,
                request.HoraFin
            );

            return Ok(new
            {
                id,
                message = "Capacitación registrada correctamente"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CrearFichaRequest
{
    public int IdProducto { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string UrlArchivo { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public class CrearCapacitacionRequest
{
    public int IdCliente { get; set; }
    public int? IdUsuarioInstructor { get; set; }
    public string Tema { get; set; } = string.Empty;
    public DateTime FechaCapacitacion { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public TimeSpan? HoraInicio { get; set; }
    public TimeSpan? HoraFin { get; set; }
}