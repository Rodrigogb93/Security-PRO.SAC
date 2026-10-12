using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.Repositories;
using SecurityPRO.ERP.Web.Services;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly PedidoRepository _repo;
    private readonly PdfService _pdf;

    public PedidosController(
        PedidoRepository repo,
        PdfService pdf)
    {
        _repo = repo;
        _pdf = pdf;
    }

    [HttpGet]
    public IActionResult Listar()
        => Ok(_repo.Listar());

    [HttpGet("{id}")]
    public IActionResult Obtener(int id)
    {
        var p = _repo.ObtenerPorId(id);

        return p == null
            ? NotFound()
            : Ok(p);
    }

    // =========================================================
    // DESCARGAR PEDIDO EN PDF
    // =========================================================
    [HttpGet("{id}/pdf")]
    public IActionResult Pdf(int id)
    {
        var pedido = _repo.ObtenerPorId(id);

        if (pedido == null)
            return NotFound(
                new { message = "Pedido no encontrado" }
            );

        var bytes = _pdf.GenerarPedidoPdf(pedido);

        return File(
            bytes,
            "application/pdf",
            $"Pedido_{pedido.NumeroPedido}.pdf"
        );
    }

    [HttpPut("{id}/estado")]
    [Authorize(Roles = "Administrador,Almacén")]
    public IActionResult CambiarEstado(
        int id,
        [FromBody] string estado)
    {
        _repo.CambiarEstado(id, estado);

        return Ok();
    }
}