using FinancialPlanner.Core.Exceptions;
using FinancialPlanner.Core.Models;
using FinancialPlanner.Core.Repositories.Interfaces;

namespace FinancialPlanner.Core.Services;

public class CategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Category> AddCategoryAsync(string name, TransactionType type, string icon = "📁", string colorHex = "#607D8B")
    {
        ValidateCategory(name);

        var existing = await _categoryRepository.GetByNameAsync(name);
        if (existing != null)
            throw new ValidationException($"Категорія з назвою '{name}' вже існує.");

        var category = new Category(name, type, icon, colorHex);
        await _categoryRepository.AddAsync(category);
        return category;
    }

    public async Task UpdateCategoryAsync(Guid id, string name, string icon, string colorHex)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(id);

        ValidateCategory(name);
        category.Name = name;
        category.Icon = icon;
        category.ColorHex = colorHex;

        await _categoryRepository.UpdateAsync(category);
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        if (!await _categoryRepository.ExistsAsync(id))
            throw new EntityNotFoundException(id);

        await _categoryRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Category>> GetAllAsync() =>
        await _categoryRepository.GetAllAsync();

    public async Task<IEnumerable<Category>> GetByTypeAsync(TransactionType type) =>
        await _categoryRepository.GetByTypeAsync(type);

    public async Task<Category?> GetByIdAsync(Guid id) =>
        await _categoryRepository.GetByIdAsync(id);

    public async Task SeedDefaultCategoriesAsync()
    {
        var existing = await _categoryRepository.GetAllAsync();
        if (existing.Any()) return;

        var defaults = new List<Category>
        {
            new("Зарплата", TransactionType.Income, "💼", "#4CAF50"),
            new("Фріланс", TransactionType.Income, "💻", "#8BC34A"),
            new("Інші доходи", TransactionType.Income, "💰", "#CDDC39"),
            new("Продукти", TransactionType.Expense, "🛒", "#FF5722"),
            new("Транспорт", TransactionType.Expense, "🚗", "#FF9800"),
            new("Комунальні послуги", TransactionType.Expense, "🏠", "#F44336"),
            new("Розваги", TransactionType.Expense, "🎮", "#9C27B0"),
            new("Здоров'я", TransactionType.Expense, "💊", "#E91E63"),
            new("Одяг", TransactionType.Expense, "👕", "#3F51B5"),
            new("Освіта", TransactionType.Expense, "📚", "#2196F3"),
            new("Ресторани", TransactionType.Expense, "🍕", "#FF5252"),
            new("Інше", TransactionType.Expense, "📦", "#607D8B"),
        };

        foreach (var cat in defaults)
            await _categoryRepository.AddAsync(cat);
    }

    private static void ValidateCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Назва категорії не може бути порожньою.");

        if (name.Length > 50)
            throw new ValidationException("Назва категорії не може перевищувати 50 символів.");
    }
}
