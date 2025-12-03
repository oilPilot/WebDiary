using WebDiary.Entities;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PdfSharp.Fonts;

namespace WebDiary.Helpers;

public class DiaryPdfExporter
{
    public byte[] Export(IEnumerable<Diary> diaries, string Header = "All diary entries from MyDiary:")
    {
        using var doc = new PdfDocument();
        doc.Info.Title = $"Diary Entry - {Header}";
        var page = doc.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        GlobalFontSettings.UseWindowsFontsUnderWindows = true;

        var titleFont = new XFont("Arial", 18, XFontStyleEx.Bold);
        var normalFont = new XFont("Arial", 12);
        var boldFont = new XFont("Arial", 12, XFontStyleEx.BoldItalic);

        double y = 50;
        foreach (Diary diary in diaries)
        {
            y += 15;
            if (y > page.Height - 100)
            {
                page = doc.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                y = 50;
            }
            gfx.DrawString($"Date: {new DateTime(diary.Date, diary.Time):HH:mm dd-MM-yyyy} {diary.mood}", normalFont, XBrushes.Black, 40, y);
            y += 15;
            /* We currently have BaseText
            // Convert simple HTML to text + styled drawing
            var htmlContent = diary.Text;
            var cleanHtml = htmlContent.Replace("<br>", "\n").Replace("<br/>", "\n");
            var docHtml = new HtmlDocument();
            docHtml.LoadHtml(cleanHtml);
            */
            var plainText = Regex.Replace(diary.BaseText, @"\s+", " ").Trim();

            // You could improve by walking nodes for <b>/<i> tags — simple version first:
            var textLines = XTextFormatterExtensions.SplitLines(plainText, 90); // helper (see below)
            foreach (var line in textLines)
            {
                gfx.DrawString(line, normalFont, XBrushes.Black, new XRect(40, y, page.Width - 80, 20), XStringFormats.TopLeft);
                y += 20;
                if (y > page.Height - 60)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 50;
                }
            }
        }

        using var stream = new MemoryStream();
        doc.Save(stream, false);
        return stream.ToArray();
    }
}

// Simple line wrapper helper
public static class XTextFormatterExtensions
{
    public static IEnumerable<string> SplitLines(string text, int maxCharsPerLine)
    {
        for (int i = 0; i < text.Length; i += maxCharsPerLine)
            yield return text.Substring(i, Math.Min(maxCharsPerLine, text.Length - i));
    }
}