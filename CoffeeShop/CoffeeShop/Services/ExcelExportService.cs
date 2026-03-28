using ClosedXML.Excel;
using CoffeeShop.Models;

namespace CoffeeShop.Services;

public class ExcelExportService
{
    public void ExportShiftReport(Shift shift, List<StaffEarning> earnings, string filePath)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Отчёт смены");

        ws.Cell(1, 1).Value = "Отчёт смены CoffeeShop";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;
        ws.Range(1, 1, 1, 5).Merge();

        ws.Cell(3, 1).Value = "Дата открытия:";
        ws.Cell(3, 2).Value = shift.OpenedAt.ToString("dd.MM.yyyy HH:mm");
        ws.Cell(4, 1).Value = "Дата закрытия:";
        ws.Cell(4, 2).Value = shift.ClosedAt?.ToString("dd.MM.yyyy HH:mm") ?? "Не закрыта";
        ws.Cell(5, 1).Value = "Общая выручка:";
        ws.Cell(5, 2).Value = shift.TotalRevenue;
        ws.Cell(5, 2).Style.NumberFormat.Format = "#,##0.00 ₽";

        ws.Cell(7, 1).Value = "Сотрудник";
        ws.Cell(7, 2).Value = "Роль";
        ws.Cell(7, 3).Value = "Заказов";
        ws.Cell(7, 4).Value = "Сумма заказов";
        ws.Cell(7, 5).Value = "Ставка %";
        ws.Cell(7, 6).Value = "Начислено";

        var headerRange = ws.Range(7, 1, 7, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#6F4E37");
        headerRange.Style.Font.FontColor = XLColor.White;

        int row = 8;
        foreach (var e in earnings)
        {
            ws.Cell(row, 1).Value = e.User?.FullName ?? "";
            ws.Cell(row, 2).Value = e.User?.Role?.Name ?? "";
            ws.Cell(row, 3).Value = e.OrdersCount;
            ws.Cell(row, 4).Value = e.OrdersTotal;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₽";
            ws.Cell(row, 5).Value = e.EarningPercent;
            ws.Cell(row, 5).Style.NumberFormat.Format = "0.00%";
            ws.Cell(row, 6).Value = e.EarnedAmount;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00 ₽";
            row++;
        }

        ws.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
    }
}
