using System.Text.Json;
using FinancialPlanner.Core.Exceptions;
using FinancialPlanner.Core.Repositories.Interfaces;

namespace FinancialPlanner.Data.Repositories;

public abstract class JsonRepository<T> : IRepository<T> where T : class
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    protected JsonRepository(string dataDirectory, string fileName)
    {
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, fileName);
    }

    protected abstract Guid GetId(T entity);

    protected async Task<List<T>> ReadAllAsync()
    {
        if (!File.Exists(_filePath))
            return new List<T>();

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
        }
        catch (Exception ex)
        {
            throw new StorageException($"Помилка читання даних з {_filePath}", ex);
        }
    }

    protected async Task WriteAllAsync(List<T> entities)
    {
        try
        {
            var json = JsonSerializer.Serialize(entities, JsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
        catch (Exception ex)
        {
            throw new StorageException($"Помилка запису даних до {_filePath}", ex);
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try { return await ReadAllAsync(); }
        finally { _lock.Release(); }
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(e => GetId(e) == id);
    }

    public async Task AddAsync(T entity)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAllAsync();
            all.Add(entity);
            await WriteAllAsync(all);
        }
        finally { _lock.Release(); }
    }

    public async Task UpdateAsync(T entity)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAllAsync();
            var index = all.FindIndex(e => GetId(e) == GetId(entity));
            if (index < 0) throw new EntityNotFoundException(GetId(entity));
            all[index] = entity;
            await WriteAllAsync(all);
        }
        finally { _lock.Release(); }
    }

    public async Task DeleteAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAllAsync();
            var removed = all.RemoveAll(e => GetId(e) == id);
            if (removed == 0) throw new EntityNotFoundException(id);
            await WriteAllAsync(all);
        }
        finally { _lock.Release(); }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        return entity != null;
    }
}
