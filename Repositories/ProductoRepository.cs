using Dapper;
using SecurityPRO.ERP.Web.Models;

namespace SecurityPRO.ERP.Web.Repositories;

public class ProductoRepository
{
    private readonly DbConnectionFactory _factory;

    public ProductoRepository(DbConnectionFactory factory)
        => _factory = factory;

    private const string SelectProducto = @"
        SELECT
            p.id_producto AS IdProducto,
            p.id_categoria AS IdCategoria,
            p.sku AS Sku,
            p.nombre AS Nombre,
            p.marca AS Marca,
            p.modelo AS Modelo,
            p.normativa AS Normativa,
            p.talla AS Talla,
            p.precio_compra AS PrecioCompra,
            p.precio_venta AS PrecioVenta,
            p.stock_minimo AS StockMinimo,
            p.stock_maximo AS StockMaximo,
            p.activo AS Activo,
            p.fecha_creacion AS FechaCreacion,
            c.nombre AS NombreCategoria
        FROM productos p
        INNER JOIN categorias_epp c
            ON p.id_categoria = c.id_categoria";

    public IEnumerable<Producto> Listar(
        string? categoria,
        string? marca,
        string? normativa)
    {
        using var conn = _factory.CreateConnection();

        var sql = SelectProducto + " WHERE p.activo = 1";

        var param = new DynamicParameters();

        if (!string.IsNullOrEmpty(categoria))
        {
            sql += " AND c.nombre = @categoria";
            param.Add("categoria", categoria);
        }

        if (!string.IsNullOrEmpty(marca))
        {
            sql += " AND p.marca = @marca";
            param.Add("marca", marca);
        }

        if (!string.IsNullOrEmpty(normativa))
        {
            sql += " AND p.normativa LIKE @normativa";
            param.Add("normativa", $"%{normativa}%");
        }

        sql += " ORDER BY p.nombre";

        return conn.Query<Producto>(sql, param);
    }

    public Producto? ObtenerPorId(int id)
    {
        using var conn = _factory.CreateConnection();

        return conn.QueryFirstOrDefault<Producto>(
            SelectProducto + " WHERE p.id_producto = @id",
            new { id });
    }

    public int Crear(Producto p)
    {
        using var conn = _factory.CreateConnection();

        return conn.ExecuteScalar<int>(
            @"INSERT INTO productos
            (
                id_categoria,
                sku,
                nombre,
                marca,
                modelo,
                normativa,
                talla,
                precio_compra,
                precio_venta,
                stock_minimo,
                stock_maximo
            )
            VALUES
            (
                @IdCategoria,
                @Sku,
                @Nombre,
                @Marca,
                @Modelo,
                @Normativa,
                @Talla,
                @PrecioCompra,
                @PrecioVenta,
                @StockMinimo,
                @StockMaximo
            );

            SELECT LAST_INSERT_ID();",
            p);
    }

    public void Actualizar(Producto p)
    {
        using var conn = _factory.CreateConnection();

        conn.Execute(
            @"UPDATE productos
            SET
                id_categoria = @IdCategoria,
                sku = @Sku,
                nombre = @Nombre,
                marca = @Marca,
                modelo = @Modelo,
                normativa = @Normativa,
                talla = @Talla,
                precio_compra = @PrecioCompra,
                precio_venta = @PrecioVenta,
                stock_minimo = @StockMinimo,
                stock_maximo = @StockMaximo,
                activo = @Activo
            WHERE id_producto = @IdProducto",
            p);
    }

    public IEnumerable<CategoriaEpp> ListarCategorias()
    {
        using var conn = _factory.CreateConnection();

        return conn.Query<CategoriaEpp>(
            @"SELECT
                id_categoria AS IdCategoria,
                nombre AS Nombre,
                descripcion AS Descripcion
            FROM categorias_epp
            ORDER BY nombre");
    }
}