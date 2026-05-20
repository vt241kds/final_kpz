using FinancialPlanner.Core.Enums;
using FinancialPlanner.Core.Interfaces.Patterns;
using FinancialPlanner.Core.Interfaces.Repositories;
using FinancialPlanner.Core.Interfaces.Services;
using FinancialPlanner.Core.Models;
using FinancialPlanner.Services.Patterns.Strategies;

namespace FinancialPlanner.Services.Services;
public class FilterCriteria : IFilterCriteria
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? CategoryId { get; set; }
    public TransactionType? Type { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Keyword { get; set; }
}

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Transaction> AddTransactionAsync(
        decimal amount, string description, TransactionType type,
        int categoryId, DateTime date, string? note = null)
    {
        ValidateAmount(amount);
        ValidateDescription(description);

        var category = await _categoryRepository.GetByIdAsync(categoryId)
            ?? throw new ArgumentException($"Категорія з ID {categoryId} не знайдена.");

        var transaction = new Transaction
        {
            Amount = amount,
            Description = description.Trim(),
            Type = type,
            CategoryId = categoryId,
            Category = category,
            Date = date,
            Note = note?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _transactionRepository.AddAsync(transaction);
    }

    public async Task<Transaction> UpdateTransactionAsync(
        int id, decimal amount, string description, TransactionType type,
        int categoryId, DateTime date, string? note = null)
    {
        ValidateAmount(amount);
        ValidateDescription(description);

        var existing = await _transactionRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Транзакція з ID {id} не знайдена.");

        var category = await _categoryRepository.GetByIdAsync(categoryId)
            ?? throw new ArgumentException($"Категорія з ID {categoryId} не знайдена.");

        existing.Amount = amount;
        existing.Description = description.Trim();
        existing.Type = type;
        existing.CategoryId = categoryId;
        existing.Category = category;
        existing.Date = date;
        existing.Note = note?.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        return await _transactionRepository.UpdateAsync(existing);
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var exists = await _transactionRepository.ExistsAsync(id);
        if (!exists)
            throw new KeyNotFoundException($"Транзакція з ID {id} не знайдена.");

        await _transactionRepository.DeleteAsync(id);
    }

    public Task<Transaction?> GetTransactionAsync(int id)
        => _transactionRepository.GetByIdAsync(id);

    public Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        => _transactionRepository.GetAllAsync();

    public async Task<IEnumerable<Transaction>> GetFilteredTransactionsAsync(IFilterCriteria criteria)
    {
        var all = await _transactionRepository.GetAllAsync();
        var strategies = BuildStrategies(criteria);
        var composite = new CompositeFilterStrategy(strategies);
        return composite.Apply(all);
    }

    public async Task<decimal> GetBalanceAsync()
    {
        var all = await _transactionRepository.GetAllAsync();
        return all.Sum(t => t.IsIncome ? t.Amount : -t.Amount);
    }

    public async Task<decimal> GetMonthlyBalanceAsync(int month, int year)
    {
        var monthly = await _transactionRepository.GetByMonthAsync(month, year);
        return monthly.Sum(t => t.IsIncome ? t.Amount : -t.Amount);
    }

    public Task<IEnumerable<Transaction>> SearchTransactionsAsync(string keyword)
        => _transactionRepository.SearchAsync(keyword);

    private static IEnumerable<IFilterStrategy> BuildStrategies(IFilterCriteria criteria)
    {
        var strategies = new List<IFilterStrategy>();

        if (criteria.DateFrom.HasValue && criteria.DateTo.HasValue)
            strategies.Add(new DateRangeFilterStrategy(criteria.DateFrom.Value, criteria.DateTo.Value));

        if (criteria.CategoryId.HasValue)
            strategies.Add(new CategoryFilterStrategy(criteria.CategoryId.Value));

        if (criteria.Type.HasValue)
            strategies.Add(new TypeFilterStrategy(criteria.Type.Value));

        if (criteria.MinAmount.HasValue && criteria.MaxAmount.HasValue)
            strategies.Add(new AmountRangeFilterStrategy(criteria.MinAmount.Value, criteria.MaxAmount.Value));

        return strategies;
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Сума транзакції має бути більше нуля.");
        if (amount > 10_000_000)
            throw new ArgumentException("Сума транзакції не може перевищувати 10 000 000 UAH.");
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Опис транзакції не може бути порожнім.");
        if (description.Length > 200)
            throw new ArgumentException("Опис не може перевищувати 200 символів.");
    }
}
