using FinancialPlanner.Core.Interfaces.Repositories;
using FinancialPlanner.Core.Interfaces.Services;
using FinancialPlanner.Core.Models;

namespace FinancialPlanner.Services.Services;

public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepository;

    public GoalService(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<Goal> AddGoalAsync(string name, string description, decimal target, DateTime deadline)
    {
        ValidateGoal(name, target, deadline);

        var goal = new Goal
        {
            Name = name.Trim(),
            Description = description.Trim(),
            TargetAmount = target,
            CurrentAmount = 0,
            Deadline = deadline,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        return await _goalRepository.AddAsync(goal);
    }

    public async Task<Goal> UpdateGoalAsync(int id, string name, string description, decimal target, DateTime deadline)
    {
        ValidateGoal(name, target, deadline);

        var goal = await _goalRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Ціль з ID {id} не знайдена.");

        goal.Name = name.Trim();
        goal.Description = description.Trim();
        goal.TargetAmount = target;
        goal.Deadline = deadline;

        return await _goalRepository.UpdateAsync(goal);
    }

    public async Task<Goal> ContributeToGoalAsync(int id, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Сума внеску має бути більше нуля.");

        var goal = await _goalRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Ціль з ID {id} не знайдена.");

        if (goal.IsCompleted)
            throw new InvalidOperationException("Ця ціль вже досягнута.");

        goal.CurrentAmount += amount;

        if (goal.CurrentAmount >= goal.TargetAmount)
        {
            goal.CurrentAmount = goal.TargetAmount;
            goal.IsCompleted = true;
            Console.WriteLine($"\n🎉 Вітаємо! Ціль \"{goal.Name}\" досягнута!\n");
        }

        return await _goalRepository.UpdateAsync(goal);
    }

    public async Task DeleteGoalAsync(int id)
    {
        var exists = await _goalRepository.ExistsAsync(id);
        if (!exists) throw new KeyNotFoundException($"Ціль з ID {id} не знайдена.");
        await _goalRepository.DeleteAsync(id);
    }

    public Task<IEnumerable<Goal>> GetAllGoalsAsync()
        => _goalRepository.GetAllAsync();

    public Task<IEnumerable<Goal>> GetActiveGoalsAsync()
        => _goalRepository.GetActiveGoalsAsync();

    private static void ValidateGoal(string name, decimal target, DateTime deadline)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва цілі не може бути порожньою.");
        if (target <= 0)
            throw new ArgumentException("Цільова сума має бути більше нуля.");
        if (deadline <= DateTime.UtcNow)
            throw new ArgumentException("Дедлайн має бути у майбутньому.");
    }
}
