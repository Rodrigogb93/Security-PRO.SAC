namespace SecurityPRO.ERP.Web.Models;

public class Factura
{
    public int IdFactura { get; set; }
    public string Serie { get; set; } = "F001";
    public string Numero { get; set; } = string.Empty;
    public int? IdPedido { get; set; }
    public int? IdServicio { get; set; }
    public int IdCliente { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public string? XmlUrl { get; set; }
    public string? PdfUrl { get; set; }
    public string? CdrSunat { get; set; }
    public string EstadoSunat { get; set; } = "Pendiente";
    public string CondicionPago { get; set; } = "Contado";
    public string? RazonSocialCliente { get; set; }
}