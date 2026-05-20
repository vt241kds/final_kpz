# 💰 Financial Planner

Консольний фінансовий планувальник на C# (.NET 8) для управління доходами, витратами, бюджетами та генерації звітів.

---

## 🚀 Функціональність

| Модуль | Опис |
|---|---|
| **Dashboard** | Огляд балансу місяця, прогрес бюджетів, останні транзакції |
| **Транзакції** | Додавання, редагування, видалення, фільтрація та сортування |
| **Категорії** | Управління категоріями доходів та витрат |
| **Бюджети** | Встановлення лімітів, автоматичні сповіщення при перевищенні |
| **Звіти** | Місячний звіт, розбивка за категоріями, доходи vs витрати, експорт CSV/JSON |

### Запуск локально

```bash
git clone https://github.com/vt241kds/final_kpz.git
cd FinancialPlanner
dotnet run --project FinancialPlanner.UI/FinancialPlanner.UI.csproj
```

**Вимоги:** .NET SDK 8.0+

Дані зберігаються у `%AppData%/FinancialPlanner/data/` (Windows) або `~/.config/FinancialPlanner/data/` (Linux/Mac) у форматі JSON.

---

## 🏗️ Структура проєкту

```
FinancialPlanner/
├── FinancialPlanner.Core/          # Бізнес-логіка (не залежить від зовнішніх бібліотек)
│   ├── Models/                     # Transaction, Category, Budget, Report, AppSettings
│   ├── Repositories/Interfaces/    # IRepository<T>, ITransactionRepository, ...
│   ├── Services/                   # TransactionService, BudgetService, ...
│   ├── Patterns/
│   │   ├── Observers/              # IBudgetObserver, ConsoleBudgetAlertObserver
│   │   ├── Strategies/             # IFilterStrategy, ISortStrategy та реалізації
│   │   └── Factories/              # ReportFactory, IReportBuilder та білдери
│   └── Exceptions/                 # Власні виключення
├── FinancialPlanner.Data/          # Доступ до даних
│   ├── Repositories/               # JsonRepository<T> + конкретні реалізації
│   └── Storage/                    # ServiceContainer (Singleton)
└── FinancialPlanner.UI/            # Консольний інтерфейс
    ├── Views/                      # DashboardView, TransactionView, ...
    └── Helpers/                    # ConsoleHelper
```

---

## 🧩 Design Patterns

### 1. Repository Pattern
**Файли:** [`FinancialPlanner.Data/Repositories/JsonRepository.cs`](FinancialPlanner.Data/Repositories/JsonRepository.cs), [`IRepository.cs`](FinancialPlanner.Core/Repositories/Interfaces/IRepository.cs)

Абстрагує роботу з даними від бізнес-логіки. Базовий клас `JsonRepository<T>` реалізує CRUD через JSON-файли і є основою для `JsonTransactionRepository`, `JsonCategoryRepository`, `JsonBudgetRepository`. Замінити JSON на SQLite можна, не змінивши жодного рядка у сервісах.

```csharp
public abstract class JsonRepository<T> : IRepository<T> where T : class
{
    public async Task<IEnumerable<T>> GetAllAsync() { ... }
    public async Task AddAsync(T entity) { ... }
    // ...
}
```

### 2. Observer Pattern
**Файли:** [`IBudgetObserver.cs`](FinancialPlanner.Core/Patterns/Observers/IBudgetObserver.cs), [`BudgetAlertObserver.cs`](FinancialPlanner.Core/Patterns/Observers/BudgetAlertObserver.cs), [`BudgetService.cs`](FinancialPlanner.Core/Services/BudgetService.cs)

`BudgetService` реалізує `IBudgetSubject` — автоматично сповіщає підписників при перевищенні бюджетного порогу. `ConsoleBudgetAlertObserver` виводить попередження в консоль, `InMemoryAlertLogObserver` зберігає лог.

```csharp
services.BudgetService.Subscribe(new ConsoleBudgetAlertObserver());
await budgetService.CheckAndNotifyBudgetAlertsAsync(categoryId, month, year);
```

### 3. Strategy Pattern
**Файли:** [`FilterStrategies.cs`](FinancialPlanner.Core/Patterns/Strategies/FilterStrategies.cs), [`SortStrategies.cs`](FinancialPlanner.Core/Patterns/Strategies/SortStrategies.cs)

Алгоритми фільтрації та сортування транзакцій інкапсульовані в окремі стратегії. `CompositeFilterStrategy` комбінує будь-яку кількість фільтрів.

