using FinancialPlanner.Core.Models;
using FinancialPlanner.Core.Repositories.Interfaces;

namespace FinancialPlanner.Data.Repositories;

public class JsonTransactionRepository : JsonRepository<Transaction>, ITransactionRepository
{
    public JsonTransactionRepository(string dataDirectory)
        : base(dataDirectory, "transactions.json") { }

    protected override Guid GetId(Transaction entity) => entity.Id;

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var all = await GetAllAsync();
        return all.Where(t => t.Date >= from && t.Date <= to);
    }

    public async Task<IEnumerable<Transaction>> GetByCategoryAsync(Guid categoryId)
    {
        var all = await GetAllAsync();
        return all.Where(t => t.CategoryId == categoryId);
    }

    public async Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type)
    {
        var all = await GetAllAsync();
        return all.Where(t => t.Type == type);
    }

    public async Task<IEnumerable<Transaction>> GetByMonthAsync(int month, int year)
    {
        var all = await GetAllAsync();
        return all.Where(t => t.Date.Month == month && t.Date.Year == year);
    }

    public async Task<decimal> GetTotalByTypeAsync(TransactionType type, DateTime from, DateTime to)
    {
        var transactions = await GetByDateRangeAsync(from, to);
        return transactions.Where(t => t.Type == type).Sum(t => t.Amount);
    }

    public async Task<decimal> GetTotalByCategoryAsync(Guid categoryId, DateTime from, DateTime to)
    {
        var transactions = await GetByDateRangeAsync(from, to);
        return transactions.Where(t => t.CategoryId == categoryId && t.Type == TransactionType.Expense).Sum(t => t.Amount);
    }
}
