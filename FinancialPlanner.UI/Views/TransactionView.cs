using FinancialPlanner.Core.Models;
using FinancialPlanner.Core.Patterns.Strategies;
using FinancialPlanner.Data.Storage;
using FinancialPlanner.UI.Helpers;

namespace FinancialPlanner.UI.Views;

public class TransactionView
{
    private readonly ServiceContainer _services = ServiceContainer.Instance;

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("ТРАНЗАКЦІЇ");
            ConsoleHelper.PrintMenuOption(1, "Список транзакцій");
            ConsoleHelper.PrintMenuOption(2, "Додати дохід");
            ConsoleHelper.PrintMenuOption(3, "Додати витрату");
            ConsoleHelper.PrintMenuOption(4, "Редагувати транзакцію");
            ConsoleHelper.PrintMenuOption(5, "Видалити транзакцію");
            ConsoleHelper.PrintMenuOption(6, "Фільтрувати транзакції");
            ConsoleHelper.PrintMenuOption(0, "Назад");

            var choice = ConsoleHelper.AskMenuChoice(0, 6);
            switch (choice)
            {
                case 1: await ShowListAsync(); break;
                case 2: await AddAsync(TransactionType.Income); break;
                case 3: await AddAsync(TransactionType.Expense); break;
                case 4: await EditAsync(); break;
                case 5: await DeleteAsync(); break;
                case 6: await FilterAsync(); break;
                case 0: return;
            }
        }
    }

    private async Task ShowListAsync(IEnumerable<Transaction>? transactions = null)
    {
        ConsoleHelper.PrintHeader("СПИСОК ТРАНЗАКЦІЙ");

        var list = transactions?.ToList() ?? (await _services.TransactionService.GetAllAsync())
            .OrderByDescending(t => t.Date).ToList();

        var categories = (await _services.CategoryService.GetAllAsync()).ToDictionary(c => c.Id);

        if (list.Count == 0)
        {
            ConsoleHelper.PrintInfo("Транзакцій ще немає.");
            ConsoleHelper.WaitForKey();
            return;
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        ConsoleHelper.PrintTableRow("ID (коротко)", "Назва", "Сума", "Дата");
        ConsoleHelper.PrintSeparator();
        Console.ResetColor();

        foreach (var t in list)
        {
            var cat = categories.TryGetValue(t.CategoryId, out var c) ? c.ToString() : "—";
            var amountColor = t.Type == TransactionType.Income ? ConsoleColor.Green : ConsoleColor.Red;
            var sign = t.Type == TransactionType.Income ? "+" : "-";

            Console.Write($"  {t.Id.ToString()[..8],-14}");
            Console.Write($"{t.Title,-22}");
            Console.ForegroundColor = amountColor;
            Console.Write($"{sign}{t.Amount,-12:C}");
            Console.ResetColor();
            Console.Write($"{cat,-18}");
            Console.WriteLine($"{t.Date:dd.MM.yyyy}");
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"\n  Всього: {list.Count} транзакцій");
        Console.ResetColor();

        ConsoleHelper.WaitForKey();
    }

    private async Task AddAsync(TransactionType type)
    {
        var typeName = type == TransactionType.Income ? "ДОХІД" : "ВИТРАТА";
        ConsoleHelper.PrintHeader($"ДОДАТИ {typeName}");

        var categories = (await _services.CategoryService.GetByTypeAsync(type)).ToList();
        if (categories.Count == 0)
        {
            ConsoleHelper.PrintError("Спочатку додайте категорії у розділі 'Категорії'.");
            ConsoleHelper.WaitForKey();
            return;
        }

        var title = ConsoleHelper.AskRequiredInput("Назва");
        var amount = ConsoleHelper.AskDecimal("Сума");
        var note = ConsoleHelper.AskInput("Примітка (необов'язково)") ?? "";

        ConsoleHelper.PrintSectionTitle("Оберіть категорію");
        for (int i = 0; i < categories.Count; i++)
            ConsoleHelper.PrintMenuOption(i + 1, categories[i].ToString());

        var catIndex = ConsoleHelper.AskMenuChoice(1, categories.Count) - 1;
        var categoryId = categories[catIndex].Id;

        try
        {
            var transaction = await _services.TransactionService.AddTransactionAsync(title, amount, type, categoryId, note);
            ConsoleHelper.PrintSuccess($"Транзакцію '{transaction.Title}' успішно додано!");

            if (type == TransactionType.Expense)
            {
                var now = DateTime.Now;
                await _services.BudgetService.CheckAndNotifyBudgetAlertsAsync(categoryId, now.Month, now.Year);
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }

        ConsoleHelper.WaitForKey();
    }

    private async Task EditAsync()
    {
        ConsoleHelper.PrintHeader("РЕДАГУВАТИ ТРАНЗАКЦІЮ");

        var idStr = ConsoleHelper.AskRequiredInput("Введіть початок ID транзакції (перші 8 символів)");
        var all = await _services.TransactionService.GetAllAsync();
        var transaction = all.FirstOrDefault(t => t.Id.ToString().StartsWith(idStr));

        if (transaction == null)
        {
            ConsoleHelper.PrintError("Транзакцію не знайдено.");
            ConsoleHelper.WaitForKey();
            return;
        }

        ConsoleHelper.PrintInfo($"Редагування: {transaction}");

        var title = ConsoleHelper.AskInput($"Нова назва [{transaction.Title}]");
        if (string.IsNullOrWhiteSpace(title)) title = transaction.Title;

        var amountStr = ConsoleHelper.AskInput($"Нова сума [{transaction.Amount}]");
        var amount = decimal.TryParse(amountStr?.Replace(',', '.'), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var a) && a > 0 ? a : transaction.Amount;

        var note = ConsoleHelper.AskInput($"Нова примітка [{transaction.Note}]") ?? transaction.Note;

        try
        {
            await _services.TransactionService.UpdateTransactionAsync(transaction.Id, title, amount, transaction.Type, transaction.CategoryId, note);
            ConsoleHelper.PrintSuccess("Транзакцію успішно оновлено!");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }

        ConsoleHelper.WaitForKey();
    }

    private async Task DeleteAsync()
    {
        ConsoleHelper.PrintHeader("ВИДАЛИТИ ТРАНЗАКЦІЮ");

        var idStr = ConsoleHelper.AskRequiredInput("Введіть початок ID транзакції");
        var all = await _services.TransactionService.GetAllAsync();
        var transaction = all.FirstOrDefault(t => t.Id.ToString().StartsWith(idStr));

        if (transaction == null)
        {
            ConsoleHelper.PrintError("Транзакцію не знайдено.");
            ConsoleHelper.WaitForKey();
            return;
        }

        ConsoleHelper.PrintWarning($"Видалити: {transaction}?");
        if (!ConsoleHelper.AskYesNo("Підтвердити видалення"))
        {
            ConsoleHelper.PrintInfo("Видалення скасовано.");
            ConsoleHelper.WaitForKey();
            return;
        }

        try
        {
            await _services.TransactionService.DeleteTransactionAsync(transaction.Id);
            ConsoleHelper.PrintSuccess("Транзакцію видалено!");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }

        ConsoleHelper.WaitForKey();
    }

    private async Task FilterAsync()
    {
        ConsoleHelper.PrintHeader("ФІЛЬТР ТРАНЗАКЦІЙ");
        ConsoleHelper.PrintMenuOption(1, "За місяцем та роком");
        ConsoleHelper.PrintMenuOption(2, "За типом (дохід/витрата)");
        ConsoleHelper.PrintMenuOption(3, "За категорією");
        ConsoleHelper.PrintMenuOption(4, "За діапазоном сум");
        ConsoleHelper.PrintMenuOption(0, "Назад");

        var choice = ConsoleHelper.AskMenuChoice(0, 4);
        IFilterStrategy? filter = null;

        switch (choice)
        {
            case 1:
                var month = ConsoleHelper.AskInt("Місяць (1-12)", 1, 12);
                var year = ConsoleHelper.AskInt("Рік", 2000, 2100);
                var from = new DateTime(year, month, 1);
                var to = from.AddMonths(1).AddDays(-1);
                filter = new DateRangeFilterStrategy(from, to);
                break;

            case 2:
                ConsoleHelper.PrintMenuOption(1, "Доходи");
                ConsoleHelper.PrintMenuOption(2, "Витрати");
                var typeChoice = ConsoleHelper.AskMenuChoice(1, 2);
                var txType = typeChoice == 1 ? TransactionType.Income : TransactionType.Expense;
                filter = new TypeFilterStrategy(txType);
                break;

            case 3:
                var categories = (await _services.CategoryService.GetAllAsync()).ToList();
                for (int i = 0; i < categories.Count; i++)
                    ConsoleHelper.PrintMenuOption(i + 1, categories[i].ToString());
                var catIdx = ConsoleHelper.AskMenuChoice(1, categories.Count) - 1;
                var cat = categories[catIdx];
                filter = new CategoryFilterStrategy(cat.Id, cat.Name);
                break;

            case 4:
                var min = ConsoleHelper.AskDecimal("Мінімальна сума");
                var max = ConsoleHelper.AskDecimal("Максимальна сума");
                filter = new AmountRangeFilterStrategy(min, max);
                break;

            case 0: return;
        }

        if (filter != null)
        {
            var filtered = await _services.TransactionService.GetFilteredAsync(filter);
            ConsoleHelper.PrintInfo($"Застосовано: {filter.Name}");
            await ShowListAsync(filtered.OrderByDescending(t => t.Date));
        }
    }
}
