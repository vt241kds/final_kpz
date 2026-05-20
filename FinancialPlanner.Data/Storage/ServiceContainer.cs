using FinancialPlanner.Core.Models;
using FinancialPlanner.Core.Repositories.Interfaces;
using FinancialPlanner.Core.Services;
using FinancialPlanner.Data.Repositories;

namespace FinancialPlanner.Data.Storage;

/// <summary>
/// Singleton service locator — створює та зберігає всі сервіси додатку.
/// Патерн Singleton: лише один екземпляр на весь час роботи додатку.
/// </summary>
public sealed class ServiceContainer
{
    private static ServiceContainer? _instance;
    private static readonly object _syncRoot = new();

    public static ServiceContainer Instance
    {
        get
        {
            if (_instance == null)
                lock (_syncRoot)
                    _instance ??= new ServiceContainer();
            return _instance;
        }
    }

    public ITransactionRepository TransactionRepository { get; }
    public ICategoryRepository CategoryRepository { get; }
    public IBudgetRepository BudgetRepository { get; }

    public TransactionService TransactionService { get; }
    public CategoryService CategoryService { get; }
    public BudgetService BudgetService { get; }
    public ReportService ReportService { get; }

    public AppSettings Settings { get; } = new();

    private ServiceContainer()
    {
        var dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FinancialPlanner",
            "data"
        );

        TransactionRepository = new JsonTransactionRepository(dataDir);
        CategoryRepository = new JsonCategoryRepository(dataDir);
        BudgetRepository = new JsonBudgetRepository(dataDir);

        TransactionService = new TransactionService(TransactionRepository, CategoryRepository);
        CategoryService = new CategoryService(CategoryRepository);
        BudgetService = new BudgetService(BudgetRepository, CategoryRepository, TransactionRepository);
        ReportService = new ReportService(TransactionRepository, CategoryRepository);
    }
}
