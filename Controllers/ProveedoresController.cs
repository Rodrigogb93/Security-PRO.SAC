using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityPRO.ERP.Web.Repositories;

namespace SecurityPRO.ERP.Web.Controllers;

[ApiController]
[Route("api/proveedores")]
[Authorize(Roles = "Administrador,Finanzas")]
public class ProveedoresController : ControllerBase
{
    private readonly DbConnectionFactory _factory;

    public ProveedoresController(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public IActionResult Listar()
    {
        using var conn = _factory.CreateConnection();

        var proveedores = conn.Query(
            @"SELECT
                id_proveedor AS IdProveedor,
                ruc AS Ruc,
                razon_social AS RazonSocial,
                pais_origen AS PaisOrigen,
                contacto AS Contacto,
                telefono AS Telefono,
                email AS Email
              FROM proveedores
              WHERE activo = 1
              ORDER BY razon_social"
        );

        return Ok(proveedores);
    }
}