using FinancialPlanner.Core.Exceptions;
using FinancialPlanner.Core.Models;
using FinancialPlanner.Core.Patterns.Strategies;
using FinancialPlanner.Core.Repositories.Interfaces;

namespace FinancialPlanner.Core.Services;

public class TransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Transaction> AddTransactionAsync(string title, decimal amount, TransactionType type, Guid categoryId, string note = "")
    {
        await ValidateTransactionAsync(title, amount, categoryId);

        var transaction = new Transaction(title, amount, type, categoryId, note);
        await _transactionRepository.AddAsync(transaction);
        return transaction;
    }

    public async Task UpdateTransactionAsync(Guid id, string title, decimal amount, TransactionType type, Guid categoryId, string note = "")
    {
        var existing = await _transactionRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(id);

        await ValidateTransactionAsync(title, amount, categoryId);

        existing.Title = title;
        existing.Amount = amount;
        existing.Type = type;
        existing.CategoryId = categoryId;
        existing.Note = note;

        await _transactionRepository.UpdateAsync(existing);
    }

    public async Task DeleteTransactionAsync(Guid id)
    {
        if (!await _transactionRepository.ExistsAsync(id))
            throw new EntityNotFoundException(id);

        await _transactionRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync() =>
        await _transactionRepository.GetAllAsync();

    public async Task<IEnumerable<Transaction>> GetFilteredAsync(IFilterStrategy filter)
    {
        var all = await _transactionRepository.GetAllAsync();
        return filter.Apply(all);
    }

    public async Task<IEnumerable<Transaction>> GetFilteredAndSortedAsync(IFilterStrategy filter, ISortStrategy sort)
    {
        var filtered = await GetFilteredAsync(filter);
        return sort.Apply(filtered);
    }

    public async Task<decimal> GetTotalIncomeAsync(DateTime from, DateTime to) =>
        await _transactionRepository.GetTotalByTypeAsync(TransactionType.Income, from, to);

    public async Task<decimal> GetTotalExpenseAsync(DateTime from, DateTime to) =>
        await _transactionRepository.GetTotalByTypeAsync(TransactionType.Expense, from, to);

    public async Task<decimal> GetBalanceAsync(DateTime from, DateTime to)
    {
        var income = await GetTotalIncomeAsync(from, to);
        var expense = await GetTotalExpenseAsync(from, to);
        return income - expense;
    }

    public async Task<Dictionary<Guid, decimal>> GetExpensesByCategoryAsync(DateTime from, DateTime to)
    {
        var transactions = await _transactionRepository.GetByDateRangeAsync(from, to);
        return transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
    }

    public async Task<IEnumerable<Transaction>> GetByMonthAsync(int month, int year) =>
        await _transactionRepository.GetByMonthAsync(month, year);

    private async Task ValidateTransactionAsync(string title, decimal amount, Guid categoryId)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(title))
            errors.Add("Назва транзакції не може бути порожньою.");

        if (amount <= 0)
            errors.Add("Сума транзакції має бути більше нуля.");

        if (!await _categoryRepository.ExistsAsync(categoryId))
            errors.Add($"Категорія з ID {categoryId} не існує.");

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}
