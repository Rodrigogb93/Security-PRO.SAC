using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.Repositories;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/cuentas-cobrar")]
[Authorize(Roles = "Administrador,Finanzas")]
public class CuentasCobrarController : ControllerBase
{
    private readonly CuentaCobrarRepository _repo;
    public CuentasCobrarController(CuentaCobrarRepository repo) => _repo = repo;

    [HttpGet]
    public IActionResult Listar() => Ok(_repo.Listar());

    [HttpPost("{id}/pago")]
    public IActionResult RegistrarPago(int id, [FromBody] PagoRequest req)
    {
        try
        {
            _repo.RegistrarPago(id, req.MontoPagado, req.MetodoPago, req.NumeroOperacion);
            return Ok();
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}

public class PagoRequest
{
    public decimal MontoPagado { get; set; }
    public string MetodoPago { get; set; } = "Transferencia";
    public string NumeroOperacion { get; set; } = string.Empty;
}