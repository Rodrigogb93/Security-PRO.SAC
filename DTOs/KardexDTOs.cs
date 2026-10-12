namespace SecurityPRO.ERP.Web.DTOs;

public class MovimientoKardexRequest
{
    public int IdProducto { get; set; }
    public int? IdLote { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public string? DocumentoReferencia { get; set; }
    public string? Observaciones { get; set; }
    public string? NumeroLote { get; set; }
    public DateTime? FechaVencimiento { get; set; }
}

public class SemáforoVencimiento
{
    public int IdProducto { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string NumeroLote { get; set; } = string.Empty;
    public DateTime? FechaVencimiento { get; set; }
    public int DiasRestantes { get; set; }
    public string Color { get; set; } = string.Empty;
}