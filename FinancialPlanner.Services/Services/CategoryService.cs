using FinancialPlanner.Core.Interfaces.Repositories;
using FinancialPlanner.Core.Interfaces.Services;
using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Services.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Category> AddCategoryAsync(string name, string icon, string color)
    {
        ValidateName(name);

        var existing = await _categoryRepository.GetByNameAsync(name);
        if (existing is not null)
            throw new InvalidOperationException($"Категорія '{name}' вже існує.");

        var category = new Category
        {
            Name = name.Trim(),
            Icon = string.IsNullOrWhiteSpace(icon) ? "📁" : icon.Trim(),
            Color = string.IsNullOrWhiteSpace(color) ? "#808080" : color.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        return await _categoryRepository.AddAsync(category);
    }

    public async Task<Category> UpdateCategoryAsync(int id, string name, string icon, string color)
    {
        ValidateName(name);

        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Категорія з ID {id} не знайдена.");

        category.Name  = name.Trim();
        category.Icon  = string.IsNullOrWhiteSpace(icon) ? "📁" : icon.Trim();
        category.Color = string.IsNullOrWhiteSpace(color) ? "#808080" : color.Trim();

        return await _categoryRepository.UpdateAsync(category);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Категорія з ID {id} не знайдена.");

        var hasTransactions = await _categoryRepository.HasTransactionsAsync(id);
        if (hasTransactions)
            throw new InvalidOperationException(
                $"Неможливо видалити категорію '{category.Name}' — вона має пов'язані транзакції.");

        await _categoryRepository.DeleteAsync(id);
    }

    public Task<Category?> GetCategoryAsync(int id)
        => _categoryRepository.GetByIdAsync(id);

    public Task<IEnumerable<Category>> GetAllCategoriesAsync()
        => _categoryRepository.GetAllAsync();

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва категорії не може бути порожньою.");
        if (name.Length > 50)
            throw new ArgumentException("Назва категорії не може перевищувати 50 символів.");
    }
}
