using FinancialPlanner.Core.Interfaces.Patterns;
using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Services.Patterns.Observers;
public class ConsoleBudgetObserver : IBudgetObserver
{
    public void OnBudgetExceeded(Budget budget)
    {
        var saved = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n⚠️  УВАГА! Бюджет перевищено!");
        Console.WriteLine($"   Категорія: {budget.Category.Icon} {budget.Category.Name}");
        Console.WriteLine($"   Ліміт: {budget.Limit:F2} UAH");
        Console.WriteLine($"   Витрачено: {budget.SpentAmount:F2} UAH");
        Console.WriteLine($"   Перевищення: {budget.SpentAmount - budget.Limit:F2} UAH\n");
        Console.ForegroundColor = saved;
    }

    public void OnBudgetWarning(Budget budget)
    {
        var saved = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n⚡ Попередження: бюджет використано на {budget.UsagePercent:F0}%");
        Console.WriteLine($"   Категорія: {budget.Category.Icon} {budget.Category.Name}");
        Console.WriteLine($"   Залишок: {budget.RemainingAmount:F2} UAH\n");
        Console.ForegroundColor = saved;
    }
}

public class LogFileBudgetObserver : IBudgetObserver
{
    private readonly string _logPath;

    public LogFileBudgetObserver(string logPath = "budget_alerts.log")
    {
        _logPath = logPath;
    }

    public void OnBudgetExceeded(Budget budget)
    {
        var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] EXCEEDED: {budget.Category.Name} " +
                      $"— {budget.SpentAmount:F2}/{budget.Limit:F2} UAH";
        File.AppendAllText(_logPath, message + Environment.NewLine);
    }

    public void OnBudgetWarning(Budget budget)
    {
        var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WARNING ({budget.UsagePercent:F0}%): " +
                      $"{budget.Category.Name} — {budget.SpentAmount:F2}/{budget.Limit:F2} UAH";
        File.AppendAllText(_logPath, message + Environment.NewLine);
    }
}
