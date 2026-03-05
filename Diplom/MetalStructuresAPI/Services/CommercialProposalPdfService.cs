using System.IO;
using MetalStructuresAPI.Models;
using Microsoft.Extensions.Hosting;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace MetalStructuresAPI.Services;

public class CommercialProposalPdfService
{
    private static readonly string FontFamily = GetSafeFontFamily();
    private readonly IHostEnvironment _environment;

    public CommercialProposalPdfService(IHostEnvironment environment)
    {
        _environment = environment;
    }

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

    public byte[] GeneratePdf(
        CompanyInfo company,
        CommercialProposal proposal,
        Calculation calculation,
        List<CalculationItem> items)
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

        // Logo (if available)
        try
        {
            var logoPath = Path.Combine(_environment.ContentRootPath, "images", "logo.png");
            if (File.Exists(logoPath))
            {
                using var logo = XImage.FromFile(logoPath);
                const double logoHeight = 40;
                var logoWidth = logo.PixelWidth * logoHeight / logo.PixelHeight;
                gfx.DrawImage(logo, page.Width - margin - logoWidth, y - 10, logoWidth, logoHeight);
            }
        }
        catch
        {
            // Игнорируем ошибки логотипа, КП все равно формируется
        }

        // Company header
        gfx.DrawString(company.Name, fontBold, XBrushes.Black, margin, y);
        y += lineHeight;
        gfx.DrawString(company.Address, fontNormal, XBrushes.Black, margin, y);
        y += lineHeight;
        gfx.DrawString($"Тел: {company.Phone} | Email: {company.Email}", fontNormal, XBrushes.Black, margin, y);
        y += lineHeight;
        gfx.DrawString($"ИНН: {company.Inn}" + (string.IsNullOrEmpty(company.Kpp) ? "" : $" | КПП: {company.Kpp}"), fontNormal, XBrushes.Black, margin, y);
        y += lineHeight;
        gfx.DrawString(company.BankDetails, fontSmall, XBrushes.Black, margin, y);
        y += lineHeight * 2;

        // Title
        gfx.DrawString("КОММЕРЧЕСКОЕ ПРЕДЛОЖЕНИЕ", fontTitle, XBrushes.Black, margin, y);
        y += lineHeight * 2;

        // Customer info
        gfx.DrawString("Для:", fontBold, XBrushes.Black, margin, y);
        y += lineHeight;
        gfx.DrawString(proposal.CustomerCompany, fontNormal, XBrushes.Black, margin, y);
        y += lineHeight;
        gfx.DrawString($"Контактное лицо: {proposal.CustomerPerson}", fontNormal, XBrushes.Black, margin, y);
        y += lineHeight;
        gfx.DrawString($"Тел: {proposal.CustomerPhone} | Email: {proposal.CustomerEmail}", fontNormal, XBrushes.Black, margin, y);
        y += lineHeight;
        if (!string.IsNullOrEmpty(proposal.CustomerAddress))
        {
            gfx.DrawString($"Адрес: {proposal.CustomerAddress}", fontNormal, XBrushes.Black, margin, y);
            y += lineHeight;
        }
        y += lineHeight;

        if (!string.IsNullOrEmpty(proposal.ProposalNumber))
        {
            gfx.DrawString($"№ {proposal.ProposalNumber}", fontNormal, XBrushes.Black, margin, y);
            y += lineHeight;
        }
        gfx.DrawString($"Дата: {proposal.CreatedAt:dd.MM.yyyy}", fontNormal, XBrushes.Black, margin, y);
        y += lineHeight * 2;

        // Table header
        double col1 = margin;
        double col2 = margin + 35;
        double col3 = margin + 120;
        double col4 = margin + 280;
        double col5 = margin + 330;
        double col6 = margin + 400;
        double col7 = margin + 470;

        gfx.DrawString("№", fontBold, XBrushes.Black, col1, y);
        gfx.DrawString("Артикул", fontBold, XBrushes.Black, col2, y);
        gfx.DrawString("Название", fontBold, XBrushes.Black, col3, y);
        gfx.DrawString("Ед.", fontBold, XBrushes.Black, col4, y);
        gfx.DrawString("Кол-во", fontBold, XBrushes.Black, col5, y);
        gfx.DrawString("Цена", fontBold, XBrushes.Black, col6, y);
        gfx.DrawString("Сумма", fontBold, XBrushes.Black, col7, y);
        y += lineHeight;

        gfx.DrawLine(XPens.LightGray, margin, y, page.Width - margin, y);
        y += lineHeight;

        int rowNum = 1;
        foreach (var item in items)
        {
            var material = item.Material ?? throw new InvalidOperationException($"Материал не найден для позиции расчёта (CalculationItem Id: {item.Id}).");
            gfx.DrawString(rowNum.ToString(), fontNormal, XBrushes.Black, col1, y);
            gfx.DrawString(material.Article, fontNormal, XBrushes.Black, col2, y);
            gfx.DrawString(Truncate(material.Name, 35), fontNormal, XBrushes.Black, col3, y);
            gfx.DrawString(material.Unit, fontNormal, XBrushes.Black, col4, y);
            gfx.DrawString(item.Quantity.ToString("N3"), fontNormal, XBrushes.Black, col5, y);
            gfx.DrawString(item.UnitPrice.ToString("N2"), fontNormal, XBrushes.Black, col6, y);
            gfx.DrawString(item.TotalPrice.ToString("N2"), fontNormal, XBrushes.Black, col7, y);
            y += lineHeight;
            rowNum++;
        }

        y += lineHeight;
        gfx.DrawString($"Итого: {calculation.TotalAmount:N2} руб.", fontBold, XBrushes.Black, col7 - 60, y);
        y += lineHeight * 2;

        if (!string.IsNullOrEmpty(proposal.Comments))
        {
            gfx.DrawString("Примечания:", fontBold, XBrushes.Black, margin, y);
            y += lineHeight;
            gfx.DrawString(proposal.Comments, fontNormal, XBrushes.Black, margin, y);
        }

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }
}
