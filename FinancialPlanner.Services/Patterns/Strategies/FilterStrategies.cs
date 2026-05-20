using FinancialPlanner.Core.Enums;
using FinancialPlanner.Core.Interfaces.Patterns;
using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Services.Patterns.Strategies;

public class DateRangeFilterStrategy : IFilterStrategy
{
    private readonly DateTime _from;
    private readonly DateTime _to;

    public string Name => "Фільтр за датою";

    public DateRangeFilterStrategy(DateTime from, DateTime to)
    {
        _from = from;
        _to = to;
    }

    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions)
        => transactions.Where(t => t.Date >= _from && t.Date <= _to);
}

public class CategoryFilterStrategy : IFilterStrategy
{
    private readonly int _categoryId;
    public string Name => "Фільтр за категорією";

    public CategoryFilterStrategy(int categoryId) => _categoryId = categoryId;

    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions)
        => transactions.Where(t => t.CategoryId == _categoryId);
}

public class TypeFilterStrategy : IFilterStrategy
{
    private readonly TransactionType _type;
    public string Name => $"Фільтр за типом ({_type})";

    public TypeFilterStrategy(TransactionType type) => _type = type;

    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions)
        => transactions.Where(t => t.Type == _type);
}

public class AmountRangeFilterStrategy : IFilterStrategy
{
    private readonly decimal _min;
    private readonly decimal _max;
    public string Name => "Фільтр за сумою";

    public AmountRangeFilterStrategy(decimal min, decimal max)
    {
        _min = min;
        _max = max;
    }

    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions)
        => transactions.Where(t => t.Amount >= _min && t.Amount <= _max);
}

public class CompositeFilterStrategy : IFilterStrategy
{
    private readonly List<IFilterStrategy> _strategies;
    public string Name => "Комбінований фільтр";

    public CompositeFilterStrategy(IEnumerable<IFilterStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions)
        => _strategies.Aggregate(transactions, (current, strategy) => strategy.Apply(current));
}

public class DateDescSortStrategy : ISortStrategy
{
    public string Name => "За датою (нові спочатку)";
    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions)
        => transactions.OrderByDescending(t => t.Date);
}
public class AmountDescSortStrategy : ISortStrategy
{
    public string Name => "За сумою (більші спочатку)";
    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions)
        => transactions.OrderByDescending(t => t.Amount);
}
