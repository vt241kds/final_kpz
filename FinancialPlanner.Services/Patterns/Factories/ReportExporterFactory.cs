using System.Text;
using System.Text.Json;
using FinancialPlanner.Core.Enums;
using FinancialPlanner.Core.Interfaces.Patterns;
using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Services.Patterns.Factories;

public class ConsoleReportExporter : IReportExporter
{
    public ReportFormat Format => ReportFormat.Console;

    public Task ExportAsync(Report report, string outputPath)
    {
        var separator = new string('═', 60);
        Console.WriteLine($"\n{separator}");
        Console.WriteLine($"  {report.Title}");
        Console.WriteLine($"  Період: {report.PeriodLabel}");
        Console.WriteLine($"  Сформовано: {report.GeneratedAt:dd.MM.yyyy HH:mm}");
        Console.WriteLine(separator);
        Console.WriteLine($"  💚 Доходи:  {report.TotalIncome,12:F2} UAH");
        Console.WriteLine($"  ❤️  Витрати: {report.TotalExpense,12:F2} UAH");

        var balanceColor = report.Balance >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
        var saved = Console.ForegroundColor;
        Console.ForegroundColor = balanceColor;
        Console.WriteLine($"  📊 Баланс:  {report.Balance,12:F2} UAH");
        Console.ForegroundColor = saved;

        if (report.CategoryBreakdown.Any())
        {
            Console.WriteLine($"\n  {new string('─', 58)}");
            Console.WriteLine("  ВИТРАТИ ЗА КАТЕГОРІЯМИ:");
            Console.WriteLine($"  {new string('─', 58)}");
            foreach (var cat in report.CategoryBreakdown.OrderByDescending(c => c.TotalAmount))
            {
                var bar = BuildProgressBar(cat.Percent, 20);
                Console.WriteLine($"  {cat.CategoryIcon} {cat.CategoryName,-18} {cat.TotalAmount,10:F2} UAH  {bar} {cat.Percent:F0}%");
            }
        }

        Console.WriteLine($"\n{separator}\n");
        return Task.CompletedTask;
    }

    public string GetDefaultFileName(Report report) => string.Empty;

    private static string BuildProgressBar(double percent, int width)
    {
        var filled = (int)(percent / 100 * width);
        return "[" + new string('█', filled) + new string('░', width - filled) + "]";
    }
}

public class CsvReportExporter : IReportExporter
{
    public ReportFormat Format => ReportFormat.Csv;

    public async Task ExportAsync(Report report, string outputPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Дата,Опис,Категорія,Тип,Сума (UAH),Примітка");

        foreach (var t in report.Transactions.OrderByDescending(t => t.Date))
        {
            sb.AppendLine(string.Join(",",
                t.Date.ToString("dd.MM.yyyy"),
                $"\"{t.Description}\"",
                $"\"{t.Category?.Name}\"",
                t.Type == Core.Enums.TransactionType.Income ? "Дохід" : "Витрата",
                t.Amount.ToString("F2"),
                $"\"{t.Note}\""
            ));
        }

        sb.AppendLine();
        sb.AppendLine($"Загальний дохід,{report.TotalIncome:F2}");
        sb.AppendLine($"Загальні витрати,{report.TotalExpense:F2}");
        sb.AppendLine($"Баланс,{report.Balance:F2}");

        await File.WriteAllTextAsync(outputPath, sb.ToString(), Encoding.UTF8);
        Console.WriteLine($"✅ CSV звіт збережено: {outputPath}");
    }

    public string GetDefaultFileName(Report report)
        => $"report_{report.PeriodFrom:yyyyMM}_{report.PeriodTo:yyyyMM}.csv";
}

public class JsonReportExporter : IReportExporter
{
    public ReportFormat Format => ReportFormat.Json;

    public async Task ExportAsync(Report report, string outputPath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(new
        {
            report.Title,
            GeneratedAt = report.GeneratedAt.ToString("o"),
            PeriodFrom = report.PeriodFrom.ToString("yyyy-MM-dd"),
            PeriodTo = report.PeriodTo.ToString("yyyy-MM-dd"),
            report.TotalIncome,
            report.TotalExpense,
            Balance = report.Balance,
            CategoryBreakdown = report.CategoryBreakdown.Select(c => new
            {
                c.CategoryName,
                c.CategoryIcon,
                c.TotalAmount,
                c.TransactionCount,
                Percent = Math.Round(c.Percent, 2)
            }),
            Transactions = report.Transactions.Select(t => new
            {
                t.Id,
                Date = t.Date.ToString("yyyy-MM-dd"),
                t.Description,
                Category = t.Category?.Name,
                Type = t.Type.ToString(),
                t.Amount,
                t.Note
            })
        }, options);

        await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8);
        Console.WriteLine($"✅ JSON звіт збережено: {outputPath}");
    }

    public string GetDefaultFileName(Report report)
        => $"report_{report.PeriodFrom:yyyyMM}_{report.PeriodTo:yyyyMM}.json";
}

public class ReportExporterFactory : IReportExporterFactory
{
    private readonly Dictionary<ReportFormat, IReportExporter> _exporters;

    public ReportExporterFactory()
    {
        _exporters = new Dictionary<ReportFormat, IReportExporter>
        {
            { ReportFormat.Console, new ConsoleReportExporter() },
            { ReportFormat.Csv,     new CsvReportExporter() },
            { ReportFormat.Json,    new JsonReportExporter() },
        };
    }

    public IReportExporter CreateExporter(ReportFormat format)
    {
        if (!_exporters.TryGetValue(format, out var exporter))
            throw new ArgumentException($"Непідтримуваний формат: {format}");
        return exporter;
    }

    public IEnumerable<ReportFormat> GetSupportedFormats() => _exporters.Keys;
}
