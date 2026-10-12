namespace SecurityPRO.ERP.Web.Models;

public class ServicioTecnico
{
    public int IdServicio { get; set; }
    public string TipoServicio { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public int? IdUsuarioTecnico { get; set; }
    public DateTime FechaServicio { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? CertificadoUrl { get; set; }
    public string? Observaciones { get; set; }
    public string? RazonSocialCliente { get; set; }
    public string? NombreTecnico { get; set; }
    public List<DetalleExtintor> Extintores { get; set; } = new();
    public List<DetalleArnes> Arneses { get; set; } = new();
}

public class DetalleExtintor
{
    public int IdDetalleExt { get; set; }
    public int IdServicio { get; set; }
    public string NumeroSerieCilindro { get; set; } = string.Empty;
    public string TipoAgente { get; set; } = string.Empty;
    public decimal CapacidadKg { get; set; }
    public decimal PresionManometro { get; set; }
    public decimal PesoAgenteRecargado { get; set; }
    public DateTime? FechaProximoMantenimiento { get; set; }
}

public class DetalleArnes
{
    public int IdDetalleArnes { get; set; }
    public int IdServicio { get; set; }
    public string NumeroSerieArnes { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public bool ChecklistReatas { get; set; }
    public bool ChecklistCosturas { get; set; }
    public bool ChecklistAnillosD { get; set; }
    public bool ChecklistHebillas { get; set; }
    public bool ChecklistAbsorbedores { get; set; }
    public string ResultadoChecklist { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}