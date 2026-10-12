using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using SecurityPRO.ERP.Web.Models;
using SecurityPRO.ERP.Web.Repositories;

namespace SecurityPRO.ERP.Web.Services;

public class PdfService
{
    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // =========================================================
    // PDF DE COTIZACIÓN
    // =========================================================
    public byte[] GenerarCotizacionPdf(Cotizacion c)
    {
        var qr = GenerarQr(
            $"COT:{c.NumeroCotizacion}|RUC:20456789123|Total:{c.Total}"
        );

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                // ENCABEZADO
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c2 =>
                        {
                            c2.Item()
                                .Text("SECURITY PRO S.A.C.")
                                .FontSize(18)
                                .Bold()
                                .FontColor("#0EA5E9");

                            c2.Item()
                                .Text("RUC: 20456789123")
                                .FontSize(9);

                            c2.Item()
                                .Text("Av. Los Industriales 456, Arequipa - Perú")
                                .FontSize(9);

                            c2.Item()
                                .Text("Tel: (054) 123456 | Email: ventas@securitypro.pe")
                                .FontSize(9);
                        });

                        row.ConstantItem(100)
                            .Image(qr)
                            .FitWidth();
                    });
                });

                // CONTENIDO
                page.Content()
                    .PaddingVertical(15)
                    .Column(col =>
                    {
                        col.Item()
                            .AlignCenter()
                            .Text($"COTIZACIÓN N° {c.NumeroCotizacion}")
                            .FontSize(14)
                            .Bold();

                        col.Item()
                            .PaddingTop(10)
                            .Text(
                                $"Fecha: {c.FechaEmision:dd/MM/yyyy} | " +
                                $"Validez: {c.ValidezDias} días"
                            );

                        col.Item()
                            .Text($"Cliente: {c.RazonSocialCliente}");

                        col.Item()
                            .Text($"Asesor: {c.NombreAsesor}");

                        // TABLA
                        col.Item()
                            .PaddingTop(15)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(60);
                                    cols.RelativeColumn(3);
                                    cols.ConstantColumn(50);
                                    cols.ConstantColumn(70);
                                    cols.ConstantColumn(70);
                                    cols.ConstantColumn(80);
                                });

                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Background("#0EA5E9")
                                        .Padding(5)
                                        .Text("SKU")
                                        .FontColor("#FFFFFF")
                                        .Bold();

                                    header.Cell()
                                        .Background("#0EA5E9")
                                        .Padding(5)
                                        .Text("Producto")
                                        .FontColor("#FFFFFF")
                                        .Bold();

                                    header.Cell()
                                        .Background("#0EA5E9")
                                        .Padding(5)
                                        .Text("Cant.")
                                        .FontColor("#FFFFFF")
                                        .Bold();

                                    header.Cell()
                                        .Background("#0EA5E9")
                                        .Padding(5)
                                        .Text("P. Unit.")
                                        .FontColor("#FFFFFF")
                                        .Bold();

                                    header.Cell()
                                        .Background("#0EA5E9")
                                        .Padding(5)
                                        .Text("Desc.")
                                        .FontColor("#FFFFFF")
                                        .Bold();

                                    header.Cell()
                                        .Background("#0EA5E9")
                                        .Padding(5)
                                        .Text("Subtotal")
                                        .FontColor("#FFFFFF")
                                        .Bold();
                                });

                                foreach (var d in c.Detalles)
                                {
                                    table.Cell()
                                        .Padding(4)
                                        .Text(d.Sku ?? "");

                                    table.Cell()
                                        .Padding(4)
                                        .Text(d.NombreProducto ?? "");

                                    table.Cell()
                                        .Padding(4)
                                        .Text(d.Cantidad.ToString());

                                    table.Cell()
                                        .Padding(4)
                                        .Text($"S/ {d.PrecioUnitario:N2}");

                                    table.Cell()
                                        .Padding(4)
                                        .Text($"S/ {d.DescuentoVolumen:N2}");

                                    table.Cell()
                                        .Padding(4)
                                        .Text($"S/ {d.SubtotalLinea:N2}");
                                }
                            });

                        // TOTALES
                        col.Item()
                            .PaddingTop(10)
                            .AlignRight()
                            .Column(c2 =>
                            {
                                c2.Item()
                                    .Text($"Subtotal: S/ {c.Subtotal:N2}");

                                c2.Item()
                                    .Text($"IGV (18%): S/ {c.Igv:N2}");

                                c2.Item()
                                    .Text($"TOTAL: S/ {c.Total:N2}")
                                    .FontSize(13)
                                    .Bold()
                                    .FontColor("#0EA5E9");
                            });

                        col.Item()
                            .PaddingTop(20)
                            .Text(
                                "Gracias por su preferencia. " +
                                "Esta cotización es válida por el plazo indicado."
                            )
                            .FontSize(8)
                            .Italic();
                    });

                // PIE DE PÁGINA
                page.Footer()
                    .AlignCenter()
                    .Text(t =>
                    {
                        t.Span(
                            "Security PRO S.A.C. - Sistema ERP | Página "
                        );

                        t.CurrentPageNumber();

                        t.Span(" de ");

                        t.TotalPages();
                    });
            });
        }).GeneratePdf();
    }

    // =========================================================
    // PDF DE FACTURA
    // =========================================================
    public byte[] GenerarFacturaPdf(Factura f)
    {
        var qr = GenerarQr(
            $"FACTURA:{f.Serie}-{f.Numero}|TOTAL:{f.Total}|ESTADO:{f.EstadoSunat}"
        );

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                // ENCABEZADO
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item()
                            .Text("SECURITY PRO S.A.C.")
                            .FontSize(18)
                            .Bold()
                            .FontColor("#0EA5E9");

                        col.Item()
                            .Text("RUC: 20456789123")
                            .FontSize(9);

                        col.Item()
                            .Text("Av. Los Industriales 456, Arequipa - Perú")
                            .FontSize(9);

                        col.Item()
                            .Text("ventas@securitypro.pe")
                            .FontSize(9);
                    });

                    row.ConstantItem(150)
                        .Border(1)
                        .BorderColor("#0EA5E9")
                        .Padding(10)
                        .Column(col =>
                        {
                            col.Item()
                                .AlignCenter()
                                .Text("FACTURA ELECTRÓNICA")
                                .Bold();

                            col.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text($"{f.Serie}-{f.Numero}")
                                .FontSize(13)
                                .Bold()
                                .FontColor("#0EA5E9");
                        });
                });

                // CONTENIDO
                page.Content()
                    .PaddingVertical(20)
                    .Column(col =>
                    {
                        col.Item()
                            .Text($"Cliente: {f.RazonSocialCliente ?? "Sin información"}")
                            .Bold();

                        col.Item()
                            .PaddingTop(5)
                            .Text(
                                $"Fecha de emisión: {f.FechaEmision:dd/MM/yyyy}"
                            );

                        if (f.FechaVencimiento.HasValue)
                        {
                            col.Item()
                                .Text(
                                    $"Fecha de vencimiento: " +
                                    $"{f.FechaVencimiento.Value:dd/MM/yyyy}"
                                );
                        }

                        col.Item()
                            .Text($"Condición de pago: {f.CondicionPago}");

                        col.Item()
                            .Text($"Estado SUNAT: {f.EstadoSunat}");

                        // RESUMEN
                        col.Item()
                            .PaddingTop(25)
                            .Border(1)
                            .BorderColor("#D1D5DB")
                            .Padding(15)
                            .Column(totales =>
                            {
                                totales.Item()
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Text("Subtotal:");

                                        row.ConstantItem(120)
                                            .AlignRight()
                                            .Text($"S/ {f.Subtotal:N2}");
                                    });

                                totales.Item()
                                    .PaddingTop(5)
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Text("IGV (18%):");

                                        row.ConstantItem(120)
                                            .AlignRight()
                                            .Text($"S/ {f.Igv:N2}");
                                    });

                                totales.Item()
                                    .PaddingTop(10)
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Text("TOTAL:")
                                            .FontSize(13)
                                            .Bold();

                                        row.ConstantItem(120)
                                            .AlignRight()
                                            .Text($"S/ {f.Total:N2}")
                                            .FontSize(13)
                                            .Bold()
                                            .FontColor("#0EA5E9");
                                    });
                            });

                        // QR
                        col.Item()
                            .PaddingTop(30)
                            .AlignCenter()
                            .Width(120)
                            .Image(qr);

                        col.Item()
                            .PaddingTop(8)
                            .AlignCenter()
                            .Text(
                                $"Factura {f.Serie}-{f.Numero}"
                            )
                            .FontSize(8);

                        col.Item()
                            .PaddingTop(20)
                            .AlignCenter()
                            .Text(
                                "Representación impresa de la factura electrónica"
                            )
                            .FontSize(8)
                            .Italic();
                    });

                // PIE DE PÁGINA
                page.Footer()
                    .AlignCenter()
                    .Text(t =>
                    {
                        t.Span(
                            "Security PRO S.A.C. - Sistema ERP | Página "
                        );

                        t.CurrentPageNumber();

                        t.Span(" de ");

                        t.TotalPages();
                    });
            });
        }).GeneratePdf();
    }

    // =========================================================
    // CERTIFICADO DE SERVICIO
    // =========================================================
    public byte[] GenerarCertificadoServicioPdf(
        string titulo,
        string cliente,
        string detalle,
        string codigoQr)
    {
        var qr = GenerarQr(codigoQr);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                // ENCABEZADO
                page.Header()
                    .Column(col =>
                    {
                        col.Item()
                            .Text("SECURITY PRO S.A.C.")
                            .FontSize(18)
                            .Bold()
                            .FontColor("#0EA5E9");

                        col.Item()
                            .Text(
                                "RUC: 20456789123 - Arequipa, Perú"
                            )
                            .FontSize(9);

                        col.Item()
                            .PaddingTop(10)
                            .AlignCenter()
                            .Text(titulo)
                            .FontSize(14)
                            .Bold();
                    });

                // CONTENIDO
                page.Content()
                    .PaddingVertical(20)
                    .Column(col =>
                    {
                        col.Item()
                            .Text($"Cliente: {cliente}");

                        col.Item()
                            .Text(
                                $"Fecha de emisión: " +
                                $"{DateTime.Now:dd/MM/yyyy HH:mm}"
                            );

                        col.Item()
                            .PaddingTop(15)
                            .Text(detalle);

                        col.Item()
                            .PaddingTop(30)
                            .AlignCenter()
                            .Width(150)
                            .Image(qr);

                        col.Item()
                            .PaddingTop(10)
                            .AlignCenter()
                            .Text(
                                $"Código de verificación: {codigoQr}"
                            )
                            .FontSize(9);
                    });

                // PIE DE PÁGINA
                page.Footer()
                    .AlignCenter()
                    .Text(
                        "Documento emitido electrónicamente - " +
                        "Security PRO S.A.C."
                    );
            });
        }).GeneratePdf();
    }
    // =========================================================
