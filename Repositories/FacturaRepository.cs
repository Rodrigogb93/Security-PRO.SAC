using Dapper;
using SecurityPRO.ERP.Web.Models;

namespace SecurityPRO.ERP.Web.Repositories;

public class FacturaRepository
{
    private readonly DbConnectionFactory _factory;

    public FacturaRepository(DbConnectionFactory factory)
        => _factory = factory;


    public IEnumerable<Factura> Listar()
    {
        using var conn = _factory.CreateConnection();

        return conn.Query<Factura>(
            @"
            SELECT
                f.id_factura AS IdFactura,
                f.serie AS Serie,
                f.numero AS Numero,
                f.id_pedido AS IdPedido,
                f.id_servicio AS IdServicio,
                f.id_cliente AS IdCliente,
                f.fecha_emision AS FechaEmision,
                f.fecha_vencimiento AS FechaVencimiento,
                f.subtotal AS Subtotal,
                f.igv AS Igv,
                f.total AS Total,
                f.xml_url AS XmlUrl,
                f.cdr_sunat AS CdrSunat,
                f.estado_sunat AS EstadoSunat,
                f.condicion_pago AS CondicionPago,
                c.razon_social AS RazonSocialCliente

            FROM facturas f

            INNER JOIN clientes c
                ON f.id_cliente = c.id_cliente

            ORDER BY f.fecha_emision DESC,
                     f.id_factura DESC"
        );
    }


    public Factura? ObtenerPorId(int id)
    {
        using var conn = _factory.CreateConnection();

        return conn.QueryFirstOrDefault<Factura>(
            @"
            SELECT
                f.id_factura AS IdFactura,
                f.serie AS Serie,
                f.numero AS Numero,
                f.id_pedido AS IdPedido,
                f.id_servicio AS IdServicio,
                f.id_cliente AS IdCliente,
                f.fecha_emision AS FechaEmision,
                f.fecha_vencimiento AS FechaVencimiento,
                f.subtotal AS Subtotal,
                f.igv AS Igv,
                f.total AS Total,
                f.xml_url AS XmlUrl,
                f.cdr_sunat AS CdrSunat,
                f.estado_sunat AS EstadoSunat,
                f.condicion_pago AS CondicionPago,
                c.razon_social AS RazonSocialCliente

            FROM facturas f

            INNER JOIN clientes c
                ON f.id_cliente = c.id_cliente

            WHERE f.id_factura = @id",
            new { id }
        );
    }


    // =========================================================
    // GENERAR SIGUIENTE NÚMERO
    // =========================================================

    public string ObtenerSiguienteNumero()
    {
        using var conn = _factory.CreateConnection();

        var ultimo = conn.ExecuteScalar<int?>(
            @"
            SELECT MAX(CAST(numero AS UNSIGNED))

            FROM facturas

            WHERE serie = 'F001'
              AND YEAR(fecha_emision) = YEAR(NOW())"
        ) ?? 0;

        return (ultimo + 1).ToString("D8");
    }


    // =========================================================
    // COMPROBAR SI EL PEDIDO YA FUE FACTURADO
    // =========================================================

    public bool PedidoYaFacturado(int idPedido)
    {
        using var conn = _factory.CreateConnection();

        return conn.ExecuteScalar<int>(
            @"
            SELECT COUNT(*)

            FROM facturas

            WHERE id_pedido = @idPedido",
            new { idPedido }
        ) > 0;
    }


    // =========================================================
    // DATOS PARA FACTURAR
    // =========================================================

