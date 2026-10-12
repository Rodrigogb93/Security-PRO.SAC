using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.Repositories;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/cuentas-pagar")]
[Authorize(Roles = "Administrador,Finanzas")]
public class CuentasPagarController : ControllerBase
{
    private readonly CuentaPagarRepository _repo;

    public CuentasPagarController(CuentaPagarRepository repo)
    {
        _repo = repo;
    }

    // LISTAR CUENTAS POR PAGAR
    [HttpGet]
    public IActionResult Listar()
    {
        return Ok(_repo.Listar());
    }

    // CREAR NUEVA CUENTA POR PAGAR
    [HttpPost]
    public IActionResult Crear(
        [FromBody] CrearCuentaPagarRequest request)
    {
        try
        {
            var id = _repo.Crear(
                request.IdProveedor,
                request.NumeroFacturaProveedor,
                request.MontoTotal,
                request.FechaVencimiento
            );

            return Ok(new
            {
                id,
                message = "Cuenta por pagar registrada correctamente"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // REGISTRAR UN PAGO
    [HttpPost("{id}/pago")]
    public IActionResult RegistrarPago(
        int id,
        [FromBody] PagoCuentaPagarRequest request)
    {
        try
        {
            _repo.RegistrarPago(
                id,
                request.Monto
            );

            return Ok(new
            {
                message = "Pago registrado correctamente"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}


// DTO PARA CREAR UNA CUENTA
public class CrearCuentaPagarRequest
{
    public int IdProveedor { get; set; }

    public string NumeroFacturaProveedor { get; set; }
        = string.Empty;

    public decimal MontoTotal { get; set; }

    public DateTime? FechaVencimiento { get; set; }
}


// DTO PARA REGISTRAR UN PAGO
public class PagoCuentaPagarRequest
{
    public decimal Monto { get; set; }
}