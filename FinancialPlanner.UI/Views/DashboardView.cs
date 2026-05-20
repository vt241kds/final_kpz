using FinancialPlanner.Core.Models;
using FinancialPlanner.Data.Storage;
using FinancialPlanner.UI.Helpers;

namespace FinancialPlanner.UI.Views;

public class DashboardView
{
    private readonly ServiceContainer _services = ServiceContainer.Instance;

    public async Task ShowAsync()
    {
        ConsoleHelper.PrintHeader("ГОЛОВНА ПАНЕЛЬ");

        var now = DateTime.Now;
        var from = new DateTime(now.Year, now.Month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var income = await _services.TransactionService.GetTotalIncomeAsync(from, to);
        var expense = await _services.TransactionService.GetTotalExpenseAsync(from, to);
        var balance = income - expense;

        ConsoleHelper.PrintInfo($"Поточний місяць: {now:MMMM yyyy}");
        ConsoleHelper.PrintBalance(income, expense, balance);

        ConsoleHelper.PrintSectionTitle("Бюджети поточного місяця");

        var budgetStatuses = (await _services.BudgetService.GetBudgetStatusesAsync(now.Month, now.Year)).ToList();
        if (budgetStatuses.Count == 0)
        {
            ConsoleHelper.PrintInfo("Бюджети ще не встановлені. Перейдіть до розділу 'Бюджети'.");
        }
        else
        {
            foreach (var status in budgetStatuses)
            {
                ConsoleHelper.PrintProgressBar(
                    $"{status.Category.Icon} {status.Category.Name}",
                    status.UsagePercent);

                Console.ForegroundColor = status.IsExceeded ? ConsoleColor.Red : ConsoleColor.DarkGray;
                Console.WriteLine($"    {status.Spent:C} / {status.Budget.Limit:C}  (залишок: {status.Remaining:C})");
                Console.ResetColor();
            }
        }

        ConsoleHelper.PrintSectionTitle("Останні 5 транзакцій");

        var allTransactions = (await _services.TransactionService.GetByMonthAsync(now.Month, now.Year))
            .OrderByDescending(t => t.Date)
            .Take(5)
            .ToList();

        if (allTransactions.Count == 0)
        {
            ConsoleHelper.PrintInfo("У цьому місяці транзакцій ще немає.");
        }
        else
        {
            var categories = (await _services.CategoryService.GetAllAsync()).ToDictionary(c => c.Id);
            ConsoleHelper.PrintTableRow("Назва", "Сума", "Категорія", "Дата");
            ConsoleHelper.PrintSeparator();
            foreach (var t in allTransactions)
            {
                var cat = categories.TryGetValue(t.CategoryId, out var c) ? c.ToString() : "—";
                var amountColor = t.Type == TransactionType.Income ? ConsoleColor.Green : ConsoleColor.Red;
                var sign = t.Type == TransactionType.Income ? "+" : "-";

                Console.Write($"  {t.Title,-30}");
                Console.ForegroundColor = amountColor;
                Console.Write($"{sign}{t.Amount,-19:C}");
                Console.ResetColor();
                Console.Write($"{cat,-15}");
                Console.WriteLine($"{t.Date:dd.MM.yyyy}");
            }
        }

        ConsoleHelper.WaitForKey();
    }
}
