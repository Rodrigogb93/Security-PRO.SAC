using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.DTOs;
using SecurityPRO.ERP.Web.Repositories;
using SecurityPRO.ERP.Web.Services;
using System.Security.Claims;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Almacén")]
public class KardexController : ControllerBase
{
    private readonly KardexRepository _repo;
    private readonly AuditoriaService _auditoria;

    public KardexController(KardexRepository repo, AuditoriaService auditoria)
    {
        _repo = repo;
        _auditoria = auditoria;
    }

    [HttpGet]
    public IActionResult Listar([FromQuery] int? idProducto) => Ok(_repo.Listar(idProducto));
    
    [HttpGet("lotes/{idProducto}")]
    public IActionResult ListarLotes(int idProducto)
{
    return Ok(_repo.ListarLotes(idProducto));
}

    [HttpPost("movimiento")]
    public IActionResult RegistrarMovimiento([FromBody] MovimientoKardexRequest request)
    {
        var idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        try
        {
            var id = _repo.RegistrarMovimiento(request, idUsuario);
            _auditoria.Registrar(idUsuario, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                "Kardex", $"Movimiento {request.TipoMovimiento} producto {request.IdProducto}", null, null);
            return Ok(new { id });
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("semaforo-vencimiento")]
    public IActionResult SemaforoVencimiento() => Ok(_repo.ObtenerSemáforoVencimiento());

    [HttpGet("exportar-excel")]
    public IActionResult ExportarExcel([FromServices] ExcelService excel, [FromQuery] int? idProducto)
    {
        var datos = _repo.Listar(idProducto);
        var bytes = excel.GenerarKardexExcel(datos);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Kardex.xlsx");
    }
}