using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Core.Patterns.Strategies;

public interface ISortStrategy
{
    string Name { get; }
    IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions);
}

public class SortByDateDescStrategy : ISortStrategy
{
    public string Name => "За датою (новіші спершу)";
    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions) =>
        transactions.OrderByDescending(t => t.Date);
}

public class SortByDateAscStrategy : ISortStrategy
{
    public string Name => "За датою (старіші спершу)";
    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions) =>
        transactions.OrderBy(t => t.Date);
}

public class SortByAmountDescStrategy : ISortStrategy
{
    public string Name => "За сумою (більші спершу)";
    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions) =>
        transactions.OrderByDescending(t => t.Amount);
}

public class SortByAmountAscStrategy : ISortStrategy
{
    public string Name => "За сумою (менші спершу)";
    public IEnumerable<Transaction> Apply(IEnumerable<Transaction> transactions) =>
        transactions.OrderBy(t => t.Amount);
}
