using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.Models;
using SecurityPRO.ERP.Web.Repositories;
using SecurityPRO.ERP.Web.Services;
using System.Security.Claims;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Servicios Técnicos")]
public class ServiciosController : ControllerBase
{
    private readonly ServicioTecnicoRepository _repo;
    private readonly PdfService _pdf;
    private readonly AuditoriaService _auditoria;

    public ServiciosController(ServicioTecnicoRepository repo, PdfService pdf, AuditoriaService auditoria)
    {
        _repo = repo; _pdf = pdf; _auditoria = auditoria;
    }

    [HttpGet]
    public IActionResult Listar([FromQuery] string? tipo) => Ok(_repo.Listar(tipo));

    [HttpPost]
    public IActionResult Crear([FromBody] ServicioTecnico s)
    {
        var idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var id = _repo.CrearServicio(s, idUsuario);
        _auditoria.Registrar(idUsuario, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
            "ServiciosTecnicos", $"Creó servicio {s.TipoServicio}", null, null);
        return Ok(new { id });
    }

    [HttpPost("{id}/certificado")]
public IActionResult GenerarCertificado(int id)
{
    var servicio = _repo.Listar(null)
        .FirstOrDefault(x => x.IdServicio == id);

    if (servicio == null)
    {
        return NotFound(
            new { message = "Servicio no encontrado" }
        );
    }

    var codigo =
        $"CERT-{id}-{DateTime.Now:yyyyMMddHHmmss}";

    var detalle = $"""
        Servicio: {servicio.TipoServicio}
        Fecha del servicio: {servicio.FechaServicio:dd/MM/yyyy}
        Técnico: {servicio.NombreTecnico ?? "No especificado"}
        """;

    // ==============================================
    // DATOS DE EXTINTORES
    // ==============================================

    if (servicio.Extintores.Any())
    {
        detalle += "\n\nDATOS TÉCNICOS DEL EXTINTOR\n";

        foreach (var e in servicio.Extintores)
        {
            detalle += $"""
                
                N° Serie: {e.NumeroSerieCilindro}
                Tipo de agente: {e.TipoAgente}
                Capacidad: {e.CapacidadKg:N2} kg
                Presión manómetro: {e.PresionManometro:N2}
                Peso agente recargado: {e.PesoAgenteRecargado:N2} kg
                Próximo mantenimiento: {(e.FechaProximoMantenimiento.HasValue
                    ? e.FechaProximoMantenimiento.Value.ToString("dd/MM/yyyy")
                    : "No especificado")}
                """;
        }
    }

    // ==============================================
    // DATOS DE ARNESES
    // ==============================================

    if (servicio.Arneses.Any())
    {
        detalle += "\n\nDATOS TÉCNICOS DEL ARNÉS\n";

        foreach (var a in servicio.Arneses)
        {
            detalle += $"""
                
                N° Serie: {a.NumeroSerieArnes}
                Marca: {a.Marca}
                Modelo: {a.Modelo}
                Reatas: {(a.ChecklistReatas ? "Conforme" : "No conforme")}
                Costuras: {(a.ChecklistCosturas ? "Conforme" : "No conforme")}
                Anillos D: {(a.ChecklistAnillosD ? "Conforme" : "No conforme")}
                Hebillas: {(a.ChecklistHebillas ? "Conforme" : "No conforme")}
                Absorbedores: {(a.ChecklistAbsorbedores ? "Conforme" : "No conforme")}
                Resultado: {a.ResultadoChecklist}
                Observaciones: {a.Observaciones ?? "-"}
                """;
        }
    }

    if (!string.IsNullOrWhiteSpace(servicio.Observaciones))
    {
        detalle +=
            $"\n\nObservaciones generales: {servicio.Observaciones}";
    }

    var bytes =
        _pdf.GenerarCertificadoServicioPdf(
            "CERTIFICADO TÉCNICO OFICIAL",
            servicio.RazonSocialCliente ?? "",
            detalle,
            codigo
        );

    _repo.CertificarServicio(
        id,
        $"/certificados/{codigo}.pdf"
    );

    return File(
        bytes,
        "application/pdf",
        $"{codigo}.pdf"
    );
}
}