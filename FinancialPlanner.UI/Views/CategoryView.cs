using FinancialPlanner.Core.Models;
using FinancialPlanner.Data.Storage;
using FinancialPlanner.UI.Helpers;

namespace FinancialPlanner.UI.Views;

public class CategoryView
{
    private readonly ServiceContainer _services = ServiceContainer.Instance;

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("КАТЕГОРІЇ");
            ConsoleHelper.PrintMenuOption(1, "Список категорій");
            ConsoleHelper.PrintMenuOption(2, "Додати категорію доходів");
            ConsoleHelper.PrintMenuOption(3, "Додати категорію витрат");
            ConsoleHelper.PrintMenuOption(4, "Редагувати категорію");
            ConsoleHelper.PrintMenuOption(5, "Видалити категорію");
            ConsoleHelper.PrintMenuOption(0, "Назад");

            var choice = ConsoleHelper.AskMenuChoice(0, 5);
            switch (choice)
            {
                case 1: await ShowListAsync(); break;
                case 2: await AddAsync(TransactionType.Income); break;
                case 3: await AddAsync(TransactionType.Expense); break;
                case 4: await EditAsync(); break;
                case 5: await DeleteAsync(); break;
                case 0: return;
            }
        }
    }

    private async Task ShowListAsync()
    {
        ConsoleHelper.PrintHeader("СПИСОК КАТЕГОРІЙ");

        var income = (await _services.CategoryService.GetByTypeAsync(TransactionType.Income)).ToList();
        var expense = (await _services.CategoryService.GetByTypeAsync(TransactionType.Expense)).ToList();

        ConsoleHelper.PrintSectionTitle("Доходи");
        if (income.Count == 0)
        {
            ConsoleHelper.PrintInfo("Немає категорій доходів.");
        }
        else
        {
            foreach (var c in income)
                Console.WriteLine($"  {c.Id.ToString()[..8]}  {c.Icon} {c.Name,-25} {c.ColorHex}");
        }

        ConsoleHelper.PrintSectionTitle("Витрати");
        if (expense.Count == 0)
        {
            ConsoleHelper.PrintInfo("Немає категорій витрат.");
        }
        else
        {
            foreach (var c in expense)
                Console.WriteLine($"  {c.Id.ToString()[..8]}  {c.Icon} {c.Name,-25} {c.ColorHex}");
        }

        ConsoleHelper.WaitForKey();
    }

    private async Task AddAsync(TransactionType type)
    {
        var typeName = type == TransactionType.Income ? "ДОХОДУ" : "ВИТРАТИ";
        ConsoleHelper.PrintHeader($"ДОДАТИ КАТЕГОРІЮ {typeName}");

        var name = ConsoleHelper.AskRequiredInput("Назва категорії");
        var icon = ConsoleHelper.AskInput("Емодзі-іконка (наприклад: 🍕)") ?? "📁";
        var color = ConsoleHelper.AskInput("Колір (HEX, наприклад: #FF5722)") ?? "#607D8B";

        try
        {
            var category = await _services.CategoryService.AddCategoryAsync(name, type, icon, color);
            ConsoleHelper.PrintSuccess($"Категорію '{category}' додано!");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }

        ConsoleHelper.WaitForKey();
    }

    private async Task EditAsync()
    {
        ConsoleHelper.PrintHeader("РЕДАГУВАТИ КАТЕГОРІЮ");

        var idStr = ConsoleHelper.AskRequiredInput("Введіть початок ID категорії");
        var all = await _services.CategoryService.GetAllAsync();
        var category = all.FirstOrDefault(c => c.Id.ToString().StartsWith(idStr));

        if (category == null)
        {
            ConsoleHelper.PrintError("Категорію не знайдено.");
            ConsoleHelper.WaitForKey();
            return;
        }

        ConsoleHelper.PrintInfo($"Редагування: {category}");

        var name = ConsoleHelper.AskInput($"Нова назва [{category.Name}]");
        if (string.IsNullOrWhiteSpace(name)) name = category.Name;

        var icon = ConsoleHelper.AskInput($"Нова іконка [{category.Icon}]");
        if (string.IsNullOrWhiteSpace(icon)) icon = category.Icon;

        var color = ConsoleHelper.AskInput($"Новий колір [{category.ColorHex}]");
        if (string.IsNullOrWhiteSpace(color)) color = category.ColorHex;

        try
        {
            await _services.CategoryService.UpdateCategoryAsync(category.Id, name, icon, color);
            ConsoleHelper.PrintSuccess("Категорію оновлено!");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }

        ConsoleHelper.WaitForKey();
    }

    private async Task DeleteAsync()
    {
        ConsoleHelper.PrintHeader("ВИДАЛИТИ КАТЕГОРІЮ");

        var idStr = ConsoleHelper.AskRequiredInput("Введіть початок ID категорії");
        var all = await _services.CategoryService.GetAllAsync();
        var category = all.FirstOrDefault(c => c.Id.ToString().StartsWith(idStr));

        if (category == null)
        {
            ConsoleHelper.PrintError("Категорію не знайдено.");
            ConsoleHelper.WaitForKey();
            return;
        }

        ConsoleHelper.PrintWarning($"Видалити категорію '{category}'?");
        if (!ConsoleHelper.AskYesNo("Підтвердити"))
        {
            ConsoleHelper.PrintInfo("Видалення скасовано.");
            ConsoleHelper.WaitForKey();
            return;
        }

        try
        {
            await _services.CategoryService.DeleteCategoryAsync(category.Id);
            ConsoleHelper.PrintSuccess("Категорію видалено!");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }

        ConsoleHelper.WaitForKey();
    }
}
