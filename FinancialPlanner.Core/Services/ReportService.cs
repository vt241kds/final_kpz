using FinancialPlanner.Core.Models;
using FinancialPlanner.Core.Patterns.Factories;
using FinancialPlanner.Core.Repositories.Interfaces;

namespace FinancialPlanner.Core.Services;

public class ReportService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ReportFactory _reportFactory;

    public ReportService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _reportFactory = new ReportFactory();
    }

    public async Task<Report> GenerateAsync(ReportType type, DateTime from, DateTime to)
    {
        var transactions = await _transactionRepository.GetAllAsync();
        var categories = await _categoryRepository.GetAllAsync();
        return _reportFactory.Create(type, transactions, categories, from, to);
    }

    public async Task<Report> GenerateMonthlyAsync(int month, int year)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        return await GenerateAsync(ReportType.Monthly, from, to);
    }

    public async Task<string> ExportToCsvAsync(Report report, string filePath)
    {
        var lines = new List<string>
        {
            $"Звіт: {report.Title}",
            $"Згенеровано: {report.GeneratedAt:dd.MM.yyyy HH:mm}",
            $"Період: {report.PeriodFrom:dd.MM.yyyy} — {report.PeriodTo:dd.MM.yyyy}",
            $"Загальні доходи: {report.TotalIncome:F2}",
            $"Загальні витрати: {report.TotalExpense:F2}",
            $"Баланс: {report.Balance:F2}",
            "",
            "Категорія;Сума;Відсоток;Кількість"
        };

        lines.AddRange(report.Entries.Select(e =>
            $"{e.Label};{e.Amount:F2};{e.Percent:F1}%;{e.Count}"));

        await File.WriteAllLinesAsync(filePath, lines, System.Text.Encoding.UTF8);
        return filePath;
    }

    public async Task<string> ExportToJsonAsync(Report report, string filePath)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(filePath, json, System.Text.Encoding.UTF8);
        return filePath;
    }

    public IEnumerable<ReportType> GetAvailableReportTypes() =>
        _reportFactory.GetSupportedTypes();
}
