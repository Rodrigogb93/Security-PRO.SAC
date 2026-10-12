using Dapper;
using MySql.Data.MySqlClient;
using SecurityPRO.ERP.Web.Models;

namespace SecurityPRO.ERP.Web.Repositories;

public class CotizacionRepository
{
    private readonly DbConnectionFactory _factory;

    public CotizacionRepository(DbConnectionFactory factory)
        => _factory = factory;

    // =========================================================
    // SELECT BASE - CABECERA DE COTIZACIÓN
    // =========================================================
    private const string SelectCotizacion = @"
        SELECT
            c.id_cotizacion AS IdCotizacion,
            c.numero_cotizacion AS NumeroCotizacion,
            c.id_cliente AS IdCliente,
            c.id_usuario_asesor AS IdUsuarioAsesor,
            c.fecha_emision AS FechaEmision,
            c.validez_dias AS ValidezDias,
            c.subtotal AS Subtotal,
            c.igv AS Igv,
            c.total AS Total,
            c.estado AS Estado,
            c.pdf_url AS PdfUrl,
            c.observaciones AS Observaciones,
            cl.razon_social AS RazonSocialCliente,
            u.nombres AS NombreAsesor
        FROM cotizaciones c
        INNER JOIN clientes cl
            ON c.id_cliente = cl.id_cliente
        INNER JOIN usuarios u
            ON c.id_usuario_asesor = u.id_usuario";

    // =========================================================
    // SELECT BASE - DETALLE DE COTIZACIÓN
    // =========================================================
    private const string SelectDetalle = @"
        SELECT
            d.id_detalle AS IdDetalle,
            d.id_cotizacion AS IdCotizacion,
            d.id_producto AS IdProducto,
            d.cantidad AS Cantidad,
            d.precio_unitario AS PrecioUnitario,
            d.descuento_volumen AS DescuentoVolumen,
            d.subtotal_linea AS SubtotalLinea,
            p.nombre AS NombreProducto,
            p.sku AS Sku
        FROM cotizacion_detalle d
        INNER JOIN productos p
            ON d.id_producto = p.id_producto";

    // =========================================================
    // LISTAR COTIZACIONES
    // =========================================================
    public IEnumerable<Cotizacion> Listar()
    {
        using var conn = _factory.CreateConnection();

        return conn.Query<Cotizacion>(
            SelectCotizacion + @"
            ORDER BY c.fecha_emision DESC,
                     c.id_cotizacion DESC"
        );
    }

    // =========================================================
    // OBTENER UNA COTIZACIÓN + SUS DETALLES
    // =========================================================
    public Cotizacion? ObtenerPorId(int id)
    {
        using var conn = _factory.CreateConnection();

        var cotizacion = conn.QueryFirstOrDefault<Cotizacion>(
            SelectCotizacion + @"
            WHERE c.id_cotizacion = @id",
            new { id }
        );

        if (cotizacion == null)
            return null;

        cotizacion.Detalles = conn.Query<CotizacionDetalle>(
            SelectDetalle + @"
            WHERE d.id_cotizacion = @id
            ORDER BY d.id_detalle",
            new { id }
        ).ToList();

        return cotizacion;
    }

