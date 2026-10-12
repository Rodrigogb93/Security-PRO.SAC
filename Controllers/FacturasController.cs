using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.Repositories;
using SecurityPRO.ERP.Web.Services;
using System.Security.Claims;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Finanzas")]
public class FacturasController : ControllerBase
{
    private readonly FacturaRepository _repo;
    private readonly SunatService _sunat;
    private readonly PdfService _pdf;
    private readonly AuditoriaService _auditoria;

    public FacturasController(
        FacturaRepository repo,
        SunatService sunat,
        PdfService pdf,
        AuditoriaService auditoria)
    {
        _repo = repo;
        _sunat = sunat;
        _pdf = pdf;
        _auditoria = auditoria;
    }

    // =========================================================
    // LISTAR FACTURAS
    // =========================================================

    [HttpGet]
    public IActionResult Listar()
    {
        return Ok(_repo.Listar());
    }

    // =========================================================
    // EMITIR FACTURA
    // =========================================================

    [HttpPost("emitir")]
    public IActionResult Emitir(
        [FromBody] EmitirFacturaRequest req)
    {
        var idUsuario = int.Parse(
            User.FindFirst(
                ClaimTypes.NameIdentifier
            )!.Value
        );

        try
        {
            // Evitar facturar dos veces el mismo pedido
            if (
                req.IdPedido.HasValue &&
                _repo.PedidoYaFacturado(
                    req.IdPedido.Value
                )
            )
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Este pedido ya fue facturado"
                    }
                );
            }

            // Obtener los datos del pedido o servicio
            var factura =
                _repo.ObtenerDatosParaFacturar(
                    req.IdPedido,
                    req.IdServicio
                )
                ?? throw new Exception(
                    "No se encontraron datos para facturar"
                );

            // Generar el número ANTES de crear el XML
            factura.Numero =
                _repo.ObtenerSiguienteNumero();

            // Generación simulada de XML/CDR SUNAT
            var (xml, cdr) =
                _sunat.EmitirFactura(
                    "F001",
                    factura.Numero,
                    factura.RucCliente,
                    factura.RazonSocial,
                    factura.Subtotal,
                    factura.Igv,
                    factura.Total,
                    req.CondicionPago
                );

            // Guardar la factura
            var idFactura =
                _repo.CrearFactura(
                    factura,
                    xml,
                    cdr,
                    req.CondicionPago
                );

            // Auditoría
            _auditoria.Registrar(
                idUsuario,
                HttpContext.Connection
                    .RemoteIpAddress?
                    .ToString() ?? "",
                "Facturas",
                $"Emitió factura F001-{factura.Numero}",
                null,
                null
            );

            return Ok(
                new
                {
                    id = idFactura,
                    numero =
                        $"F001-{factura.Numero}",
                    estado = "Aceptado"
                }
            );
        }
        catch (Exception ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                }
            );
        }
    }

    // =========================================================
    // PDF
    // =========================================================

    [HttpGet("{id}/pdf")]
    public IActionResult Pdf(int id)
    {
        var factura =
            _repo.ObtenerPorId(id);

        if (factura == null)
        {
            return NotFound();
        }

        var bytes =
            _pdf.GenerarFacturaPdf(factura);

        return File(
            bytes,
            "application/pdf",
            $"Factura_{factura.Serie}-{factura.Numero}.pdf"
        );
    }
}

// =============================================================
// REQUEST PARA EMITIR FACTURA
// =============================================================

public class EmitirFacturaRequest
{
    public int? IdPedido { get; set; }

    public int? IdServicio { get; set; }

    public string CondicionPago { get; set; }
        = "Contado";
}