using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Core.Patterns.Strategies;

public interface IFilterStrategy
{
    string Name { get; }
    IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions);
}

public class DateRangeFilterStrategy : IFilterStrategy
{
    private readonly DateTime _from;
    private readonly DateTime _to;

    public string Name => $"Фільтр за датою: {_from:dd.MM.yyyy} — {_to:dd.MM.yyyy}";

    public DateRangeFilterStrategy(DateTime from, DateTime to)
    {
        _from = from;
        _to = to;
    }

    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions) =>
        transactions.Where(t => t.Date >= _from && t.Date <= _to);
}

public class CategoryFilterStrategy : IFilterStrategy
{
    private readonly Guid _categoryId;
    private readonly string _categoryName;

    public string Name => $"Фільтр за категорією: {_categoryName}";

    public CategoryFilterStrategy(Guid categoryId, string categoryName)
    {
        _categoryId = categoryId;
        _categoryName = categoryName;
    }

    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions) =>
        transactions.Where(t => t.CategoryId == _categoryId);
}

public class TypeFilterStrategy : IFilterStrategy
{
    private readonly TransactionType _type;

    public string Name => $"Фільтр за типом: {_type}";

    public TypeFilterStrategy(TransactionType type) => _type = type;

    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions) =>
        transactions.Where(t => t.Type == _type);
}

public class AmountRangeFilterStrategy : IFilterStrategy
{
    private readonly decimal _min;
    private readonly decimal _max;

    public string Name => $"Фільтр за сумою: {_min:C} — {_max:C}";

    public AmountRangeFilterStrategy(decimal min, decimal max)
    {
        _min = min;
        _max = max;
    }

    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions) =>
        transactions.Where(t => t.Amount >= _min && t.Amount <= _max);
}

public class CompositeFilterStrategy : IFilterStrategy
{
    private readonly List<IFilterStrategy> _strategies = new();

    public string Name => "Комбінований фільтр: " + string.Join(", ", _strategies.Select(s => s.Name));

    public CompositeFilterStrategy AddFilter(IFilterStrategy strategy)
    {
        _strategies.Add(strategy);
        return this;
    }

    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions) =>
        _strategies.Aggregate(transactions, (current, strategy) => strategy.Apply(current));
}
