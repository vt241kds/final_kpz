using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Core.Patterns.Observers;

public class BudgetAlertEventArgs : EventArgs
{
    public Budget Budget { get; }
    public Category Category { get; }
    public decimal SpentAmount { get; }
    public bool IsExceeded { get; }

    public BudgetAlertEventArgs(Budget budget, Category category, decimal spent, bool isExceeded)
    {
        Budget = budget;
        Category = category;
        SpentAmount = spent;
        IsExceeded = isExceeded;
    }

    public decimal UsagePercent => Budget.GetUsagePercent(SpentAmount);
}

public interface IBudgetObserver
{
    void OnBudgetAlert(object sender, BudgetAlertEventArgs e);
}

public interface IBudgetSubject
{
    void Subscribe(IBudgetObserver observer);
    void Unsubscribe(IBudgetObserver observer);
    Task NotifyObserversAsync(BudgetAlertEventArgs args);
}
