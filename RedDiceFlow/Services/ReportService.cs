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
        private static readonly CultureInfo Money = CultureInfo.GetCultureInfo("en-US");

        public string CreateManagerReport(IEnumerable<Product> products, IEnumerable<Order> orders, string period = "all")
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "RedDiceFlow Reports");

            Directory.CreateDirectory(folder);

            var now = DateTime.Now;
            var allOrders = orders.ToList();
            var periodOrders = FilterOrdersByPeriod(allOrders, period, now).ToList();
            var deliveredOrders = periodOrders.Where(o => o.Status == "delivered").ToList();

            var path = Path.Combine(folder, $"manager-report-{now:yyyy-MM-dd-HH-mm}.pdf");
            var pages = BuildPages(products.ToList(), allOrders, periodOrders, deliveredOrders, period, now);
            WritePdf(path, pages);

            return path;
        }

        private static IEnumerable<Order> FilterOrdersByPeriod(IEnumerable<Order> orders, string period, DateTime now)
        {
            return period switch
            {
                "today" => orders.Where(o => o.CreatedAt.Date == now.Date),
                "week" => orders.Where(o => o.CreatedAt >= now.Date.AddDays(-(int)now.DayOfWeek)),
                "month" => orders.Where(o => o.CreatedAt.Year == now.Year && o.CreatedAt.Month == now.Month),
                _ => orders
            };
        }

        private static List<List<string>> BuildPages(List<Product> products, List<Order> allOrders,
            List<Order> periodOrders, List<Order> deliveredOrders, string period, DateTime now)
        {
            var periodLabel = period switch
            {
                "today" => "Today",
                "week" => "This Week",
                "month" => "This Month",
                _ => "All Time"
            };

            var totalProducts = products.Count;
            var totalStock = products.Sum(p => p.Stock);
            var stockValue = products.Sum(p => p.Price * p.Stock);
            var deliveredCount = deliveredOrders.Count;
            var salesTotal = deliveredOrders.Sum(o => o.TotalPrice);
            var lowStockItems = products.Where(p => p.Stock <= 5).ToList();

            var lines = new List<string>();

            lines.Add("RED DICE FLOW");
            lines.Add("Manager Report");
            lines.Add($"Period: {periodLabel}  |  {now:yyyy-MM-dd HH:mm}");
            lines.Add("");

            lines.Add("SUMMARY");
            lines.Add($"  Products:        {totalProducts}");
            lines.Add($"  Total Stock:     {totalStock} units");
            lines.Add($"  Stock Value:     {stockValue.ToString("N2", Money)}$");
            lines.Add($"  Orders (period): {periodOrders.Count}");
            lines.Add($"  Delivered:       {deliveredCount}");
            lines.Add($"  Revenue:         {salesTotal.ToString("N2", Money)}$");
            lines.Add("");

            lines.Add("INVENTORY");
            lines.Add("  Name                          SKU            Genre           Stock  Price");
            

            foreach (var p in products)
            {
                var name = p.Name.Length > 29 ? p.Name[..27] + ".." : p.Name;
                var sku = (p.Sku ?? "").Length > 11 ? (p.Sku ?? "")[..11] : (p.Sku ?? "");
                var genre = (p.Genre ?? "").Length > 14 ? (p.Genre ?? "")[..14] : (p.Genre ?? "");
                lines.Add($"  {name,-29} {sku,-13} {genre,-16} {p.Stock,5} {p.Price,7:N2}$");
            }

            lines.Add("");

            if (periodOrders.Count > 0)
            {
                lines.Add("ORDERS");
                lines.Add("  Order#        Customer                      Total      Status      Date");
                

                foreach (var o in periodOrders.Take(40))
                {
                    var customer = (o.CustomerName ?? o.CustomerPhone ?? "").Length > 27
                        ? (o.CustomerName ?? o.CustomerPhone ?? "")[..25] + ".."
                        : (o.CustomerName ?? o.CustomerPhone ?? "");
                    var status = (o.Status ?? "").Length > 9 ? (o.Status ?? "")[..9] : (o.Status ?? "");
                    var date = o.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                    lines.Add($"  {o.OrderNumber,-12} {customer,-29} {o.TotalPrice,9:N2}$ {status,-11} {date}");
                }

                lines.Add("");
            }

            lines.Add("LOW STOCK");
            if (lowStockItems.Count == 0)
                lines.Add("  No low stock items.");
            else
                foreach (var p in lowStockItems)
                    lines.Add($"  {p.Name,-35} only {p.Stock} left");

            return SplitPages(lines, 55);
        }

        private static List<List<string>> SplitPages(List<string> lines, int maxPerPage)
        {
            var pages = new List<List<string>>();
            for (int i = 0; i < lines.Count; i += maxPerPage)
                pages.Add(lines.GetRange(i, Math.Min(maxPerPage, lines.Count - i)));
            return pages;
        }

        private static void WritePdf(string path, List<List<string>> pages)
        {
            var pageObjs = new List<string>();
            var streamObjs = new List<string>();

            for (var p = 0; p < pages.Count; p++)
            {
                var content = new StringBuilder();
                content.AppendLine("BT");
                content.AppendLine("/F1 18 Tf");
                content.AppendLine("40 800 Td");

                var y = 800;
                var bigFont = true;
                foreach (var line in pages[p])
                {
                    if (line == "RED DICE FLOW")
                    {
                        content.AppendLine("/F2 24 Tf");
                        content.AppendLine($"({Escape(line)}) Tj");
                        content.AppendLine("0 -30 Td");
                        y -= 30;
                        bigFont = true;
                        continue;
                    }

                    if (line == "Manager Report")
                    {
                        content.AppendLine("/F1 14 Tf");
                        content.AppendLine($"({Escape(line)}) Tj");
                        content.AppendLine("0 -22 Td");
                        y -= 22;
                        bigFont = true;
                        continue;
                    }

                    if (line.EndsWith("  |  ") || line == "" || line == "SUMMARY" || line == "INVENTORY" ||
                        line == "ORDERS" || line == "LOW STOCK")
                    {
                        if (line == "SUMMARY" || line == "INVENTORY" || line == "ORDERS" || line == "LOW STOCK")
                        {
                            content.AppendLine("/F2 13 Tf");
                            content.AppendLine($"({Escape(line)}) Tj");
                            content.AppendLine("0 -20 Td");
                            y -= 20;
                        }
                        else
                        {
                            var fontSize = line == "" || line.EndsWith("  |  ") ? 12 : 10;
                            content.AppendLine($"/F1 {fontSize} Tf");
                            if (!string.IsNullOrEmpty(line))
                                content.AppendLine($"({Escape(line)}) Tj");
                            content.AppendLine("0 -16 Td");
                            y -= 16;
                        }
                        bigFont = true;
                        continue;
                    }

                    if (line.StartsWith("  "))
                    {
                        content.AppendLine("/F1 9 Tf");

                        if (line.Contains("-----") || line.Contains("Order#"))
                        {
                            content.AppendLine("/F2 9 Tf");
                            content.AppendLine($"({Escape(line)}) Tj");
                            content.AppendLine("0 -14 Td");
                            y -= 14;
                        }
                        else
                        {
                            content.AppendLine($"({Escape(line)}) Tj");
                            content.AppendLine("0 -14 Td");
                            y -= 14;
                        }
                        bigFont = false;
                        continue;
                    }

                    if (!bigFont)
                    {
                        content.AppendLine("/F1 10 Tf");
                        content.AppendLine($"({Escape(line)}) Tj");
                        content.AppendLine("0 -16 Td");
                        y -= 16;
                    }
                    else
                    {
                        content.AppendLine($"({Escape(line)}) Tj");
                        content.AppendLine("0 -16 Td");
                        y -= 16;
                    }

                    bigFont = false;
                }

                content.AppendLine("ET");
                var stream = content.ToString();
                streamObjs.Add(stream);
                pageObjs.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents {6 + 2 * p} 0 R >>");
            }

            
            var objects = new List<string>
            {
                $"<< /Type /Catalog /Pages 2 0 R >>",
                $"<< /Type /Pages /Kids [{string.Join(" ", Enumerable.Range(3, pages.Count).Select(i => $"{i} 0 R"))}] /Count {pages.Count} >>",
            };
            objects.AddRange(pageObjs);
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");

            foreach (var stream in streamObjs)
                objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}endstream");

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
