namespace FinancialPlanner.UI.Helpers;

public static class ConsoleHelper
{
    private const int TableWidth = 80;

    public static void PrintHeader(string title)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', TableWidth));
        Console.WriteLine($"  💰  ФІНАНСОВИЙ ПЛАНУВАЛЬНИК  |  {title}");
        Console.WriteLine(new string('═', TableWidth));
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void PrintSectionTitle(string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  ▶  {title}");
        Console.WriteLine(new string('─', TableWidth));
        Console.ResetColor();
    }

    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓  {message}");
        Console.ResetColor();
    }

    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗  {message}");
        Console.ResetColor();
    }

    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠  {message}");
        Console.ResetColor();
    }

    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  ℹ  {message}");
        Console.ResetColor();
    }

    public static void PrintMenuOption(int number, string label)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"  [{number}] ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(label);
        Console.ResetColor();
    }

    public static void PrintSeparator() =>
        Console.WriteLine(new string('─', TableWidth));

    public static void PrintTableRow(string col1, string col2, string col3 = "", string col4 = "")
    {
        Console.Write($"  {col1,-30}");
        Console.Write($"{col2,-20}");
        if (!string.IsNullOrEmpty(col3)) Console.Write($"{col3,-15}");
        if (!string.IsNullOrEmpty(col4)) Console.Write($"{col4}");
        Console.WriteLine();
    }

    public static void PrintBalance(decimal income, decimal expense, decimal balance)
    {
        PrintSectionTitle("Баланс");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Доходи:  {income,15:C}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Витрати: {expense,15:C}");
        Console.ForegroundColor = balance >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"  Баланс:  {balance,15:C}");
        Console.ResetColor();
    }

    public static void PrintProgressBar(string label, decimal percent, int width = 30)
    {
        var filled = (int)(percent / 100 * width);
        filled = Math.Clamp(filled, 0, width);

        Console.ForegroundColor = percent >= 100 ? ConsoleColor.Red :
                                  percent >= 80 ? ConsoleColor.Yellow :
                                  ConsoleColor.Green;

        var bar = new string('█', filled) + new string('░', width - filled);
        Console.WriteLine($"  {label,-25} [{bar}] {percent:F0}%");
        Console.ResetColor();
    }

    public static string? AskInput(string prompt)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"\n  {prompt}: ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        var input = Console.ReadLine()?.Trim();
        Console.ResetColor();
        return input;
    }

    public static string AskRequiredInput(string prompt)
    {
        while (true)
        {
            var input = AskInput(prompt);
            if (!string.IsNullOrWhiteSpace(input)) return input;
            PrintError("Це поле не може бути порожнім.");
        }
    }

    public static decimal AskDecimal(string prompt)
    {
        while (true)
        {
            var input = AskInput(prompt);
            if (decimal.TryParse(input?.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var result) && result > 0)
                return result;
            PrintError("Введіть коректне число більше нуля.");
        }
    }

    public static int AskInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            var input = AskInput(prompt);
            if (int.TryParse(input, out var result) && result >= min && result <= max)
                return result;
            PrintError($"Введіть ціле число від {min} до {max}.");
        }
    }

    public static bool AskYesNo(string prompt)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"\n  {prompt} (т/н): ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        var input = Console.ReadLine()?.Trim().ToLower();
        Console.ResetColor();
        return input == "т" || input == "y" || input == "yes" || input == "так";
    }

    public static int AskMenuChoice(int min, int max)
    {
        Console.WriteLine();
        return AskInt("Ваш вибір", min, max);
    }

    public static void WaitForKey(string message = "Натисніть Enter для продовження...")
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"\n  {message}");
        Console.ResetColor();
        Console.ReadLine();
    }
}
