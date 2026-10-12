using ClosedXML.Excel;
using SecurityPRO.ERP.Web.Models;

namespace SecurityPRO.ERP.Web.Services;

public class ExcelService
{
    public byte[] GenerarKardexExcel(IEnumerable<Kardex> datos)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Kardex");

        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "SKU";
        ws.Cell(1, 3).Value = "Producto";
        ws.Cell(1, 4).Value = "Lote";
        ws.Cell(1, 5).Value = "Tipo Movimiento";
        ws.Cell(1, 6).Value = "Cantidad";
        ws.Cell(1, 7).Value = "Saldo Anterior";
        ws.Cell(1, 8).Value = "Saldo Actual";
        ws.Cell(1, 9).Value = "Fecha";
        ws.Cell(1, 10).Value = "Documento";

        var header = ws.Range(1, 1, 1, 10);
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#0EA5E9");
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Font.Bold = true;

        int fila = 2;
        foreach (var k in datos)
        {
            ws.Cell(fila, 1).Value = k.IdKardex;
            ws.Cell(fila, 2).Value = k.Sku ?? "";
            ws.Cell(fila, 3).Value = k.NombreProducto ?? "";
            ws.Cell(fila, 4).Value = k.NumeroLote ?? "";
            ws.Cell(fila, 5).Value = k.TipoMovimiento;
            ws.Cell(fila, 6).Value = k.Cantidad;
            ws.Cell(fila, 7).Value = k.SaldoAnterior;
            ws.Cell(fila, 8).Value = k.SaldoActual;
            ws.Cell(fila, 9).Value = k.FechaMovimiento;
            ws.Cell(fila, 10).Value = k.DocumentoReferencia ?? "";
            fila++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}