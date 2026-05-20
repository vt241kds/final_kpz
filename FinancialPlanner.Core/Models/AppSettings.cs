namespace FinancialPlanner.Core.Models;

public class AppSettings
{
    public string Currency { get; set; } = "UAH";
    public string CurrencySymbol { get; set; } = "₴";
    public string DateFormat { get; set; } = "dd.MM.yyyy";
    public bool EnableBudgetAlerts { get; set; } = true;
    public string DataDirectory { get; set; } = "data";
}
