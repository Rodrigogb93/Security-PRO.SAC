using Dapper;
using SecurityPRO.ERP.Web.Models;

namespace SecurityPRO.ERP.Web.Repositories;

public class ClienteRepository
{
    private readonly DbConnectionFactory _factory;

    public ClienteRepository(DbConnectionFactory factory)
        => _factory = factory;

    private const string SelectCliente = @"
        SELECT
            id_cliente AS IdCliente,
            ruc AS Ruc,
            razon_social AS RazonSocial,
            direccion_fiscal AS DireccionFiscal,
            telefono AS Telefono,
            email_contacto AS EmailContacto,
            sector_economico AS SectorEconomico,
            linea_credito AS LineaCredito,
            condiciones_pago AS CondicionesPago,
            convenio_suministro AS ConvenioSuministro,
            activo AS Activo,
            fecha_registro AS FechaRegistro
        FROM clientes";

    public IEnumerable<Cliente> Listar(string? search)
    {
        using var conn = _factory.CreateConnection();

        var sql = SelectCliente + " WHERE activo = 1";

        if (!string.IsNullOrEmpty(search))
        {
            sql += @"
                AND (
                    razon_social LIKE @s
                    OR ruc LIKE @s
                )";
        }

        sql += " ORDER BY razon_social";

        return conn.Query<Cliente>(
            sql,
            new { s = $"%{search}%" }
        );
    }

    public Cliente? ObtenerPorId(int id)
    {
        using var conn = _factory.CreateConnection();

        return conn.QueryFirstOrDefault<Cliente>(
            SelectCliente + " WHERE id_cliente = @id",
            new { id }
        );
    }

    public Cliente? ObtenerPorRuc(string ruc)
    {
        using var conn = _factory.CreateConnection();

        return conn.QueryFirstOrDefault<Cliente>(
            SelectCliente + @"
                WHERE ruc = @ruc
                AND activo = 1",
            new { ruc }
        );
    }

    public int Crear(Cliente c)
    {
        using var conn = _factory.CreateConnection();

        return conn.ExecuteScalar<int>(
            @"INSERT INTO clientes
            (
                ruc,
                razon_social,
                direccion_fiscal,
                telefono,
                email_contacto,
                sector_economico,
                linea_credito,
                condiciones_pago,
                convenio_suministro
            )
            VALUES
            (
                @Ruc,
                @RazonSocial,
                @DireccionFiscal,
                @Telefono,
                @EmailContacto,
                @SectorEconomico,
                @LineaCredito,
                @CondicionesPago,
                @ConvenioSuministro
            );

            SELECT LAST_INSERT_ID();",
            c
        );
    }

    public void Actualizar(Cliente c)
{
    using var conn = _factory.CreateConnection();

    conn.Execute(
        @"UPDATE clientes
        SET
            ruc = @Ruc,
            razon_social = @RazonSocial,
            direccion_fiscal = @DireccionFiscal,
            telefono = @Telefono,
            email_contacto = @EmailContacto,
            sector_economico = @SectorEconomico,
            linea_credito = @LineaCredito,
            condiciones_pago = @CondicionesPago,
            convenio_suministro = @ConvenioSuministro
        WHERE id_cliente = @IdCliente",
        c
    );
}
}