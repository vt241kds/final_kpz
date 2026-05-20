namespace FinancialPlanner.Core.Exceptions;

public class EntityNotFoundException : Exception
{
    public Guid EntityId { get; }

    public EntityNotFoundException(Guid id)
        : base($"Сутність з ID {id} не знайдена.") => EntityId = id;

    public EntityNotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base("Помилка валідації: " + string.Join("; ", errors))
        => Errors = errors.ToList().AsReadOnly();

    public ValidationException(string error)
        : base($"Помилка валідації: {error}")
        => Errors = new List<string> { error }.AsReadOnly();
}

public class DuplicateBudgetException : Exception
{
    public DuplicateBudgetException(string categoryName, int month, int year)
        : base($"Бюджет для категорії '{categoryName}' на {month}/{year} вже існує.") { }
}

public class StorageException : Exception
{
    public StorageException(string message, Exception? inner = null)
        : base(message, inner) { }
}
