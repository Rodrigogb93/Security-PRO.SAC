using Dapper;

namespace SecurityPRO.ERP.Web.Repositories;

public class ComisionRepository
{
    private readonly DbConnectionFactory _factory;
    public ComisionRepository(DbConnectionFactory factory) => _factory = factory;

    public IEnumerable<dynamic> Listar()
    {
        using var conn = _factory.CreateConnection();
        return conn.Query(
            @"SELECT lc.*, u.nombres AS NombreVendedor
              FROM liquidacion_comisiones lc
              INNER JOIN usuarios u ON lc.id_usuario_vendedor = u.id_usuario
              ORDER BY lc.periodo_anio DESC, lc.periodo_mes DESC");
    }

    public int Liquidar(int mes, int anio)
    {
        using var conn = (MySql.Data.MySqlClient.MySqlConnection)_factory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            // Vendedores con ventas cobradas en el periodo
            var vendedores = conn.Query<dynamic>(
                @"SELECT DISTINCT c.id_usuario_asesor AS idVendedor, 
                         SUM(p.total) AS totalCobrado
                  FROM pedidos p
                  INNER JOIN cotizaciones c ON p.id_cotizacion = c.id_cotizacion
                  INNER JOIN facturas f ON f.id_pedido = p.id_pedido
                  INNER JOIN cuentas_por_cobrar cxc ON cxc.id_factura = f.id_factura
                  WHERE cxc.estado = 'Pagado'
                    AND MONTH(f.fecha_emision) = @mes AND YEAR(f.fecha_emision) = @anio
                  GROUP BY c.id_usuario_asesor", new { mes, anio }, tx);

            int contador = 0;
            foreach (var v in vendedores)
            {
                var comision = (decimal)v.totalCobrado * 0.035m; // 3.5% comisión
                conn.Execute(
                    @"INSERT INTO liquidacion_comisiones (id_usuario_vendedor, periodo_mes, periodo_anio,
                      monto_facturado_cobrado, porcentaje_comision, comision_neta, estado)
                      VALUES (@idVendedor, @mes, @anio, @totalCobrado, 3.50, @comision, 'Pendiente')",
                    new { v.idVendedor, mes, anio, v.totalCobrado, comision }, tx);
                contador++;
            }
            tx.Commit();
            return contador;
        }
        catch { tx.Rollback(); throw; }
    }
}