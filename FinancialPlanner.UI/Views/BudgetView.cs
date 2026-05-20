using FinancialPlanner.Core.Models;
using FinancialPlanner.Data.Storage;
using FinancialPlanner.UI.Helpers;

namespace FinancialPlanner.UI.Views;

public class BudgetView
{
    private readonly ServiceContainer _services = ServiceContainer.Instance;

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("БЮДЖЕТИ");
            ConsoleHelper.PrintMenuOption(1, "Статус бюджетів поточного місяця");
            ConsoleHelper.PrintMenuOption(2, "Встановити/оновити бюджет");
            ConsoleHelper.PrintMenuOption(3, "Переглянути бюджети за місяцем");
            ConsoleHelper.PrintMenuOption(4, "Видалити бюджет");
            ConsoleHelper.PrintMenuOption(0, "Назад");

            var choice = ConsoleHelper.AskMenuChoice(0, 4);
            switch (choice)
            {
                case 1: await ShowCurrentStatusAsync(); break;
                case 2: await SetBudgetAsync(); break;
                case 3: await ShowByMonthAsync(); break;
                case 4: await DeleteBudgetAsync(); break;
                case 0: return;
            }
        }
    }

    private async Task ShowCurrentStatusAsync()
    {
        var now = DateTime.Now;
        await ShowStatusForMonthAsync(now.Month, now.Year);
    }

    private async Task ShowStatusForMonthAsync(int month, int year)
    {
        ConsoleHelper.PrintHeader($"БЮДЖЕТИ: {new DateTime(year, month, 1):MMMM yyyy}".ToUpper());

        var statuses = (await _services.BudgetService.GetBudgetStatusesAsync(month, year)).ToList();

        if (statuses.Count == 0)
        {
            ConsoleHelper.PrintInfo("Для цього місяця бюджетів не встановлено.");
            ConsoleHelper.WaitForKey();
            return;
        }

        decimal totalLimit = 0, totalSpent = 0;

        foreach (var status in statuses)
        {
            Console.WriteLine();
            Console.Write($"  {status.Category.Icon} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(status.Category.Name);
            Console.ResetColor();

            ConsoleHelper.PrintProgressBar($"  Використано", status.UsagePercent);

            Console.ForegroundColor = status.IsExceeded ? ConsoleColor.Red : ConsoleColor.DarkGray;
            Console.WriteLine($"    Витрачено: {status.Spent:C}  |  Ліміт: {status.Budget.Limit:C}  |  Залишок: {status.Remaining:C}");
            Console.ResetColor();

            if (status.IsExceeded)
                ConsoleHelper.PrintError($"  Бюджет ПЕРЕВИЩЕНО на {Math.Abs(status.Remaining):C}!");
            else if (status.IsAlertActive)
                ConsoleHelper.PrintWarning($"  Майже вичерпано ({status.Budget.AlertThresholdPercent}% поріг)");

            totalLimit += status.Budget.Limit;
            totalSpent += status.Spent;
        }

        ConsoleHelper.PrintSectionTitle("Підсумок");
        Console.WriteLine($"  Загальний ліміт: {totalLimit:C}");
        Console.WriteLine($"  Витрачено всього: {totalSpent:C}");
        Console.WriteLine($"  Залишок: {(totalLimit - totalSpent):C}");

        ConsoleHelper.WaitForKey();
    }

    private async Task SetBudgetAsync()
    {
        ConsoleHelper.PrintHeader("ВСТАНОВИТИ БЮДЖЕТ");

        var expenseCategories = (await _services.CategoryService.GetByTypeAsync(TransactionType.Expense)).ToList();
        if (expenseCategories.Count == 0)
        {
            ConsoleHelper.PrintError("Спочатку додайте категорії витрат.");
            ConsoleHelper.WaitForKey();
            return;
        }

        ConsoleHelper.PrintSectionTitle("Оберіть категорію");
        for (int i = 0; i < expenseCategories.Count; i++)
            ConsoleHelper.PrintMenuOption(i + 1, expenseCategories[i].ToString());

        var catIdx = ConsoleHelper.AskMenuChoice(1, expenseCategories.Count) - 1;
        var category = expenseCategories[catIdx];

        var limit = ConsoleHelper.AskDecimal("Місячний ліміт");

        var now = DateTime.Now;
        var monthStr = ConsoleHelper.AskInput($"Місяць [{now.Month}]");
        var month = int.TryParse(monthStr, out var m) && m >= 1 && m <= 12 ? m : now.Month;

        var yearStr = ConsoleHelper.AskInput($"Рік [{now.Year}]");
        var year = int.TryParse(yearStr, out var y) && y >= 2000 ? y : now.Year;

        var thresholdStr = ConsoleHelper.AskInput("Поріг сповіщення у % [80]");
        var threshold = decimal.TryParse(thresholdStr, out var t) && t > 0 && t <= 100 ? t : 80m;

        try
        {
            await _services.BudgetService.SetBudgetAsync(category.Id, limit, month, year, threshold);
            ConsoleHelper.PrintSuccess($"Бюджет для '{category}' на {month}/{year}: {limit:C} встановлено!");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }

        ConsoleHelper.WaitForKey();
    }

    private async Task ShowByMonthAsync()
    {
        var month = ConsoleHelper.AskInt("Місяць (1-12)", 1, 12);
        var year = ConsoleHelper.AskInt("Рік", 2000, 2100);
        await ShowStatusForMonthAsync(month, year);
    }

    private async Task DeleteBudgetAsync()
    {
        ConsoleHelper.PrintHeader("ВИДАЛИТИ БЮДЖЕТ");

        var now = DateTime.Now;
        var budgets = (await _services.BudgetService.GetByMonthAsync(now.Month, now.Year)).ToList();
        var categories = (await _services.CategoryService.GetAllAsync()).ToDictionary(c => c.Id);

        if (budgets.Count == 0)
        {
            ConsoleHelper.PrintInfo("Бюджетів для поточного місяця немає.");
            ConsoleHelper.WaitForKey();
            return;
        }

        for (int i = 0; i < budgets.Count; i++)
        {
            var catName = categories.TryGetValue(budgets[i].CategoryId, out var cat) ? cat.ToString() : "—";
            ConsoleHelper.PrintMenuOption(i + 1, $"{catName} — {budgets[i].Limit:C}");
        }

        var idx = ConsoleHelper.AskMenuChoice(1, budgets.Count) - 1;
        var budget = budgets[idx];

        if (!ConsoleHelper.AskYesNo("Видалити бюджет?"))
        {
            ConsoleHelper.PrintInfo("Скасовано.");
            ConsoleHelper.WaitForKey();
            return;
        }

        try
        {
            await _services.BudgetService.DeleteBudgetAsync(budget.Id);
            ConsoleHelper.PrintSuccess("Бюджет видалено!");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }

        ConsoleHelper.WaitForKey();
    }
}
