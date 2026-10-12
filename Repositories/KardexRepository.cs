using Dapper;
using SecurityPRO.ERP.Web.DTOs;
using SecurityPRO.ERP.Web.Models;

namespace SecurityPRO.ERP.Web.Repositories;

public class KardexRepository
{
    private readonly DbConnectionFactory _factory;

    public KardexRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    // ============================================================
    // LISTAR MOVIMIENTOS DE KARDEX
    // ============================================================

    public IEnumerable<Kardex> Listar(int? idProducto)
    {
        using var conn = _factory.CreateConnection();

        var sql = @"
            SELECT
                k.id_kardex AS IdKardex,
                k.id_producto AS IdProducto,
                k.id_lote AS IdLote,
                k.tipo_movimiento AS TipoMovimiento,
                k.cantidad AS Cantidad,
                k.saldo_anterior AS SaldoAnterior,
                k.saldo_actual AS SaldoActual,
                k.costo_unitario AS CostoUnitario,
                k.documento_referencia AS DocumentoReferencia,
                k.id_usuario AS IdUsuario,
                k.fecha_movimiento AS FechaMovimiento,
                k.observaciones AS Observaciones,

                p.nombre AS NombreProducto,
                p.sku AS Sku,
                l.numero_lote AS NumeroLote

            FROM kardex k

            INNER JOIN productos p
                ON k.id_producto = p.id_producto

            LEFT JOIN lotes l
                ON k.id_lote = l.id_lote";

        if (idProducto.HasValue)
        {
            sql += " WHERE k.id_producto = @idProducto";
        }

        sql += @"
            ORDER BY k.fecha_movimiento DESC,
                     k.id_kardex DESC
            LIMIT 500";

        return conn.Query<Kardex>(
            sql,
            new { idProducto }
        );
    }


    // ============================================================
    // LISTAR LOTES DISPONIBLES
    // ============================================================

    public IEnumerable<Lote> ListarLotes(int idProducto)
    {
        using var conn = _factory.CreateConnection();

        return conn.Query<Lote>(
            @"
            SELECT
                id_lote AS IdLote,
                id_producto AS IdProducto,
                numero_lote AS NumeroLote,
                fecha_fabricacion AS FechaFabricacion,
                fecha_vencimiento AS FechaVencimiento,
                cantidad_inicial AS CantidadInicial,
                cantidad_actual AS CantidadActual

            FROM lotes

            WHERE id_producto = @idProducto
              AND cantidad_actual > 0

            ORDER BY
                CASE
                    WHEN fecha_vencimiento IS NULL THEN 1
                    ELSE 0
                END,
                fecha_vencimiento ASC,
                id_lote ASC",
            new { idProducto }
        );
    }


    // ============================================================
    // REGISTRAR MOVIMIENTO
    // ============================================================

