using FinancialPlanner.Core.Models;
using FinancialPlanner.Data.Storage;
using FinancialPlanner.UI.Helpers;

namespace FinancialPlanner.UI.Views;

public class ReportView
{
    private readonly ServiceContainer _services = ServiceContainer.Instance;

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("ЗВІТИ ТА АНАЛІТИКА");
            ConsoleHelper.PrintMenuOption(1, "Місячний звіт");
            ConsoleHelper.PrintMenuOption(2, "Розбивка за категоріями");
            ConsoleHelper.PrintMenuOption(3, "Доходи vs Витрати");
            ConsoleHelper.PrintMenuOption(4, "Експорт звіту (CSV)");
            ConsoleHelper.PrintMenuOption(5, "Експорт звіту (JSON)");
            ConsoleHelper.PrintMenuOption(0, "Назад");

            var choice = ConsoleHelper.AskMenuChoice(0, 5);
            switch (choice)
            {
                case 1: await ShowReportAsync(ReportType.Monthly); break;
                case 2: await ShowReportAsync(ReportType.CategoryBreakdown); break;
                case 3: await ShowReportAsync(ReportType.IncomeVsExpense); break;
                case 4: await ExportAsync(false); break;
                case 5: await ExportAsync(true); break;
                case 0: return;
            }
        }
    }

    private async Task<(Report report, DateTime from, DateTime to)> AskPeriodAndGenerateAsync(ReportType type)
    {
        var now = DateTime.Now;
        var monthStr = ConsoleHelper.AskInput($"Місяць [{now.Month}]");
        var month = int.TryParse(monthStr, out var m) && m >= 1 && m <= 12 ? m : now.Month;
        var yearStr = ConsoleHelper.AskInput($"Рік [{now.Year}]");
        var year = int.TryParse(yearStr, out var y) && y >= 2000 ? y : now.Year;

        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        var report = await _services.ReportService.GenerateAsync(type, from, to);
        return (report, from, to);
    }

    private async Task ShowReportAsync(ReportType type)
    {
        ConsoleHelper.PrintHeader("ПАРАМЕТРИ ЗВІТУ");
        var (report, _, _) = await AskPeriodAndGenerateAsync(type);

        ConsoleHelper.PrintHeader(report.Title.ToUpper());
        ConsoleHelper.PrintInfo($"Згенеровано: {report.GeneratedAt:dd.MM.yyyy HH:mm}");
        ConsoleHelper.PrintInfo($"Період: {report.PeriodFrom:dd.MM.yyyy} — {report.PeriodTo:dd.MM.yyyy}");

        ConsoleHelper.PrintBalance(report.TotalIncome, report.TotalExpense, report.Balance);

        if (report.Entries.Count > 0)
        {
            ConsoleHelper.PrintSectionTitle("Деталізація");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  {"Категорія",-30} {"Сума",12} {"Відсоток",10} {"Кількість",10}");
            ConsoleHelper.PrintSeparator();
            Console.ResetColor();

            foreach (var entry in report.Entries)
            {
                Console.WriteLine($"  {entry.Label,-30} {entry.Amount,12:C} {entry.Percent,9:F1}% {entry.Count,10}");
            }
        }
        else
        {
            ConsoleHelper.PrintInfo("Даних за вказаний період немає.");
        }

        ConsoleHelper.WaitForKey();
    }

    private async Task ExportAsync(bool asJson)
    {
        ConsoleHelper.PrintHeader($"ЕКСПОРТ У {(asJson ? "JSON" : "CSV")}");
        ConsoleHelper.PrintMenuOption(1, "Місячний звіт");
        ConsoleHelper.PrintMenuOption(2, "Розбивка за категоріями");
        ConsoleHelper.PrintMenuOption(3, "Доходи vs Витрати");

        var typeChoice = ConsoleHelper.AskMenuChoice(1, 3);
        var reportType = typeChoice switch
        {
            1 => ReportType.Monthly,
            2 => ReportType.CategoryBreakdown,
            _ => ReportType.IncomeVsExpense
        };

        var (report, _, _) = await AskPeriodAndGenerateAsync(reportType);

        var ext = asJson ? "json" : "csv";
        var defaultName = $"report_{report.PeriodFrom:yyyy_MM}.{ext}";
        var fileName = ConsoleHelper.AskInput($"Ім'я файлу [{defaultName}]");
        if (string.IsNullOrWhiteSpace(fileName)) fileName = defaultName;

        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, fileName);

        try
        {
            if (asJson)
                await _services.ReportService.ExportToJsonAsync(report, filePath);
            else
                await _services.ReportService.ExportToCsvAsync(report, filePath);

            ConsoleHelper.PrintSuccess($"Звіт збережено: {filePath}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError($"Помилка при збереженні: {ex.Message}");
        }

        ConsoleHelper.WaitForKey();
    }
}
