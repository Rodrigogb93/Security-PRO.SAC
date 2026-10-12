namespace SecurityPRO.ERP.Web.Models;

public class Cotizacion
{
    public int IdCotizacion { get; set; }
    public string NumeroCotizacion { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public int IdUsuarioAsesor { get; set; }
    public DateTime FechaEmision { get; set; }
    public int ValidezDias { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Cotizado";
    public string? PdfUrl { get; set; }
    public string? Observaciones { get; set; }
    public string? RazonSocialCliente { get; set; }
    public string? NombreAsesor { get; set; }
    public List<CotizacionDetalle> Detalles { get; set; } = new();
}

public class CotizacionDetalle
{
    public int IdDetalle { get; set; }
    public int IdCotizacion { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoVolumen { get; set; }
    public decimal SubtotalLinea { get; set; }
    public string? NombreProducto { get; set; }
    public string? Sku { get; set; }
}