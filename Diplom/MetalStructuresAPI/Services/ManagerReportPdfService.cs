using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using MetalStructuresAPI.DTOs;

namespace MetalStructuresAPI.Services;

public class ManagerReportPdfService
{
    private static readonly string FontFamily = GetSafeFontFamily();

    private static string GetSafeFontFamily()
    {
        foreach (var name in new[] { "Arial", "Verdana", "Segoe UI", "Liberation Sans", "DejaVu Sans", "Helvetica" })
        {
            try
            {
                var _ = new XFont(name, 10, XFontStyle.Regular);
                return name;
            }
            catch { /* пробуем следующий шрифт */ }
        }
        return "Arial";
    }

    public byte[] GenerateReport(
        string adminName,
        DateTime from,
        DateTime to,
        List<ManagerActivityDto> stats)
    {
        var document = new PdfDocument();
        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;
        var gfx = XGraphics.FromPdfPage(page);

        var fontTitle = new XFont(FontFamily, 16, XFontStyle.Bold);
        var fontBold = new XFont(FontFamily, 10, XFontStyle.Bold);
        var fontNormal = new XFont(FontFamily, 10, XFontStyle.Regular);
        var fontSmall = new XFont(FontFamily, 8, XFontStyle.Regular);

        double y = 40;
        const double margin = 40;
        const double lineHeight = 14;

        gfx.DrawString("ОТЧЕТ ПО ДЕЯТЕЛЬНОСТИ МЕНЕДЖЕРОВ", fontTitle, XBrushes.Black, margin, y);
        y += lineHeight * 2;

        gfx.DrawString($"Период: {from:dd.MM.yyyy} — {to:dd.MM.yyyy}", fontNormal, XBrushes.Black, margin, y);
        y += lineHeight;
        gfx.DrawString($"Администратор: {adminName}", fontNormal, XBrushes.Black, margin, y);
        y += lineHeight;
        gfx.DrawString($"Дата формирования отчета: {DateTime.UtcNow:dd.MM.yyyy HH:mm} (UTC)", fontSmall, XBrushes.Gray, margin, y);
        y += lineHeight * 2;

        gfx.DrawString("Сводная информация по менеджерам:", fontBold, XBrushes.Black, margin, y);
        y += lineHeight * 2;

        // Table header
        double col1 = margin;
        double col2 = margin + 160;
        double col3 = margin + 260;
        double col4 = margin + 360;

        gfx.DrawString("Менеджер", fontBold, XBrushes.Black, col1, y);
        gfx.DrawString("Расчетов", fontBold, XBrushes.Black, col2, y);
        gfx.DrawString("КП", fontBold, XBrushes.Black, col3, y);
        gfx.DrawString("Сумма КП, руб.", fontBold, XBrushes.Black, col4, y);
        y += lineHeight;

        gfx.DrawLine(XPens.LightGray, margin, y, page.Width - margin, y);
        y += lineHeight;

        decimal totalAmountAll = 0;
        int totalCalcsAll = 0;
        int totalProposalsAll = 0;

        foreach (var m in stats.OrderBy(s => s.ManagerFullName))
        {
            if (y > page.Height - margin - 40)
            {
                page = document.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                gfx = XGraphics.FromPdfPage(page);
                y = margin;
            }

            gfx.DrawString(m.ManagerFullName, fontNormal, XBrushes.Black, col1, y);
            gfx.DrawString(m.CalculationsCount.ToString(), fontNormal, XBrushes.Black, col2, y);
            gfx.DrawString(m.CommercialProposalsCount.ToString(), fontNormal, XBrushes.Black, col3, y);
            gfx.DrawString(m.CommercialProposalsTotalAmount.ToString("N2"), fontNormal, XBrushes.Black, col4, y);
            y += lineHeight;

            totalCalcsAll += m.CalculationsCount;
            totalProposalsAll += m.CommercialProposalsCount;
            totalAmountAll += m.CommercialProposalsTotalAmount;
        }

        y += lineHeight;
        gfx.DrawLine(XPens.Gray, margin, y, page.Width - margin, y);
        y += lineHeight;

        gfx.DrawString($"ИТОГО расчетов: {totalCalcsAll}", fontBold, XBrushes.Black, col1, y);
        y += lineHeight;
        gfx.DrawString($"ИТОГО КП: {totalProposalsAll}", fontBold, XBrushes.Black, col1, y);
        y += lineHeight;
        gfx.DrawString($"ИТОГО сумма КП: {totalAmountAll:N2} руб.", fontBold, XBrushes.Black, col1, y);

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }
}

