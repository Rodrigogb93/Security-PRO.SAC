namespace SecurityPRO.ERP.Web.Models;

public class Kardex
{
    public long IdKardex { get; set; }
    public int IdProducto { get; set; }
    public int? IdLote { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int SaldoAnterior { get; set; }
    public int SaldoActual { get; set; }
    public decimal? CostoUnitario { get; set; }
    public string? DocumentoReferencia { get; set; }
    public int? IdUsuario { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public string? Observaciones { get; set; }
    public string? NombreProducto { get; set; }
    public string? Sku { get; set; }
    public string? NumeroLote { get; set; }
}

public class Lote
{
    public int IdLote { get; set; }
    public int IdProducto { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public DateTime? FechaFabricacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int CantidadInicial { get; set; }
    public int CantidadActual { get; set; }
    public string? Semáforo { get; set; }
}