using FinancialPlanner.Core.Patterns.Observers;
using FinancialPlanner.Data.Storage;
using FinancialPlanner.UI.Helpers;
using FinancialPlanner.UI.Views;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var services = ServiceContainer.Instance;

var consoleObserver = new ConsoleBudgetAlertObserver();
var logObserver = new InMemoryAlertLogObserver();
services.BudgetService.Subscribe(consoleObserver);
services.BudgetService.Subscribe(logObserver);

await services.CategoryService.SeedDefaultCategoriesAsync();

var dashboard = new DashboardView();
var transactions = new TransactionView();
var categories = new CategoryView();
var budgets = new BudgetView();
var reports = new ReportView();

while (true)
{
    ConsoleHelper.PrintHeader("ГОЛОВНЕ МЕНЮ");
    ConsoleHelper.PrintMenuOption(1, "  Головна панель (Dashboard)");
    ConsoleHelper.PrintMenuOption(2, "  Транзакції");
    ConsoleHelper.PrintMenuOption(3, "  Категорії");
    ConsoleHelper.PrintMenuOption(4, "  Бюджети");
    ConsoleHelper.PrintMenuOption(5, "  Звіти та аналітика");
    ConsoleHelper.PrintMenuOption(0, "  Вихід");

    if (logObserver.AlertLog.Count > 0)
    {
        Console.WriteLine();
        ConsoleHelper.PrintWarning($"Є {logObserver.AlertLog.Count} бюджетних сповіщень!");
    }
    

    var choice = ConsoleHelper.AskMenuChoice(0, 5);

    switch (choice)
    {
        case 1: await dashboard.ShowAsync(); break;
        case 2: await transactions.ShowAsync(); break;
        case 3: await categories.ShowAsync(); break;
        case 4: await budgets.ShowAsync(); break;
        case 5: await reports.ShowAsync(); break;
        case 0:
            ConsoleHelper.PrintInfo("До побачення!");
            return;
    }
}
