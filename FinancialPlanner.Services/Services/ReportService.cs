using FinancialPlanner.Core.Enums;
using FinancialPlanner.Core.Interfaces.Patterns;
using FinancialPlanner.Core.Interfaces.Repositories;
using FinancialPlanner.Core.Interfaces.Services;
using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Services.Services;

public class ReportService : IReportService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IReportExporterFactory _exporterFactory;

    public ReportService(
        ITransactionRepository transactionRepository,
        IReportExporterFactory exporterFactory)
    {
        _transactionRepository = transactionRepository;
        _exporterFactory = exporterFactory;
    }

    public async Task<Report> GenerateMonthlyReportAsync(int month, int year)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        return await GeneratePeriodReportAsync(from, to);
    }

    public async Task<Report> GeneratePeriodReportAsync(DateTime from, DateTime to)
    {
        var transactions = (await _transactionRepository.GetByDateRangeAsync(from, to)).ToList();

        var totalIncome  = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.IsExpense).Sum(t => t.Amount);

        var breakdown = BuildCategoryBreakdown(transactions, totalExpense);

        return new Report
        {
            Title = $"Фінансовий звіт",
            GeneratedAt = DateTime.UtcNow,
            PeriodFrom = from,
            PeriodTo = to,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            CategoryBreakdown = breakdown,
            Transactions = transactions
        };
    }

    public async Task ExportReportAsync(Report report, ReportFormat format, string outputPath)
    {
        var exporter = _exporterFactory.CreateExporter(format);
        await exporter.ExportAsync(report, outputPath);
    }

    private static List<CategorySummary> BuildCategoryBreakdown(
        List<Transaction> transactions, decimal totalExpense)
    {
        return transactions
            .Where(t => t.IsExpense)
            .GroupBy(t => t.CategoryId)
            .Select(g => new CategorySummary
            {
                CategoryName     = g.First().Category?.Name ?? "Невідомо",
                CategoryIcon     = g.First().Category?.Icon ?? "📦",
                TotalAmount      = g.Sum(t => t.Amount),
                TransactionCount = g.Count(),
                Percent          = totalExpense == 0 ? 0
                                   : (double)(g.Sum(t => t.Amount) / totalExpense) * 100
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();
    }
}
