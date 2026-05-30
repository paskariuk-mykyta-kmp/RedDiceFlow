using RedDiceFlow.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace RedDiceFlow.Services
{
    public class ReportService
    {
        public string CreateManagerReport(IEnumerable<Product> products, IEnumerable<Order> orders)
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "RedDiceFlow Reports");

            Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, $"manager-report-{DateTime.Now:yyyy-MM-dd-HH-mm}.pdf");
            var lines = BuildReportLines(products.ToList(), orders.ToList());
            WriteSimplePdf(path, lines);

            return path;
        }

        private static List<string> BuildReportLines(List<Product> products, List<Order> orders)
        {
            var moneyCulture = CultureInfo.GetCultureInfo("en-US");

            var lines = new List<string>
            {
                "RED DICE FLOW - MANAGER REPORT",
                $"Created: {DateTime.Now:yyyy-MM-dd HH:mm}",
                "",
                $"Products in catalog: {products.Count}",
                $"Items in stock: {products.Sum(p => p.Stock)}",
                $"Stock value: {products.Sum(p => p.Price * p.Stock).ToString("C", moneyCulture)}",
                $"Total orders: {orders.Count}",
                $"Sales total: {orders.Sum(o => o.TotalPrice).ToString("C", moneyCulture)}",
                "",
                "INVENTORY",
                "Name | SKU | Genre | Stock | Price"
            };

            lines.AddRange(products.Select(p =>
                $"{p.Name} | {p.Sku} | {p.Genre} | {p.Stock} | {p.Price:0.00}"));

            lines.Add("");
            lines.Add("RECENT ORDERS");
            lines.Add("Date | Customer | Total");

            lines.AddRange(orders.Take(35).Select(o =>
                $"{o.CreatedAt:yyyy-MM-dd HH:mm} | {(string.IsNullOrEmpty(o.CustomerName) ? o.CustomerPhone : o.CustomerName)} | {o.TotalPrice:0.00}"));

            lines.Add("");
            lines.Add("LOW STOCK");

            var lowStock = products.Where(p => p.Stock <= 5).ToList();
            if (lowStock.Count == 0)
                lines.Add("No low stock products.");
            else
                lines.AddRange(lowStock.Select(p => $"{p.Name}: only {p.Stock} left"));

            return lines;
        }

        private static void WriteSimplePdf(string path, List<string> lines)
        {
            var content = new StringBuilder();
            content.AppendLine("BT");
            content.AppendLine("/F1 10 Tf");
            content.AppendLine("40 800 Td");

            foreach (var line in lines.Take(55))
            {
                content.AppendLine($"({Escape(line)}) Tj");
                content.AppendLine("0 -14 Td");
            }

            content.AppendLine("ET");

            var stream = content.ToString();

            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}endstream"
            };

            using var memory = new MemoryStream();
            using var writer = new StreamWriter(memory, Encoding.ASCII, leaveOpen: true);

            writer.WriteLine("%PDF-1.4");

            var offsets = new List<long> { 0 };
            for (var i = 0; i < objects.Count; i++)
            {
                writer.Flush();
                offsets.Add(memory.Position);
                writer.WriteLine($"{i + 1} 0 obj");
                writer.WriteLine(objects[i]);
                writer.WriteLine("endobj");
            }

            writer.Flush();
            var xref = memory.Position;

            writer.WriteLine("xref");
            writer.WriteLine($"0 {objects.Count + 1}");
            writer.WriteLine("0000000000 65535 f ");

            foreach (var offset in offsets.Skip(1))
                writer.WriteLine($"{offset:0000000000} 00000 n ");

            writer.WriteLine("trailer");
            writer.WriteLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
            writer.WriteLine("startxref");
            writer.WriteLine(xref);
            writer.WriteLine("%%EOF");
            writer.Flush();

            File.WriteAllBytes(path, memory.ToArray());
        }

        private static string Escape(string text)
        {
            return text
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
        }
    }
}