// PDF DE PEDIDO
// =========================================================
public byte[] GenerarPedidoPdf(Pedido p)
{
    return Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(10));

            // ENCABEZADO
            page.Header().Column(col =>
            {
                col.Item()
                    .Text("SECURITY PRO S.A.C.")
                    .FontSize(18)
                    .Bold()
                    .FontColor("#0EA5E9");

                col.Item()
                    .Text("RUC: 20456789123")
                    .FontSize(9);

                col.Item()
                    .Text("Av. Los Industriales 456, Arequipa - Perú")
                    .FontSize(9);

                col.Item()
                    .Text("Tel: (054) 123456 | Email: ventas@securitypro.pe")
                    .FontSize(9);
            });

            // CONTENIDO
            page.Content()
                .PaddingVertical(15)
                .Column(col =>
                {
                    col.Item()
                        .AlignCenter()
                        .Text($"PEDIDO DE VENTA N° {p.NumeroPedido}")
                        .FontSize(15)
                        .Bold();

                    col.Item()
                        .PaddingTop(15)
                        .Text($"Cliente: {p.RazonSocialCliente}");

                    col.Item()
                        .Text(
                            $"Orden de Compra: " +
                            $"{(string.IsNullOrWhiteSpace(p.OrdenCompraCliente) ? "-" : p.OrdenCompraCliente)}"
                        );

                    col.Item()
                        .Text($"Fecha: {p.FechaPedido:dd/MM/yyyy}");

                    col.Item()
                        .Text($"Estado: {p.Estado}");

                    if (p.FechaEntregaProgramada.HasValue)
                    {
                        col.Item()
                            .Text(
                                $"Entrega programada: " +
                                $"{p.FechaEntregaProgramada.Value:dd/MM/yyyy}"
                            );
                    }

                    if (!string.IsNullOrWhiteSpace(p.DireccionEntrega))
                    {
                        col.Item()
                            .Text($"Dirección de entrega: {p.DireccionEntrega}");
                    }

                    // TABLA DE PRODUCTOS
                    col.Item()
                        .PaddingTop(15)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(75);
                                columns.RelativeColumn(3);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Background("#0EA5E9")
                                    .Padding(5)
                                    .Text("SKU")
                                    .FontColor("#FFFFFF")
                                    .Bold();

                                header.Cell()
                                    .Background("#0EA5E9")
                                    .Padding(5)
                                    .Text("PRODUCTO")
                                    .FontColor("#FFFFFF")
                                    .Bold();

                                header.Cell()
                                    .Background("#0EA5E9")
                                    .Padding(5)
                                    .Text("CANT.")
                                    .FontColor("#FFFFFF")
                                    .Bold();

                                header.Cell()
                                    .Background("#0EA5E9")
                                    .Padding(5)
                                    .Text("P. UNIT.")
                                    .FontColor("#FFFFFF")
                                    .Bold();

                                header.Cell()
                                    .Background("#0EA5E9")
                                    .Padding(5)
                                    .Text("SUBTOTAL")
                                    .FontColor("#FFFFFF")
                                    .Bold();
                            });

                            foreach (var d in p.Detalles)
                            {
                                table.Cell()
                                    .Padding(5)
                                    .Text(d.Sku ?? "");

                                table.Cell()
                                    .Padding(5)
                                    .Text(d.NombreProducto ?? "");

                                table.Cell()
                                    .Padding(5)
                                    .Text(d.Cantidad.ToString());

                                table.Cell()
                                    .Padding(5)
                                    .Text($"S/ {d.PrecioUnitario:N2}");

                                table.Cell()
                                    .Padding(5)
                                    .Text($"S/ {d.SubtotalLinea:N2}");
                            }
                        });

                    // TOTALES
                    col.Item()
                        .PaddingTop(15)
                        .AlignRight()
                        .Text($"Subtotal: S/ {p.Subtotal:N2}");

                    col.Item()
                        .AlignRight()
                        .Text($"IGV (18%): S/ {p.Igv:N2}");

                    col.Item()
                        .AlignRight()
                        .Text($"TOTAL: S/ {p.Total:N2}")
                        .FontSize(14)
                        .Bold()
                        .FontColor("#0EA5E9");
                });

            // PIE
            page.Footer()
                .AlignCenter()
                .Text("Security PRO S.A.C. - Pedido de Venta");
        });
    }).GeneratePdf();
}

    // =========================================================
    // GENERADOR QR
    // =========================================================
    private byte[] GenerarQr(string contenido)
    {
        using var qrGenerator = new QRCodeGenerator();

        using var qrData = qrGenerator.CreateQrCode(
            contenido,
            QRCodeGenerator.ECCLevel.Q
        );

        using var qrCode = new PngByteQRCode(qrData);

        return qrCode.GetGraphic(10);
    }
}