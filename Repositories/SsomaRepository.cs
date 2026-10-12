using Dapper;

namespace SecurityPRO.ERP.Web.Repositories;

public class SsomaRepository
{
    private readonly DbConnectionFactory _factory;

    public SsomaRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public dynamic Resumen()
    {
        using var conn = _factory.CreateConnection();

        return new
        {
            fichas = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM fichas_tecnicas"),

            certificados = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM certificados_calidad"),

            capacitaciones = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM capacitaciones")
        };
    }


    public IEnumerable<dynamic> ListarFichas()
    {
        using var conn = _factory.CreateConnection();

        return conn.Query(
            @"SELECT
                f.*,
                p.nombre AS NombreProducto,
                p.sku AS Sku
              FROM fichas_tecnicas f
              INNER JOIN productos p
                ON f.id_producto = p.id_producto");
    }


    public int CrearFicha(
        int idProducto,
        string nombreArchivo,
        string urlArchivo,
        string version)
    {
        using var conn = _factory.CreateConnection();

        return conn.ExecuteScalar<int>(
            @"INSERT INTO fichas_tecnicas
            (
                id_producto,
                nombre_archivo,
                url_archivo,
                version
            )
            VALUES
            (
                @idProducto,
                @nombreArchivo,
                @urlArchivo,
                @version
            );

            SELECT LAST_INSERT_ID();",
            new
            {
                idProducto,
                nombreArchivo,
                urlArchivo,
                version
            });
    }


    public IEnumerable<dynamic> ListarCapacitaciones()
    {
        using var conn = _factory.CreateConnection();

        return conn.Query(
            @"SELECT
                c.*,
                cl.razon_social AS RazonSocialCliente,
                u.nombres AS NombreInstructor
              FROM capacitaciones c
              INNER JOIN clientes cl
                ON c.id_cliente = cl.id_cliente
              LEFT JOIN usuarios u
                ON c.id_usuario_instructor = u.id_usuario
              ORDER BY c.fecha_capacitacion DESC");
    }


    public int CrearCapacitacion(
        int idCliente,
        int? idUsuarioInstructor,
        string tema,
        DateTime fechaCapacitacion,
        string lugar,
        TimeSpan? horaInicio,
        TimeSpan? horaFin)
    {
        using var conn = _factory.CreateConnection();

        return conn.ExecuteScalar<int>(
            @"INSERT INTO capacitaciones
            (
                id_cliente,
                id_usuario_instructor,
                tema,
                fecha_capacitacion,
                lugar,
                hora_inicio,
                hora_fin
            )
            VALUES
            (
                @idCliente,
                @idUsuarioInstructor,
                @tema,
                @fechaCapacitacion,
                @lugar,
                @horaInicio,
                @horaFin
            );

            SELECT LAST_INSERT_ID();",
            new
            {
                idCliente,
                idUsuarioInstructor,
                tema,
                fechaCapacitacion,
                lugar,
                horaInicio,
                horaFin
            });
    }
}