using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Core.Patterns.Observers;

public class ConsoleBudgetAlertObserver : IBudgetObserver
{
    public void OnBudgetAlert(object sender, BudgetAlertEventArgs e)
    {
        var status = e.IsExceeded ? "ПЕРЕВИЩЕНО" : "ПОПЕРЕДЖЕННЯ";
        Console.ForegroundColor = e.IsExceeded ? ConsoleColor.Red : ConsoleColor.Yellow;
        Console.WriteLine($"\n⚠ [{status}] Бюджет: {e.Category} — використано {e.UsagePercent}% ({e.SpentAmount:C} з {e.Budget.Limit:C})");
        Console.ResetColor();
    }
}

public class InMemoryAlertLogObserver : IBudgetObserver
{
    private readonly List<string> _alertLog = new();

    public IReadOnlyList<string> AlertLog => _alertLog.AsReadOnly();

    public void OnBudgetAlert(object sender, BudgetAlertEventArgs e)
    {
        var status = e.IsExceeded ? "ПЕРЕВИЩЕНО" : "ПОПЕРЕДЖЕННЯ";
        var message = $"[{DateTime.Now:HH:mm:ss}] {status}: {e.Category.Name} — {e.UsagePercent}% ({e.SpentAmount:C} / {e.Budget.Limit:C})";
        _alertLog.Add(message);
    }

    public void ClearLog() => _alertLog.Clear();
}
