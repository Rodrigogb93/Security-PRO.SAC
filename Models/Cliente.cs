namespace SecurityPRO.ERP.Web.Models;

public class Cliente
{
    public int IdCliente { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? DireccionFiscal { get; set; }
    public string? Telefono { get; set; }
    public string? EmailContacto { get; set; }
    public string? SectorEconomico { get; set; }
    public decimal LineaCredito { get; set; }
    public string CondicionesPago { get; set; } = "Contado";
    public bool ConvenioSuministro { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaRegistro { get; set; }
}