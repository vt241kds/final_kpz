using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Core.Repositories.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Transaction>> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type);
    Task<IEnumerable<Transaction>> GetByMonthAsync(int month, int year);
    Task<decimal> GetTotalByTypeAsync(TransactionType type, DateTime from, DateTime to);
    Task<decimal> GetTotalByCategoryAsync(Guid categoryId, DateTime from, DateTime to);
}