    // =========================================================
    // CREAR COTIZACIÓN
    // =========================================================
    public int Crear(Cotizacion c)
    {
        using var conn =
            (MySqlConnection)_factory.CreateConnection();

        conn.Open();

        using var tx = conn.BeginTransaction();

        try
        {
            // -------------------------------------------------
            // VALIDACIONES
            // -------------------------------------------------
            if (c.IdCliente <= 0)
                throw new Exception("Debe seleccionar un cliente válido.");

            if (c.IdUsuarioAsesor <= 0)
                throw new Exception("No se pudo identificar al asesor.");

            if (c.Detalles == null || c.Detalles.Count == 0)
                throw new Exception(
                    "La cotización debe contener al menos un producto."
                );

            foreach (var detalle in c.Detalles)
            {
                if (detalle.IdProducto <= 0)
                    throw new Exception(
                        "Uno de los productos de la cotización no es válido."
                    );

                if (detalle.Cantidad <= 0)
                    throw new Exception(
                        "La cantidad de cada producto debe ser mayor que cero."
                    );

                if (detalle.PrecioUnitario < 0)
                    throw new Exception(
                        "El precio unitario no puede ser negativo."
                    );

                if (detalle.DescuentoVolumen < 0)
                    throw new Exception(
                        "El descuento no puede ser negativo."
                    );
            }

            // -------------------------------------------------
            // GENERAR NÚMERO DE COTIZACIÓN
            // -------------------------------------------------
            var correlativo = conn.ExecuteScalar<int>(
                @"SELECT COUNT(*) + 1
                  FROM cotizaciones
                  WHERE YEAR(fecha_emision) = YEAR(NOW())",
                transaction: tx
            );

            c.NumeroCotizacion =
                $"COT-{DateTime.Now:yyyy}-{correlativo:D4}";

            // -------------------------------------------------
            // CALCULAR TOTALES
            // -------------------------------------------------
            foreach (var detalle in c.Detalles)
            {
                detalle.SubtotalLinea =
                    (detalle.Cantidad * detalle.PrecioUnitario)
                    - detalle.DescuentoVolumen;

                if (detalle.SubtotalLinea < 0)
                    throw new Exception(
                        "El descuento de un producto no puede superar " +
                        "el importe de la línea."
                    );
            }

            c.Subtotal = Math.Round(
                c.Detalles.Sum(d => d.SubtotalLinea),
                2
            );

            c.Igv = Math.Round(c.Subtotal * 0.18m, 2);

            c.Total = Math.Round(
                c.Subtotal + c.Igv,
                2
            );

            if (c.ValidezDias <= 0)
                c.ValidezDias = 15;

            // -------------------------------------------------
            // INSERTAR CABECERA
            // -------------------------------------------------
            var idCotizacion = conn.ExecuteScalar<int>(
                @"INSERT INTO cotizaciones
                (
                    numero_cotizacion,
                    id_cliente,
                    id_usuario_asesor,
                    validez_dias,
                    subtotal,
                    igv,
                    total,
                    estado,
                    observaciones
                )
                VALUES
                (
                    @NumeroCotizacion,
                    @IdCliente,
                    @IdUsuarioAsesor,
                    @ValidezDias,
                    @Subtotal,
                    @Igv,
                    @Total,
                    'Cotizado',
                    @Observaciones
                );

                SELECT LAST_INSERT_ID();",
                c,
                tx
            );

            // -------------------------------------------------
            // INSERTAR DETALLES
            // -------------------------------------------------
            foreach (var detalle in c.Detalles)
            {
                detalle.IdCotizacion = idCotizacion;

                conn.Execute(
                    @"INSERT INTO cotizacion_detalle
                    (
                        id_cotizacion,
                        id_producto,
                        cantidad,
                        precio_unitario,
                        descuento_volumen,
                        subtotal_linea
                    )
                    VALUES
                    (
                        @IdCotizacion,
                        @IdProducto,
                        @Cantidad,
                        @PrecioUnitario,
                        @DescuentoVolumen,
                        @SubtotalLinea
                    )",
                    detalle,
                    tx
                );
            }

            tx.Commit();

            return idCotizacion;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // =========================================================
    // CONCRETAR COTIZACIÓN Y GENERAR PEDIDO
    // =========================================================
    public int ConcretarCotizacion(
        int idCotizacion,
        string ordenCompra,
        int idUsuario)
    {
        using var conn =
            (MySqlConnection)_factory.CreateConnection();

        conn.Open();

        using var tx = conn.BeginTransaction();

        try
        {
            // -------------------------------------------------
            // OBTENER CABECERA CORRECTAMENTE MAPEADA
            // -------------------------------------------------
            var cotizacion = conn.QueryFirstOrDefault<Cotizacion>(
                @"SELECT
                    id_cotizacion AS IdCotizacion,
                    numero_cotizacion AS NumeroCotizacion,
                    id_cliente AS IdCliente,
                    id_usuario_asesor AS IdUsuarioAsesor,
                    fecha_emision AS FechaEmision,
                    validez_dias AS ValidezDias,
                    subtotal AS Subtotal,
                    igv AS Igv,
                    total AS Total,
                    estado AS Estado,
                    pdf_url AS PdfUrl,
                    observaciones AS Observaciones
                  FROM cotizaciones
                  WHERE id_cotizacion = @idCotizacion
                    AND estado IN ('Cotizado', 'En Negociación')",
                new { idCotizacion },
                tx
            );

            if (cotizacion == null)
            {
                throw new Exception(
                    "La cotización no existe o ya fue procesada."
                );
            }

            // -------------------------------------------------
            // OBTENER DETALLES CORRECTAMENTE MAPEADOS
            // -------------------------------------------------
            var detalles = conn.Query<CotizacionDetalle>(
                @"SELECT
                    id_detalle AS IdDetalle,
                    id_cotizacion AS IdCotizacion,
                    id_producto AS IdProducto,
                    cantidad AS Cantidad,
                    precio_unitario AS PrecioUnitario,
                    descuento_volumen AS DescuentoVolumen,
                    subtotal_linea AS SubtotalLinea
                  FROM cotizacion_detalle
                  WHERE id_cotizacion = @idCotizacion
                  ORDER BY id_detalle",
                new { idCotizacion },
                tx
            ).ToList();

            if (detalles.Count == 0)
            {
                throw new Exception(
                    "La cotización no contiene productos."
                );
            }

            // -------------------------------------------------
            // GENERAR NÚMERO DE PEDIDO
            // -------------------------------------------------
            var correlativo = conn.ExecuteScalar<int>(
                @"SELECT COUNT(*) + 1
                  FROM pedidos
                  WHERE YEAR(fecha_pedido) = YEAR(NOW())",
                transaction: tx
            );

            var numeroPedido =
                $"PED-{DateTime.Now:yyyy}-{correlativo:D4}";

            // -------------------------------------------------
            // CREAR PEDIDO
            // -------------------------------------------------
            var idPedido = conn.ExecuteScalar<int>(
                @"INSERT INTO pedidos
                (
                    numero_pedido,
                    id_cotizacion,
                    id_cliente,
                    orden_compra_cliente,
                    subtotal,
                    igv,
                    total,
                    estado
                )
                VALUES
                (
                    @NumeroPedido,
                    @IdCotizacion,
                    @IdCliente,
                    @OrdenCompra,
                    @Subtotal,
                    @Igv,
                    @Total,
                    'Pendiente'
                );

                SELECT LAST_INSERT_ID();",
                new
                {
                    NumeroPedido = numeroPedido,
                    IdCotizacion = idCotizacion,
                    IdCliente = cotizacion.IdCliente,
                    OrdenCompra = ordenCompra,
                    Subtotal = cotizacion.Subtotal,
                    Igv = cotizacion.Igv,
                    Total = cotizacion.Total
                },
                tx
            );

            // -------------------------------------------------
            // COPIAR DETALLES DE COTIZACIÓN → PEDIDO
            // -------------------------------------------------
            foreach (var detalle in detalles)
            {
                conn.Execute(
                    @"INSERT INTO pedido_detalle
                    (
                        id_pedido,
                        id_producto,
                        cantidad,
                        precio_unitario,
                        subtotal_linea
                    )
                    VALUES
                    (
                        @IdPedido,
                        @IdProducto,
                        @Cantidad,
                        @PrecioUnitario,
                        @SubtotalLinea
                    )",
                    new
                    {
                        IdPedido = idPedido,
                        detalle.IdProducto,
                        detalle.Cantidad,
                        detalle.PrecioUnitario,
                        detalle.SubtotalLinea
                    },
                    tx
                );
            }

            // -------------------------------------------------
            // MARCAR COTIZACIÓN COMO CONCRETADA
            // -------------------------------------------------
            conn.Execute(
                @"UPDATE cotizaciones
                SET estado = 'Concretada'
                WHERE id_cotizacion = @idCotizacion",
                new { idCotizacion },
                tx
            );

            tx.Commit();

            return idPedido;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}