```csharp
var filter = new CompositeFilterStrategy()
    .AddFilter(new DateRangeFilterStrategy(from, to))
    .AddFilter(new TypeFilterStrategy(TransactionType.Expense));
var result = await transactionService.GetFilteredAsync(filter);
```

### 4. Factory Pattern
**Файли:** [`ReportFactory.cs`](FinancialPlanner.Core/Patterns/Factories/ReportFactory.cs)

`ReportFactory` делегує побудову звітів конкретним `IReportBuilder` залежно від типу. Додати новий тип звіту — реалізувати `IReportBuilder` і зареєструвати в фабриці.

```csharp
var report = reportFactory.Create(ReportType.CategoryBreakdown, transactions, categories, from, to);
```

### 5. Singleton Pattern
**Файли:** [`ServiceContainer.cs`](FinancialPlanner.Data/Storage/ServiceContainer.cs)

`ServiceContainer` — єдиний екземпляр на весь час роботи додатку, забезпечує потокобезпечне створення через double-checked locking.

```csharp
public sealed class ServiceContainer
{
    private static ServiceContainer? _instance;
    private static readonly object _syncRoot = new();
    public static ServiceContainer Instance { get { ... } }
}
```

---

## 🔑 Programming Principles

### 1. SRP (Single Responsibility Principle)
Кожен клас відповідає за одне: `TransactionService` — лише CRUD транзакцій, `BudgetService` — лише бюджети та сповіщення, `ReportService` — лише генерація звітів. UI-класи (`DashboardView`, `TransactionView`) відповідають лише за відображення.

### 2. OCP (Open/Closed Principle)
`ReportFactory` відкрита для розширення (новий `IReportBuilder`) і закрита для змін — додавання нового типу звіту не змінює фабрику. Аналогічно — нова стратегія фільтра не змінює сервіс.

### 3. LSP (Liskov Substitution Principle)
`JsonTransactionRepository` повністю замінює `ITransactionRepository` — сервіси не знають про конкретну реалізацію. Будь-який `IBudgetObserver` можна підключити до `BudgetService`.

### 4. DRY (Don't Repeat Yourself)
Базовий `JsonRepository<T>` містить єдину реалізацію CRUD для всіх репозиторіїв. `ConsoleHelper` централізує весь вивід у консоль. Валідація зосереджена в сервісах.

### 5. KISS (Keep It Simple, Stupid)
Зберігання даних — JSON файли замість складної бази даних. Кожен метод виконує одну чітку дію. Відсутність зайвих абстракцій там, де вони не потрібні.

### 6. YAGNI (You Aren't Gonna Need It)
Реалізовано лише потрібний функціонал. Немає передчасної оптимізації, кешування чи складних патернів там, де вони не виправдані.

### 7. Fail Fast
Валідація відбувається на початку методів сервісів. Власні виключення (`ValidationException`, `EntityNotFoundException`) дають чіткий опис помилки.

---

## 🔧 Refactoring Techniques

| Техніка | Де застосовано |
|---|---|
| **Extract Method** | Виділення `ValidateTransactionAsync`, `ValidateBudget`, `ValidateCategory` з основної логіки сервісів |
| **Extract Interface** | `IRepository<T>`, `IFilterStrategy`, `ISortStrategy`, `IReportBuilder`, `IBudgetObserver` — всі абстраговані через інтерфейси |
| **Extract Class** | `ConsoleHelper` виділено з View-класів; `ReportFactory` виділено з `ReportService` |
| **Replace Magic Numbers with Constants** | Дефолтні значення (поріг 80%, максимум назви 50 символів) як іменовані параметри |
| **Move Method** | Вся бізнес-логіка переміщена з UI (`Views`) до сервісів (`Services`) |
| **Rename Variable/Method** | Зрозумілі імена: `GetFilteredAndSortedAsync`, `CheckAndNotifyBudgetAlertsAsync`, `SeedDefaultCategoriesAsync` |
| **Replace Conditional with Polymorphism** | `switch` по типу звіту замінено фабрикою зі словником білдерів |
| **Introduce Parameter Object** | `BudgetAlertEventArgs` замість передачі окремих параметрів у спостерігачі |

---

## 📊 Підрахунок рядків коду

```bash
git ls-files '*.cs' | xargs wc -l
```

---

## 🛠️ Технологічний стек

- **C# 12 / .NET 8**
- **System.Text.Json** — серіалізація/десеріалізація даних
- **SemaphoreSlim** — потокобезпечний доступ до файлів
- Зберігання: JSON файли (`transactions.json`, `categories.json`, `budgets.json`)
