using Dapper;

namespace SecurityPRO.ERP.Web.Repositories;

public class CuentaPagarRepository
{
    private readonly DbConnectionFactory _factory;

    public CuentaPagarRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public IEnumerable<dynamic> Listar()
{
    using var conn = _factory.CreateConnection();

    return conn.Query(
        @"SELECT
            cxp.id_cxp AS IdCxp,
            cxp.id_proveedor AS IdProveedor,
            cxp.numero_factura_proveedor AS NumeroFacturaProveedor,
            cxp.monto_total AS MontoTotal,
            cxp.saldo_pendiente AS SaldoPendiente,
            cxp.fecha_vencimiento AS FechaVencimiento,
            cxp.estado AS Estado,
            p.razon_social AS RazonSocialProveedor
        FROM cuentas_por_pagar cxp
        INNER JOIN proveedores p
            ON cxp.id_proveedor = p.id_proveedor
        ORDER BY cxp.fecha_vencimiento");
}
public int Crear(
    int idProveedor,
    string numeroFactura,
    decimal montoTotal,
    DateTime? fechaVencimiento)
{
    using var conn = _factory.CreateConnection();

    if (montoTotal <= 0)
        throw new Exception("El monto total debe ser mayor a cero");

    var proveedorExiste = conn.ExecuteScalar<int>(
        @"SELECT COUNT(*)
        FROM proveedores
        WHERE id_proveedor = @idProveedor
            AND activo = 1",
        new { idProveedor }
    );

    if (proveedorExiste == 0)
        throw new Exception("El proveedor no existe o está inactivo");

    var id = conn.ExecuteScalar<int>(
        @"INSERT INTO cuentas_por_pagar
        (
        id_proveedor,
            numero_factura_proveedor,
            monto_total,
            saldo_pendiente,
            fecha_vencimiento,
            estado
        )
        VALUES
        (
            @idProveedor,
            @numeroFactura,
            @montoTotal,
            @montoTotal,
            @fechaVencimiento,
            'Pendiente'
        );

        SELECT LAST_INSERT_ID();",
        new
        {
            idProveedor,
            numeroFactura,
            montoTotal,
            fechaVencimiento
        }
    );

    return id;
}
    public void RegistrarPago(int idCxp, decimal monto)
    {
        using var conn = (MySql.Data.MySqlClient.MySqlConnection)
            _factory.CreateConnection();

        conn.Open();

        using var tx = conn.BeginTransaction();

        try
        {
            var cxp = conn.QueryFirstOrDefault<dynamic>(
                @"SELECT * 
                  FROM cuentas_por_pagar 
                  WHERE id_cxp = @id 
                  FOR UPDATE",
                new { id = idCxp },
                tx
            ) ?? throw new Exception("Cuenta por pagar no encontrada");

            if (monto <= 0)
                throw new Exception("El monto debe ser mayor a cero");

            if (monto > cxp.saldo_pendiente)
                throw new Exception("El monto excede el saldo pendiente");

            decimal nuevoSaldo = cxp.saldo_pendiente - monto;

            string estado = nuevoSaldo <= 0
                ? "Pagado"
                : "Pendiente";

            conn.Execute(
                @"UPDATE cuentas_por_pagar
                  SET saldo_pendiente = @nuevoSaldo,
                      estado = @estado
                  WHERE id_cxp = @idCxp",
                new
                {
                    nuevoSaldo,
                    estado,
                    idCxp
                },
                tx
            );

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}