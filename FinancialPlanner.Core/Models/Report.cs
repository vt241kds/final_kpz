namespace FinancialPlanner.Core.Models;

public enum ReportType
{
    Monthly,
    CategoryBreakdown,
    IncomeVsExpense,
    BudgetSummary
}

public class ReportEntry
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percent { get; set; }
    public int Count { get; set; }
}

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance => TotalIncome - TotalExpense;
    public List<ReportEntry> Entries { get; set; } = new();

    public Report() { }

    public Report(string title, ReportType type, DateTime from, DateTime to)
    {
        Title = title;
        Type = type;
        PeriodFrom = from;
        PeriodTo = to;
    }

    public override string ToString() =>
        $"{Title} | {PeriodFrom:MMM yyyy} — Баланс: {Balance:C}";
}
