using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.Models;
using SecurityPRO.ERP.Web.Repositories;
using SecurityPRO.ERP.Web.Services;
using System.Security.Claims;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Comercial")]
public class CotizacionesController : ControllerBase
{
    private readonly CotizacionRepository _repo;
    private readonly PdfService _pdf;
    private readonly AuditoriaService _auditoria;

    public CotizacionesController(CotizacionRepository repo, PdfService pdf, AuditoriaService auditoria)
    {
        _repo = repo; _pdf = pdf; _auditoria = auditoria;
    }

    [HttpGet]
    public IActionResult Listar() => Ok(_repo.Listar());

    [HttpGet("{id}")]
    public IActionResult Obtener(int id)
    {
        var c = _repo.ObtenerPorId(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public IActionResult Crear([FromBody] Cotizacion cotizacion)
    {
        var idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        cotizacion.IdUsuarioAsesor = idUsuario;
        var id = _repo.Crear(cotizacion);
        _auditoria.Registrar(idUsuario, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
            "Cotizaciones", $"Creó cotización {cotizacion.NumeroCotizacion}", null, null);
        return Ok(new { id });
    }

    [HttpPost("{id}/pdf")]
    public IActionResult GenerarPdf(int id, [FromServices] PdfService pdf)
    {
        var c = _repo.ObtenerPorId(id);
        if (c == null) return NotFound();
        var bytes = pdf.GenerarCotizacionPdf(c);
        return File(bytes, "application/pdf", $"Cotizacion_{c.NumeroCotizacion}.pdf");
    }

    [HttpPut("{id}/concretar")]
    public IActionResult Concretar(int id, [FromBody] ConcretarCotizacionRequest req)
    {
        var idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        try
        {
            var idPedido = _repo.ConcretarCotizacion(id, req.OrdenCompraCliente, idUsuario);
            _auditoria.Registrar(idUsuario, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                "Cotizaciones", $"Concretó cotización {id} -> pedido {idPedido}", null, null);
            return Ok(new { idPedido });
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}

public class ConcretarCotizacionRequest
{
    public string OrdenCompraCliente { get; set; } = string.Empty;
}