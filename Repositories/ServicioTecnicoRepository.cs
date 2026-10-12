using Dapper;
using SecurityPRO.ERP.Web.Models;

namespace SecurityPRO.ERP.Web.Repositories;

public class ServicioTecnicoRepository
{
    private readonly DbConnectionFactory _factory;
    public ServicioTecnicoRepository(DbConnectionFactory factory) => _factory = factory;

    public IEnumerable<ServicioTecnico> Listar(string? tipo)
{
    using var conn = _factory.CreateConnection();

    var sql = @"
        SELECT
            s.id_servicio AS IdServicio,
            s.tipo_servicio AS TipoServicio,
            s.id_cliente AS IdCliente,
            s.id_usuario_tecnico AS IdUsuarioTecnico,
            s.fecha_servicio AS FechaServicio,
            s.estado AS Estado,
            s.observaciones AS Observaciones,
            s.certificado_url AS CertificadoUrl,

            c.razon_social AS RazonSocialCliente,
            u.nombres AS NombreTecnico

        FROM servicios_tecnicos s

        INNER JOIN clientes c
            ON s.id_cliente = c.id_cliente

        LEFT JOIN usuarios u
            ON s.id_usuario_tecnico = u.id_usuario";

    if (!string.IsNullOrEmpty(tipo))
    {
        sql += " WHERE s.tipo_servicio = @tipo";
    }

    sql += @"
        ORDER BY s.fecha_servicio DESC,
                 s.id_servicio DESC";

    var servicios = conn.Query<ServicioTecnico>(
        sql,
        new { tipo }
    ).ToList();


    // =====================================================
    // CARGAR DETALLES DE CADA SERVICIO
    // =====================================================

    foreach (var servicio in servicios)
    {
        servicio.Extintores = conn.Query<DetalleExtintor>(
            @"
            SELECT
                id_detalle_ext AS IdDetalleExt,
                id_servicio AS IdServicio,
                numero_serie_cilindro AS NumeroSerieCilindro,
                tipo_agente AS TipoAgente,
                capacidad_kg AS CapacidadKg,
                presion_manometro AS PresionManometro,
                peso_agente_recargado AS PesoAgenteRecargado,
                fecha_proximo_mantenimiento AS FechaProximoMantenimiento

            FROM detalle_extintores

            WHERE id_servicio = @idServicio",
            new
            {
                idServicio = servicio.IdServicio
            }
        ).ToList();


        servicio.Arneses = conn.Query<DetalleArnes>(
            @"
            SELECT
                id_detalle_arnes AS IdDetalleArnes,
                id_servicio AS IdServicio,
                numero_serie_arnes AS NumeroSerieArnes,
                marca AS Marca,
                modelo AS Modelo,
                checklist_reatas AS ChecklistReatas,
                checklist_costuras AS ChecklistCosturas,
                checklist_anillos_d AS ChecklistAnillosD,
                checklist_hebillas AS ChecklistHebillas,
                checklist_absorbedores AS ChecklistAbsorbedores,
                resultado_checklist AS ResultadoChecklist,
                observaciones AS Observaciones

            FROM detalle_arneses

            WHERE id_servicio = @idServicio",
            new
            {
                idServicio = servicio.IdServicio
            }
        ).ToList();
    }


    return servicios;
}

    public int CrearServicio(ServicioTecnico s, int idUsuario)
    {
        using var conn = (MySql.Data.MySqlClient.MySqlConnection)_factory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            var id = conn.ExecuteScalar<int>(
                @"INSERT INTO servicios_tecnicos (tipo_servicio, id_cliente, id_usuario_tecnico, fecha_servicio, estado, observaciones)
                  VALUES (@TipoServicio, @IdCliente, @idUsuario, NOW(), 'Pendiente', @Observaciones);
                  SELECT LAST_INSERT_ID();", new { s.TipoServicio, s.IdCliente, idUsuario, s.Observaciones }, tx);

            if (s.Extintores?.Any() == true)
            {
                foreach (var e in s.Extintores)
                {
                    e.IdServicio = id;
                    conn.Execute(
                        @"INSERT INTO detalle_extintores (id_servicio, numero_serie_cilindro, tipo_agente, capacidad_kg,
                          presion_manometro, peso_agente_recargado, fecha_proximo_mantenimiento)
                          VALUES (@IdServicio, @NumeroSerieCilindro, @TipoAgente, @CapacidadKg,
                          @PresionManometro, @PesoAgenteRecargado, DATE_ADD(NOW(), INTERVAL 1 YEAR))", e, tx);
                }
            }

            if (s.Arneses?.Any() == true)
            {
                foreach (var a in s.Arneses)
                {
                    a.IdServicio = id;
                    conn.Execute(
                        @"INSERT INTO detalle_arneses (id_servicio, numero_serie_arnes, marca, modelo,
                          checklist_reatas, checklist_costuras, checklist_anillos_d, checklist_hebillas,
                          checklist_absorbedores, resultado_checklist, observaciones)
                          VALUES (@IdServicio, @NumeroSerieArnes, @Marca, @Modelo,
                          @ChecklistReatas, @ChecklistCosturas, @ChecklistAnillosD, @ChecklistHebillas,
                          @ChecklistAbsorbedores, @ResultadoChecklist, @Observaciones)", a, tx);
                }
            }

            tx.Commit();
            return id;
        }
        catch { tx.Rollback(); throw; }
    }

    public void CertificarServicio(int idServicio, string certificadoUrl)
    {
        using var conn = _factory.CreateConnection();
        conn.Execute("UPDATE servicios_tecnicos SET estado='Certificado', certificado_url=@url WHERE id_servicio=@id",
            new { id = idServicio, url = certificadoUrl });
    }
}