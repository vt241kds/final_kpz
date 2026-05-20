using FinancialPlanner.Core.Models;
using FinancialPlanner.Core.Repositories.Interfaces;

namespace FinancialPlanner.Data.Repositories;


public class JsonCategoryRepository : JsonRepository<Category>, ICategoryRepository
{
    public JsonCategoryRepository(string dataDirectory)
        : base(dataDirectory, "categories.json") { }

    protected override Guid GetId(Category entity) => entity.Id;

    public async Task<IEnumerable<Category>> GetByTypeAsync(TransactionType type)
    {
        var all = await GetAllAsync();
        return all.Where(c => c.Type == type);
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}

public class JsonBudgetRepository : JsonRepository<Core.Models.Budget>, IBudgetRepository
{
    public JsonBudgetRepository(string dataDirectory)
        : base(dataDirectory, "budgets.json") { }

    protected override Guid GetId(Core.Models.Budget entity) => entity.Id;

    public async Task<Core.Models.Budget?> GetByCategoryAndMonthAsync(Guid categoryId, int month, int year)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(b => b.CategoryId == categoryId && b.Month == month && b.Year == year);
    }

    public async Task<IEnumerable<Core.Models.Budget>> GetByMonthAsync(int month, int year)
    {
        var all = await GetAllAsync();
        return all.Where(b => b.Month == month && b.Year == year);
    }
}
