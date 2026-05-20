namespace FinancialPlanner.Core.Models;

public enum TransactionType
{
    Income,
    Expense
}

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string Note { get; set; } = string.Empty;

    public Transaction() { }

    public Transaction(string title, decimal amount, TransactionType type, Guid categoryId, string note = "")
    {
        Title = title;
        Amount = amount;
        Type = type;
        CategoryId = categoryId;
        Note = note;
    }

    public override string ToString() =>
        $"[{Type}] {Title} — {Amount:C} ({Date:dd.MM.yyyy})";
}
