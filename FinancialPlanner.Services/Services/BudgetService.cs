using FinancialPlanner.Core.Enums;
using FinancialPlanner.Core.Interfaces.Patterns;
using FinancialPlanner.Core.Interfaces.Repositories;
using FinancialPlanner.Core.Interfaces.Services;
using FinancialPlanner.Core.Models;


namespace FinancialPlanner.Services.Services;

public class BudgetService : IBudgetService, IBudgetSubject
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

    public void Subscribe(IBudgetObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IBudgetObserver observer) => _observers.Remove(observer);

    public void NotifyBudgetExceeded(Budget budget)
    {
        foreach (var observer in _observers)
            observer.OnBudgetExceeded(budget);
    }

    public void NotifyBudgetWarning(Budget budget)
    {
        foreach (var observer in _observers)
            observer.OnBudgetWarning(budget);
    }

    public async Task<Budget> SetBudgetAsync(int categoryId, decimal limit, int month, int year)
    {
        if (limit <= 0)
            throw new ArgumentException("Ліміт бюджету має бути більше нуля.");

        var category = await _categoryRepository.GetByIdAsync(categoryId)
            ?? throw new ArgumentException($"Категорія з ID {categoryId} не знайдена.");

        var existing = await _budgetRepository.GetByCategoryAndMonthAsync(categoryId, month, year);

        Budget budget;
        if (existing is not null)
        {
            existing.Limit = limit;
            budget = await _budgetRepository.UpdateAsync(existing);
        }
        else
        {
            budget = new Budget
            {
                CategoryId = categoryId,
                Category = category,
                Limit = limit,
                Month = month,
                Year = year,
                CreatedAt = DateTime.UtcNow
            };
            budget = await _budgetRepository.AddAsync(budget);
        }

        await SyncBudgetSpentAsync(budget, month, year);
        CheckAndNotify(budget);

        return budget;
    }

    public async Task DeleteBudgetAsync(int id)
    {
        var exists = await _budgetRepository.ExistsAsync(id);
        if (!exists) throw new KeyNotFoundException($"Бюджет з ID {id} не знайдено.");
        await _budgetRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Budget>> GetMonthlyBudgetsAsync(int month, int year)
    {
        var budgets = (await _budgetRepository.GetByMonthAsync(month, year)).ToList();
        await SyncAllBudgetsSpentAsync(budgets, month, year);
        return budgets;
    }

    public async Task<Budget?> GetBudgetAsync(int categoryId, int month, int year)
    {
        var budget = await _budgetRepository.GetByCategoryAndMonthAsync(categoryId, month, year);
        if (budget is not null)
            await SyncBudgetSpentAsync(budget, month, year);
        return budget;
    }

    public async Task<IEnumerable<Budget>> CheckBudgetStatusAsync(int month, int year)
    {
        var budgets = (await GetMonthlyBudgetsAsync(month, year)).ToList();
        foreach (var budget in budgets)
            CheckAndNotify(budget);
        return budgets;
    }

    public async Task SyncSpentAmountsAsync(int month, int year)
    {
        var budgets = (await _budgetRepository.GetByMonthAsync(month, year)).ToList();
        await SyncAllBudgetsSpentAsync(budgets, month, year);
    }
    private async Task SyncBudgetSpentAsync(Budget budget, int month, int year)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        budget.SpentAmount = await _transactionRepository.GetTotalByTypeAsync(
            TransactionType.Expense, from, to);

        
        var categoryTransactions = await _transactionRepository.GetByCategoryAsync(budget.CategoryId);
        budget.SpentAmount = categoryTransactions
            .Where(t => t.Date.Month == month && t.Date.Year == year && t.IsExpense)
            .Sum(t => t.Amount);

        await _budgetRepository.UpdateAsync(budget);
    }

    private async Task SyncAllBudgetsSpentAsync(List<Budget> budgets, int month, int year)
    {
        foreach (var budget in budgets)
            await SyncBudgetSpentAsync(budget, month, year);
    }
    private void CheckAndNotify(Budget budget)
    {
        if (budget.IsExceeded) NotifyBudgetExceeded(budget);
        else if (budget.IsWarning) NotifyBudgetWarning(budget);
    }
}
