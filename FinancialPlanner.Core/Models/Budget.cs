namespace FinancialPlanner.Core.Models;

public class Budget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoryId { get; set; }
    public decimal Limit { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal AlertThresholdPercent { get; set; } = 80m;

    public Budget() { }

    public Budget(Guid categoryId, decimal limit, int month, int year, decimal alertThresholdPercent = 80m)
    {
        CategoryId = categoryId;
        Limit = limit;
        Month = month;
        Year = year;
        AlertThresholdPercent = alertThresholdPercent;
    }

    public bool IsAlertThresholdReached(decimal spent) =>
        Limit > 0 && (spent / Limit * 100) >= AlertThresholdPercent;

    public bool IsExceeded(decimal spent) =>
        spent > Limit;

    public decimal GetRemainingAmount(decimal spent) =>
        Limit - spent;

    public decimal GetUsagePercent(decimal spent) =>
        Limit > 0 ? Math.Round(spent / Limit * 100, 1) : 0;
}
