using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Core.Patterns.Factories;

public interface IReportBuilder
{
    ReportType SupportedType { get; }
    Report Build(IEnumerable<Transaction> transactions, IEnumerable<Category> categories, DateTime from, DateTime to);
}

public class MonthlyReportBuilder : IReportBuilder
{
    public ReportType SupportedType => ReportType.Monthly;

    public Report Build(IEnumerable<Transaction> transactions, IEnumerable<Category> categories, DateTime from, DateTime to)
    {
        var txList = transactions.Where(t => t.Date >= from && t.Date <= to).ToList();
        var report = new Report($"Місячний звіт: {from:MMMM yyyy}", ReportType.Monthly, from, to)
        {
            TotalIncome = txList.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpense = txList.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
        };

        var grouped = txList.GroupBy(t => t.Date.Day)
            .OrderBy(g => g.Key)
            .Select(g => new ReportEntry
            {
                Label = $"{g.Key:00} {from:MMMM}",
                Amount = g.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount),
                Count = g.Count()
            });

        report.Entries.AddRange(grouped);
        return report;
    }
}

public class CategoryBreakdownReportBuilder : IReportBuilder
{
    public ReportType SupportedType => ReportType.CategoryBreakdown;

    public Report Build(IEnumerable<Transaction> transactions, IEnumerable<Category> categories, DateTime from, DateTime to)
    {
        var txList = transactions.Where(t => t.Date >= from && t.Date <= to && t.Type == TransactionType.Expense).ToList();
        var total = txList.Sum(t => t.Amount);
        var catMap = categories.ToDictionary(c => c.Id);

        var report = new Report($"Розбивка за категоріями: {from:MMM yyyy}", ReportType.CategoryBreakdown, from, to)
        {
            TotalExpense = total,
            TotalIncome = transactions.Where(t => t.Date >= from && t.Date <= to && t.Type == TransactionType.Income).Sum(t => t.Amount)
        };

        var entries = txList
            .GroupBy(t => t.CategoryId)
            .Select(g => new ReportEntry
            {
                Label = catMap.TryGetValue(g.Key, out var cat) ? cat.ToString() : "Невідома",
                Amount = g.Sum(t => t.Amount),
                Percent = total > 0 ? Math.Round(g.Sum(t => t.Amount) / total * 100, 1) : 0,
                Count = g.Count()
            })
            .OrderByDescending(e => e.Amount);

        report.Entries.AddRange(entries);
        return report;
    }
}

public class IncomeVsExpenseReportBuilder : IReportBuilder
{
    public ReportType SupportedType => ReportType.IncomeVsExpense;

    public Report Build(IEnumerable<Transaction> transactions, IEnumerable<Category> categories, DateTime from, DateTime to)
    {
        var txList = transactions.Where(t => t.Date >= from && t.Date <= to).ToList();
        var report = new Report($"Доходи vs Витрати: {from:MMM} — {to:MMM yyyy}", ReportType.IncomeVsExpense, from, to)
        {
            TotalIncome = txList.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpense = txList.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
        };

        report.Entries.Add(new ReportEntry { Label = "Доходи", Amount = report.TotalIncome, Count = txList.Count(t => t.Type == TransactionType.Income) });
        report.Entries.Add(new ReportEntry { Label = "Витрати", Amount = report.TotalExpense, Count = txList.Count(t => t.Type == TransactionType.Expense) });
        report.Entries.Add(new ReportEntry { Label = "Баланс", Amount = report.Balance });

        return report;
    }
}

public class ReportFactory
{
    private readonly Dictionary<ReportType, IReportBuilder> _builders;

    public ReportFactory()
    {
        var builders = new List<IReportBuilder>
        {
            new MonthlyReportBuilder(),
            new CategoryBreakdownReportBuilder(),
            new IncomeVsExpenseReportBuilder()
        };
        _builders = builders.ToDictionary(b => b.SupportedType);
    }

    public Report Create(ReportType type, IEnumerable<Transaction> transactions, IEnumerable<Category> categories, DateTime from, DateTime to)
    {
        if (!_builders.TryGetValue(type, out var builder))
            throw new ArgumentException($"Непідтримуваний тип звіту: {type}");

        return builder.Build(transactions, categories, from, to);
    }

    public IEnumerable<ReportType> GetSupportedTypes() => _builders.Keys;
}
