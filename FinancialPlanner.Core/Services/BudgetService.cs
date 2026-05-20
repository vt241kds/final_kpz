using FinancialPlanner.Core.Exceptions;
using FinancialPlanner.Core.Models;
using FinancialPlanner.Core.Patterns.Observers;
using FinancialPlanner.Core.Repositories.Interfaces;

namespace FinancialPlanner.Core.Services;

public class BudgetService : IBudgetSubject
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly List<IBudgetObserver> _observers = new();

    public BudgetService(
        IBudgetRepository budgetRepository,
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
    }

    public void Subscribe(IBudgetObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void Unsubscribe(IBudgetObserver observer) =>
        _observers.Remove(observer);

    public async Task NotifyObserversAsync(BudgetAlertEventArgs args)
    {
        foreach (var observer in _observers)
            observer.OnBudgetAlert(this, args);

        await Task.CompletedTask;
    }

    public async Task<Budget> SetBudgetAsync(Guid categoryId, decimal limit, int month, int year, decimal alertThreshold = 80m)
    {
        ValidateBudget(limit, alertThreshold);

        if (!await _categoryRepository.ExistsAsync(categoryId))
            throw new EntityNotFoundException(categoryId);

        var existing = await _budgetRepository.GetByCategoryAndMonthAsync(categoryId, month, year);
        if (existing != null)
        {
            existing.Limit = limit;
            existing.AlertThresholdPercent = alertThreshold;
            await _budgetRepository.UpdateAsync(existing);
            return existing;
        }

        var budget = new Budget(categoryId, limit, month, year, alertThreshold);
        await _budgetRepository.AddAsync(budget);
        return budget;
    }

    public async Task DeleteBudgetAsync(Guid id)
    {
        if (!await _budgetRepository.ExistsAsync(id))
            throw new EntityNotFoundException(id);

        await _budgetRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Budget>> GetByMonthAsync(int month, int year) =>
        await _budgetRepository.GetByMonthAsync(month, year);

    public async Task<IEnumerable<BudgetStatus>> GetBudgetStatusesAsync(int month, int year)
    {
        var budgets = await _budgetRepository.GetByMonthAsync(month, year);
        var statuses = new List<BudgetStatus>();

        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        foreach (var budget in budgets)
        {
            var spent = await _transactionRepository.GetTotalByCategoryAsync(budget.CategoryId, from, to);
            var category = await _categoryRepository.GetByIdAsync(budget.CategoryId);
            if (category == null) continue;

            statuses.Add(new BudgetStatus(budget, category, spent));
        }

        return statuses;
    }

    public async Task CheckAndNotifyBudgetAlertsAsync(Guid categoryId, int month, int year)
    {
        var budget = await _budgetRepository.GetByCategoryAndMonthAsync(categoryId, month, year);
        if (budget == null) return;

        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null) return;

        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        var spent = await _transactionRepository.GetTotalByCategoryAsync(categoryId, from, to);

        var isExceeded = budget.IsExceeded(spent);
        var isAlert = budget.IsAlertThresholdReached(spent);

        if (isExceeded || isAlert)
        {
            var args = new BudgetAlertEventArgs(budget, category, spent, isExceeded);
            await NotifyObserversAsync(args);
        }
    }

    private static void ValidateBudget(decimal limit, decimal threshold)
    {
        var errors = new List<string>();

        if (limit <= 0)
            errors.Add("Ліміт бюджету має бути більше нуля.");

        if (threshold < 1 || threshold > 100)
            errors.Add("Поріг сповіщення має бути від 1 до 100.");

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}

public class BudgetStatus
{
    public Budget Budget { get; }
    public Category Category { get; }
    public decimal Spent { get; }
    public decimal Remaining => Budget.GetRemainingAmount(Spent);
    public decimal UsagePercent => Budget.GetUsagePercent(Spent);
    public bool IsExceeded => Budget.IsExceeded(Spent);
    public bool IsAlertActive => Budget.IsAlertThresholdReached(Spent);

    public BudgetStatus(Budget budget, Category category, decimal spent)
    {
        Budget = budget;
        Category = category;
        Spent = spent;
    }
}
