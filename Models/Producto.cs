namespace SecurityPRO.ERP.Web.Models;

public class Producto
{
    public int IdProducto { get; set; }
    public int IdCategoria { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Normativa { get; set; }
    public string? Talla { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public int StockMinimo { get; set; }
    public int StockMaximo { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? NombreCategoria { get; set; }
}

public class CategoriaEpp
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}