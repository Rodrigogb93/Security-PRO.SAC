using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.Repositories;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Finanzas")]
public class ComisionesController : ControllerBase
{
    private readonly ComisionRepository _repo;
    public ComisionesController(ComisionRepository repo) => _repo = repo;

    [HttpGet]
    public IActionResult Listar() => Ok(_repo.Listar());

    [HttpPost("liquidar")]
    public IActionResult Liquidar([FromQuery] int mes, [FromQuery] int anio)
        => Ok(new { liquidaciones = _repo.Liquidar(mes, anio) });
}