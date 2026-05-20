using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Core.Repositories.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetByTypeAsync(TransactionType type);
    Task<Category?> GetByNameAsync(string name);
}

public interface IBudgetRepository : IRepository<Budget>
{
    Task<Budget?> GetByCategoryAndMonthAsync(Guid categoryId, int month, int year);
    Task<IEnumerable<Budget>> GetByMonthAsync(int month, int year);
}