    public FacturaDatosSunat? ObtenerDatosParaFacturar(
        int? idPedido,
        int? idServicio)
    {
        using var conn = _factory.CreateConnection();

        if (idPedido.HasValue)
        {
            return conn.QueryFirstOrDefault<FacturaDatosSunat>(
                @"
                SELECT
                    p.id_cliente AS IdCliente,
                    c.ruc AS RucCliente,
                    c.razon_social AS RazonSocial,

                    p.subtotal AS Subtotal,
                    p.igv AS Igv,
                    p.total AS Total,

                    p.id_pedido AS IdPedido

                FROM pedidos p

                INNER JOIN clientes c
                    ON p.id_cliente = c.id_cliente

                WHERE p.id_pedido = @idPedido",
                new { idPedido }
            );
        }


        if (idServicio.HasValue)
        {
            return conn.QueryFirstOrDefault<FacturaDatosSunat>(
                @"
                SELECT
                    s.id_cliente AS IdCliente,
                    c.ruc AS RucCliente,
                    c.razon_social AS RazonSocial,

                    0 AS Subtotal,
                    0 AS Igv,
                    0 AS Total,

                    NULL AS IdPedido

                FROM servicios_tecnicos s

                INNER JOIN clientes c
                    ON s.id_cliente = c.id_cliente

                WHERE s.id_servicio = @idServicio",
                new { idServicio }
            );
        }


        return null;
    }


    // =========================================================
    // CREAR FACTURA
    // =========================================================

    public int CrearFactura(
        FacturaDatosSunat datos,
        string xml,
        string cdr,
        string condicionPago)
    {
        using var conn =
            (MySql.Data.MySqlClient.MySqlConnection)
            _factory.CreateConnection();

        conn.Open();

        using var transaction =
            conn.BeginTransaction();

        try
        {
            // Segunda protección contra duplicados
            if (datos.IdPedido.HasValue)
            {
                var existe = conn.ExecuteScalar<int>(
                    @"
                    SELECT COUNT(*)
                    FROM facturas
                    WHERE id_pedido = @IdPedido",
                    new
                    {
                        datos.IdPedido
                    },
                    transaction
                );

                if (existe > 0)
                {
                    throw new Exception(
                        "Este pedido ya fue facturado"
                    );
                }
            }


            var id = conn.ExecuteScalar<int>(
                @"
                INSERT INTO facturas
                (
                    serie,
                    numero,
                    id_pedido,
                    id_cliente,
                    fecha_vencimiento,
                    subtotal,
                    igv,
                    total,
                    xml_url,
                    cdr_sunat,
                    estado_sunat,
                    condicion_pago
                )
                VALUES
                (
                    'F001',
                    @Numero,
                    @IdPedido,
                    @IdCliente,
                    DATE_ADD(NOW(), INTERVAL 30 DAY),
                    @Subtotal,
                    @Igv,
                    @Total,
                    @Xml,
                    @Cdr,
                    'Aceptado',
                    @CondicionPago
                );

                SELECT LAST_INSERT_ID();",
                new
                {
                    Numero = datos.Numero,
                    datos.IdPedido,
                    datos.IdCliente,
                    datos.Subtotal,
                    datos.Igv,
                    datos.Total,
                    Xml = xml,
                    Cdr = cdr,
                    CondicionPago = condicionPago
                },
                transaction
            );


            // Crear automáticamente la cuenta por cobrar
            conn.Execute(
                @"
                INSERT INTO cuentas_por_cobrar
                (
                    id_factura,
                    id_cliente,
                    monto_total,
                    saldo_pendiente,
                    fecha_vencimiento,
                    estado
                )
                VALUES
                (
                    @IdFactura,
                    @IdCliente,
                    @Total,
                    @Total,
                    DATE_ADD(NOW(), INTERVAL 30 DAY),
                    'Pendiente'
                )",
                new
                {
                    IdFactura = id,
                    datos.IdCliente,
                    datos.Total
                },
                transaction
            );


            transaction.Commit();

            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}


public class FacturaDatosSunat
{
    public int IdCliente { get; set; }

    public string RucCliente { get; set; }
        = string.Empty;

    public string RazonSocial { get; set; }
        = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal Igv { get; set; }

    public decimal Total { get; set; }

    public int? IdPedido { get; set; }

    public string Numero { get; set; }
        = string.Empty;
}