    public int RegistrarMovimiento(
        MovimientoKardexRequest request,
        int idUsuario)
    {
        using var conn =
            (MySql.Data.MySqlClient.MySqlConnection)
            _factory.CreateConnection();

        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            // ----------------------------------------------------
            // VALIDACIONES BÁSICAS
            // ----------------------------------------------------

            if (request.IdProducto <= 0)
            {
                throw new Exception(
                    "Debe seleccionar un producto válido"
                );
            }

            if (request.Cantidad <= 0)
            {
                throw new Exception(
                    "La cantidad debe ser mayor que cero"
                );
            }

            var tiposPermitidos = new[]
            {
                "Entrada",
                "Salida",
                "Baja",
                "Merma",
                "Ajuste"
            };

            if (!tiposPermitidos.Contains(request.TipoMovimiento))
            {
                throw new Exception(
                    "Tipo de movimiento no válido"
                );
            }


            // ----------------------------------------------------
            // OBTENER STOCK GENERAL ACTUAL
            // ----------------------------------------------------

            var saldoActual = conn.ExecuteScalar<int>(
                @"
                SELECT COALESCE(SUM(cantidad), 0)

                FROM kardex

                WHERE id_producto = @IdProducto",
                new
                {
                    request.IdProducto
                },
                transaction
            );


            // ----------------------------------------------------
            // DETERMINAR SIGNO DEL MOVIMIENTO
            // ----------------------------------------------------

            bool esEntrada =
                request.TipoMovimiento == "Entrada";

            bool esSalida =
                request.TipoMovimiento == "Salida" ||
                request.TipoMovimiento == "Baja" ||
                request.TipoMovimiento == "Merma";

            /*
             * Por ahora tratamos Ajuste como incremento.
             * Después podemos agregar Ajuste + / Ajuste -
             * explícitamente en el formulario.
             */
            bool esAjuste =
                request.TipoMovimiento == "Ajuste";


            int cantidadMovimiento;

            if (esEntrada || esAjuste)
            {
                cantidadMovimiento =
                    Math.Abs(request.Cantidad);
            }
            else
            {
                cantidadMovimiento =
                    -Math.Abs(request.Cantidad);
            }


            int nuevoSaldo =
                saldoActual + cantidadMovimiento;


            if (nuevoSaldo < 0)
            {
                throw new Exception(
                    $"Stock insuficiente. Stock disponible: {saldoActual}"
                );
            }


            // ----------------------------------------------------
            // MANEJO DEL LOTE
            // ----------------------------------------------------

            int? idLote = null;


            // ====================================================
            // ENTRADA
            // ====================================================

            if (esEntrada)
            {
                if (!string.IsNullOrWhiteSpace(request.NumeroLote))
                {
                    /*
                     * Primero comprobamos si el lote ya existe
                     * para ese producto.
                     */

                    var loteExistente =
                        conn.QueryFirstOrDefault<Lote>(
                            @"
                            SELECT
                                id_lote AS IdLote,
                                id_producto AS IdProducto,
                                numero_lote AS NumeroLote,
                                fecha_fabricacion AS FechaFabricacion,
                                fecha_vencimiento AS FechaVencimiento,
                                cantidad_inicial AS CantidadInicial,
                                cantidad_actual AS CantidadActual

                            FROM lotes

                            WHERE id_producto = @IdProducto
                              AND numero_lote = @NumeroLote

                            LIMIT 1",
                            new
                            {
                                request.IdProducto,
                                request.NumeroLote
                            },
                            transaction
                        );


                    // --------------------------------------------
                    // LOTE YA EXISTE
                    // --------------------------------------------

                    if (loteExistente != null)
                    {
                        idLote = loteExistente.IdLote;

                        conn.Execute(
                            @"
                            UPDATE lotes

                            SET
                                cantidad_actual =
                                    cantidad_actual + @Cantidad,

                                fecha_vencimiento =
                                    COALESCE(
                                        @FechaVencimiento,
                                        fecha_vencimiento
                                    )

                            WHERE id_lote = @IdLote",
                            new
                            {
                                request.Cantidad,
                                request.FechaVencimiento,
                                IdLote = idLote
                            },
                            transaction
                        );
                    }

                    // --------------------------------------------
                    // LOTE NUEVO
                    // --------------------------------------------

                    else
                    {
                        idLote =
                            conn.ExecuteScalar<int>(
                                @"
                                INSERT INTO lotes
                                (
                                    id_producto,
                                    numero_lote,
                                    fecha_vencimiento,
                                    cantidad_inicial,
                                    cantidad_actual
                                )
                                VALUES
                                (
                                    @IdProducto,
                                    @NumeroLote,
                                    @FechaVencimiento,
                                    @Cantidad,
                                    @Cantidad
                                );

                                SELECT LAST_INSERT_ID();",
                                new
                                {
                                    request.IdProducto,
                                    request.NumeroLote,
                                    request.FechaVencimiento,
                                    request.Cantidad
                                },
                                transaction
                            );

                        /*
                         * IMPORTANTE:
                         * Ya NO volvemos a sumar la cantidad aquí.
                         *
                         * Antes:
                         * lote nuevo = 100
                         * luego +100
                         * resultado = 200 ❌
                         *
                         * Ahora:
                         * lote nuevo = 100 ✅
                         */
                    }
                }
            }


            // ====================================================
            // SALIDA / BAJA / MERMA
            // ====================================================

            if (esSalida)
            {
                if (!request.IdLote.HasValue)
                {
                    throw new Exception(
                        "Debe seleccionar el lote del cual se retirará el producto"
                    );
                }


                var lote =
                    conn.QueryFirstOrDefault<Lote>(
                        @"
                        SELECT
                            id_lote AS IdLote,
                            id_producto AS IdProducto,
                            numero_lote AS NumeroLote,
                            fecha_fabricacion AS FechaFabricacion,
                            fecha_vencimiento AS FechaVencimiento,
                            cantidad_inicial AS CantidadInicial,
                            cantidad_actual AS CantidadActual

                        FROM lotes

                        WHERE id_lote = @IdLote
                          AND id_producto = @IdProducto

                        LIMIT 1",
                        new
                        {
                            IdLote = request.IdLote.Value,
                            request.IdProducto
                        },
                        transaction
                    );


                if (lote == null)
                {
                    throw new Exception(
                        "El lote seleccionado no existe para este producto"
                    );
                }


                if (lote.CantidadActual < request.Cantidad)
                {
                    throw new Exception(
                        $"Stock insuficiente en el lote {lote.NumeroLote}. " +
                        $"Disponible: {lote.CantidadActual}"
                    );
                }


                idLote = lote.IdLote;


                conn.Execute(
                    @"
                    UPDATE lotes

                    SET cantidad_actual =
                        cantidad_actual - @Cantidad

                    WHERE id_lote = @IdLote",
                    new
                    {
                        request.Cantidad,
                        IdLote = lote.IdLote
                    },
                    transaction
                );
            }


            // ====================================================
            // AJUSTE
            // ====================================================

            if (esAjuste && request.IdLote.HasValue)
            {
                var lote =
                    conn.QueryFirstOrDefault<Lote>(
                        @"
                        SELECT
                            id_lote AS IdLote,
                            id_producto AS IdProducto,
                            numero_lote AS NumeroLote,
                            fecha_fabricacion AS FechaFabricacion,
                            fecha_vencimiento AS FechaVencimiento,
                            cantidad_inicial AS CantidadInicial,
                            cantidad_actual AS CantidadActual

                        FROM lotes

                        WHERE id_lote = @IdLote
                          AND id_producto = @IdProducto

                        LIMIT 1",
                        new
                        {
                            IdLote = request.IdLote.Value,
                            request.IdProducto
                        },
                        transaction
                    );


                if (lote == null)
                {
                    throw new Exception(
                        "El lote seleccionado no existe"
                    );
                }


                idLote = lote.IdLote;


                conn.Execute(
                    @"
                    UPDATE lotes

                    SET cantidad_actual =
                        cantidad_actual + @Cantidad

                    WHERE id_lote = @IdLote",
                    new
                    {
                        request.Cantidad,
                        IdLote = lote.IdLote
                    },
                    transaction
                );
            }


            // ----------------------------------------------------
            // REGISTRAR KARDEX
            // ----------------------------------------------------

            var id = conn.ExecuteScalar<int>(
                @"
                INSERT INTO kardex
                (
                    id_producto,
                    id_lote,
                    tipo_movimiento,
                    cantidad,
                    saldo_anterior,
                    saldo_actual,
                    costo_unitario,
                    documento_referencia,
                    id_usuario,
                    observaciones
                )
                VALUES
                (
                    @IdProducto,
                    @IdLote,
                    @TipoMovimiento,
                    @Cantidad,
                    @SaldoAnterior,
                    @SaldoActual,
                    @CostoUnitario,
                    @DocumentoReferencia,
                    @IdUsuario,
                    @Observaciones
                );

                SELECT LAST_INSERT_ID();",
                new
                {
                    request.IdProducto,

                    IdLote = idLote,

                    request.TipoMovimiento,

                    Cantidad = cantidadMovimiento,

                    SaldoAnterior = saldoActual,

                    SaldoActual = nuevoSaldo,

                    request.CostoUnitario,

                    request.DocumentoReferencia,

                    IdUsuario = idUsuario,

                    request.Observaciones
                },
                transaction
            );


            transaction.Commit();

            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


    // ============================================================
    // SEMÁFORO DE VENCIMIENTOS
    // ============================================================

    public IEnumerable<SemáforoVencimiento>
        ObtenerSemáforoVencimiento()
    {
        using var conn = _factory.CreateConnection();

        var sql = @"
            SELECT
                l.id_producto AS IdProducto,
                p.sku AS Sku,
                p.nombre AS Producto,
                l.numero_lote AS NumeroLote,
                l.fecha_vencimiento AS FechaVencimiento

            FROM lotes l

            INNER JOIN productos p
                ON l.id_producto = p.id_producto

            WHERE l.cantidad_actual > 0
              AND l.fecha_vencimiento IS NOT NULL

            ORDER BY l.fecha_vencimiento";

        var lotes =
            conn.Query<SemáforoVencimiento>(sql)
                .ToList();


        foreach (var lote in lotes)
        {
            if (!lote.FechaVencimiento.HasValue)
            {
                continue;
            }


            int dias =
                (
                    lote.FechaVencimiento.Value.Date
                    - DateTime.Today
                ).Days;


            lote.DiasRestantes = dias;


            // --------------------------------------------
            // NUEVO SEMÁFORO
            // --------------------------------------------

            if (dias <= 0)
            {
                lote.Color = "Vencido";
            }
            else if (dias <= 30)
            {
                lote.Color = "Rojo";
            }
            else if (dias <= 90)
            {
                lote.Color = "Amarillo";
            }
            else
            {
                lote.Color = "Verde";
            }
        }


        return lotes;
    }
}