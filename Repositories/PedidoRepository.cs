using Dapper;
using SecurityPRO.ERP.Web.Models;


namespace SecurityPRO.ERP.Web.Repositories;

public class PedidoRepository
{
    private readonly DbConnectionFactory _factory;

    public PedidoRepository(DbConnectionFactory factory)
        => _factory = factory;

    // =========================================================
    // SELECT BASE - CABECERA DEL PEDIDO
    // =========================================================
    private const string SelectPedido = @"
        SELECT
            p.id_pedido AS IdPedido,
            p.numero_pedido AS NumeroPedido,
            p.id_cotizacion AS IdCotizacion,
            p.id_cliente AS IdCliente,
            p.orden_compra_cliente AS OrdenCompraCliente,
            p.fecha_pedido AS FechaPedido,
            p.fecha_entrega_programada AS FechaEntregaProgramada,
            p.direccion_entrega AS DireccionEntrega,
            p.subtotal AS Subtotal,
            p.igv AS Igv,
            p.total AS Total,
            p.estado AS Estado,
            c.razon_social AS RazonSocialCliente
        FROM pedidos p
        INNER JOIN clientes c
            ON p.id_cliente = c.id_cliente";

    // =========================================================
    // SELECT BASE - DETALLES DEL PEDIDO
    // =========================================================
    private const string SelectDetalle = @"
        SELECT
            d.id_detalle AS IdDetalle,
            d.id_pedido AS IdPedido,
            d.id_producto AS IdProducto,
            d.cantidad AS Cantidad,
            d.precio_unitario AS PrecioUnitario,
            d.subtotal_linea AS SubtotalLinea,
            d.cantidad_despachada AS CantidadDespachada,
            pr.nombre AS NombreProducto,
            pr.sku AS Sku
        FROM pedido_detalle d
        INNER JOIN productos pr
            ON d.id_producto = pr.id_producto";

    // =========================================================
    // LISTAR PEDIDOS
    // =========================================================
    public IEnumerable<Pedido> Listar()
    {
        using var conn = _factory.CreateConnection();

        return conn.Query<Pedido>(
            SelectPedido + @"
            ORDER BY p.fecha_pedido DESC,
                     p.id_pedido DESC"
        );
    }

    // =========================================================
    // OBTENER PEDIDO + DETALLES
    // =========================================================
    public Pedido? ObtenerPorId(int id)
    {
        using var conn = _factory.CreateConnection();

        var pedido = conn.QueryFirstOrDefault<Pedido>(
            SelectPedido + @"
            WHERE p.id_pedido = @id",
            new { id }
        );

        if (pedido == null)
            return null;

        pedido.Detalles = conn.Query<PedidoDetalle>(
            SelectDetalle + @"
            WHERE d.id_pedido = @id
            ORDER BY d.id_detalle",
            new { id }
        ).ToList();

        return pedido;
    }

    // =========================================================
    // CAMBIAR ESTADO
    // =========================================================
    public void CambiarEstado(int id, string estado)
    {
        using var conn = _factory.CreateConnection();

        conn.Execute(
            @"UPDATE pedidos
            SET estado = @estado
            WHERE id_pedido = @id",
            new { id, estado }
        );
    }
}
public class Pedido
{
    public int IdPedido { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public int? IdCotizacion { get; set; }
    public int IdCliente { get; set; }
    public string? OrdenCompraCliente { get; set; }
    public DateTime FechaPedido { get; set; }
    public DateTime? FechaEntregaProgramada { get; set; }
    public string? DireccionEntrega { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? RazonSocialCliente { get; set; }
    public List<PedidoDetalle> Detalles { get; set; } = new();
}

public class PedidoDetalle
{
    public int IdDetalle { get; set; }
    public int IdPedido { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubtotalLinea { get; set; }
    public int CantidadDespachada { get; set; }
    public string? NombreProducto { get; set; }
    public string? Sku { get; set; }
}