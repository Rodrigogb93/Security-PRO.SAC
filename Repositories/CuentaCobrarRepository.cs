using Dapper;

namespace SecurityPRO.ERP.Web.Repositories;

public class CuentaCobrarRepository
{
    private readonly DbConnectionFactory _factory;

    public CuentaCobrarRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    // =========================================================
    // LISTAR CUENTAS POR COBRAR
    // =========================================================

    public IEnumerable<dynamic> Listar()
    {
        using var conn = _factory.CreateConnection();

        return conn.Query(
            @"SELECT
                cxc.id_cxc AS idCxc,
                cxc.id_factura AS idFactura,
                cxc.id_cliente AS idCliente,
                cxc.monto_total AS montoTotal,
                cxc.saldo_pendiente AS saldoPendiente,
                cxc.fecha_vencimiento AS fechaVencimiento,
                cxc.estado AS estado,

                f.serie AS serie,
                f.numero AS numeroFactura,

                cl.razon_social AS razonSocialCliente

              FROM cuentas_por_cobrar cxc

              INNER JOIN facturas f
                  ON cxc.id_factura = f.id_factura

              INNER JOIN clientes cl
                  ON cxc.id_cliente = cl.id_cliente

              ORDER BY cxc.fecha_vencimiento"
        );
    }

    // =========================================================
    // REGISTRAR PAGO
    // =========================================================

    public void RegistrarPago(
        int idCxc,
        decimal monto,
        string metodo,
        string numOperacion)
    {
        using var conn =
            (MySql.Data.MySqlClient.MySqlConnection)
            _factory.CreateConnection();

        conn.Open();

        using var tx = conn.BeginTransaction();

        try
        {
            var cxc =
                conn.QueryFirstOrDefault<dynamic>(
                    @"SELECT *
                      FROM cuentas_por_cobrar
                      WHERE id_cxc = @id
                      FOR UPDATE",
                    new { id = idCxc },
                    tx
                )
                ?? throw new Exception(
                    "Cuenta no encontrada"
                );

            if (monto <= 0)
            {
                throw new Exception(
                    "El monto debe ser mayor a cero"
                );
            }

            if (monto > cxc.saldo_pendiente)
            {
                throw new Exception(
                    "El monto excede el saldo pendiente"
                );
            }

            conn.Execute(
                @"INSERT INTO pagos_cobranzas
                    (
                        id_cxc,
                        monto_pagado,
                        metodo_pago,
                        numero_operacion
                    )
                  VALUES
                    (
                        @idCxc,
                        @monto,
                        @metodo,
                        @numOperacion
                    )",
                new
                {
                    idCxc,
                    monto,
                    metodo,
                    numOperacion
                },
                tx
            );

            decimal nuevoSaldo =
                cxc.saldo_pendiente - monto;

            string estado =
                nuevoSaldo <= 0
                    ? "Pagado"
                    : "Pendiente";

            conn.Execute(
                @"UPDATE cuentas_por_cobrar
                  SET
                      saldo_pendiente = @nuevoSaldo,
                      estado = @estado
                  WHERE id_cxc = @idCxc",
                new
                {
                    nuevoSaldo,
                    estado,
                    idCxc
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