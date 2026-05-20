namespace FinancialPlanner.Core.Models;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "📁";
    public string ColorHex { get; set; } = "#607D8B";
    public TransactionType Type { get; set; }

    public Category() { }

    public Category(string name, TransactionType type, string icon = "📁", string colorHex = "#607D8B")
    {
        Name = name;
        Type = type;
        Icon = icon;
        ColorHex = colorHex;
    }

    public override string ToString() => $"{Icon} {Name}";
}
