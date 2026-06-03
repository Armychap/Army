1. **Синглтон (Singleton)**

**Назначение:** Гарантирует, что класс имеет только один экземпляр, и предоставляет глобальную точку доступа к этому экземпляру

**Реализация:**
- [`Services/DamageService.cs`](Services/DamageService.cs) — `public static DamageService Instance`
- [`Services/ObserverManager.cs`](Services/ObserverManager.cs)

**Зачем:** Несколько частей приложения должны обращаться к одному сервису подсчёта урона или созданию юнитов.

```csharp
public class DamageService
{
    public static DamageService Instance { get; } = new DamageService();
    private DamageService() { }
    
    public void ResolveAttack(IUnit attacker, IUnit target) { ... }
}
```

2. **Фабричный метод (Factory Method)**

**Назначение:** Создание объектов через интерфейс, позволяя подклассам решать, какой класс инстанцировать.

**Реализация:**
- [`Services/UnitFactory.cs`](Services/UnitFactory.cs) — метод `CreateFromType(string unitType, int fighterNumber)`

**Зачем:** Загрузка сохранённых игр требует создания юнитов по строковому типу. Фабрика скрывает логику выбора конкретного конструктора.

```csharp
public IUnit CreateFromType(string unitType, int fighterNumber)
{
    return unitType switch
    {
        nameof(WeakFighter) => new WeakFighter(fighterNumber),
        nameof(Archer) => new Archer(fighterNumber),
        nameof(GulayGorod) => new GulayGorod(fighterNumber),
        // ...
    };
}
```

3. **Стратегия (Strategy)**

**Назначение:** Перестроение колонн (одна, три, стена)

**Реализация:**
- [`Game/Formations/IFormationStrategy.cs`](Game/Formations/IFormationStrategy.cs) — интерфейс
- [`Game/Formations/OneColumnStrategy.cs`](Game/Formations/OneColumnStrategy.cs)
- [`Game/Formations/ThreeColumnsStrategy.cs`](Game/Formations/ThreeColumnsStrategy.cs)
- [`Game/Formations/WallStrategy.cs`](Game/Formations/WallStrategy.cs)

**Зачем:** Игрок может выбрать разные боевые построения. Каждое построение реализует свою логику расстановки и атаки.

```csharp
public interface IFormationStrategy
{
    void DisplayRoundHeader(BattleEngine engine, int round);
    void DisplayBattleOrder(BattleEngine engine);
    // ...
}

// Каждая стратегия переопределяет поведение
public class OneColumnStrategy : IFormationStrategy { ... }
public class ThreeColumnsStrategy : IFormationStrategy { ... }
```


4. **Наблюдатель (Observer)**

**Назначение:** Логирование и бип

**Реализация:**
- [`Models/Interfaces/IUnitObserver.cs`](Models/Interfaces/IUnitObserver.cs) — интерфейс наблюдателя
- [`Models/Observers/DeathBeepObserver.cs`](Models/Observers/DeathBeepObserver.cs)
- [`Models/Observers/DamageLogObserver.cs`](Models/Observers/DamageLogObserver.cs)
- [`Services/ObserverManager.cs`](Services/ObserverManager.cs)

**Зачем:** Юниты генерируют события (получение урона, смерть, исцеление). Различные наблюдатели реагируют на события: логирование в файл, звук при смерти и т.д.

```csharp
public interface IUnitObserver
{
    void OnDamageTaken(IUnit unit, int damage, string attackerName, int newHealth);
    void OnDeath(IUnit unit, string killerName);
    void OnHealed(IUnit unit, int amount, int newHealth);
}

// Юнит уведомляет наблюдателей
foreach (var observer in Observers)
{
    observer.OnDamageTaken(this, actualDamage, attackerName, Health);
}
```


5. **Прототип (Prototype)**

**Назначение:** Клонирование магом

**Реализация:**
- [`Models/CloneAbility.cs`](Models/CloneAbility.cs) — способность мага клонировать юнитов
- [`Models/Units/Wizard.cs`](Models/Units/Wizard.cs) — маг использует `CloneAbility`

**Зачем:** Маг может клонировать союзников. Вместо создания нового юнита с нуля, копируются его характеристики (здоровье, максимум здоровья и т.д.).

```csharp
public class CloneAbility : AbilityBase
{
    public void Execute(IUnit? user, IUnit? target)
    {
        // Копируем характеристики выбранного юнита
        IUnit clone;
        if (chosen is Archer)
            clone = new Archer(newFighterNumber);
        else
            clone = new WeakFighter(newFighterNumber);
        
        // Копируем состояние (здоровье)
        clone.Health = chosen.Health;
        clone.MaxHealth = chosen.MaxHealth;
    }
}
```


6. **Адаптер (Adapter)**

**Назначение:** Преобразование интерфейса класса в другой интерфейс. Адаптер позволяет несовместимым интерфейсам работать вместе.

**Реализация:**
- [`Models/Units/GulayGorod.cs`](Models/Units/GulayGorod.cs) — адаптирует сооружение "Гуляй-город" под интерфейс IUnit
- [`Models/Interfaces/IUnitAdapter.cs`](Models/Interfaces/IUnitAdapter.cs) — интерфейс для адаптеров
- [`Models/UnitExtensions.cs`](Models/UnitExtensions.cs) — метод `GetRootUnit()` разворачивает адаптеры

**Зачем:** "Гуляй-город" — не обычный боец, блокирует половину урона, не может атаковать, не может быть клонирован. Адаптер позволяет использовать его как обычный IUnit без изменения интерфейса.

```csharp
public class GulayGorod : IUnit
{
    public void TakeDamage(int damage, string attackerName)
    {
        // АДАПТЕР: Специальная механика урона для Гуляй-города
        // Блокирует половину входящего урона
        int reducedDamage = Math.Max(1, damage - (Defence / 2));
        Health -= reducedDamage;
    }
    
    public void AttackUnit(IUnit target)
    {
        // АДАПТЕР: Не может атаковать, только защищает
    }
}
```


7. **Абстрактная фабрика (Abstract Factory)**

**Назначение**: Создание семейств связанных объектов (всех типов юнитов) без указания их конкретных классов.

**Реализация**:

- [`Services/UnitFactory.cs`] — создание юнитов по строковому типу

**Зачем**: Юниты (слабый боец, лучник, сильный боец, лекарь, маг, стена) — это семейство боевых единиц. Фабрика создаёт нужного юнита по строковому типу, скрывая выбор конкретного класса. Особенно полезна при загрузке из JSON, где тип юнита записан как строка.

```csharp
public class UnitFactory
{
    // Абстрактная фабрика — умеет создавать ЛЮБОГО юнита по строковому типу
    public IUnit CreateFromType(string unitType, int fighterNumber)
    {
        return unitType switch
        {
            nameof(WeakFighter) => new WeakFighter(fighterNumber),
            nameof(Archer) => new Archer(fighterNumber),
            nameof(StrongFighter) => new StrongFighter(fighterNumber),
            nameof(Healer) => new Healer(fighterNumber),
            nameof(Wizard) => new Wizard(fighterNumber),
            nameof(ShieldWall) => new ShieldWall(fighterNumber),
            nameof(ShieldWallAdapter) => new ShieldWallAdapter(fighterNumber),
            _ => throw new InvalidOperationException($"Неизвестный тип юнита: {unitType}")
        };
    }
}

// Статический провайдер (синглтон фабрики)
public static class UnitFactoryProvider
{
    public static UnitFactory Instance { get; } = new UnitFactory();
}
```


8. **Декоратор (Decorator)**

**Назначение:** Динамическое добавление новых возможностей к объекту путём обёртывания его в объект-декоратор.

**Реализация:**
- [`Models/Decorators/BuffDecorator.cs`](Models/Decorators/BuffDecorator.cs) — базовый класс для всех баффов
- [`Models/Decorators/HorseBuffDecorator.cs`](Models/Decorators/HorseBuffDecorator.cs) — +2 атака
- [`Models/Decorators/ShieldBuffDecorator.cs`](Models/Decorators/ShieldBuffDecorator.cs) — +3 защита
- [`Models/Decorators/HelmetBuffDecorator.cs`](Models/Decorators/HelmetBuffDecorator.cs) — +5 здоровья
- [`Models/Decorators/SpearBuffDecorator.cs`](Models/Decorators/SpearBuffDecorator.cs) — +1 атака

**Зачем:** Каждый юнит может иметь несколько баффов одновременно (конь + щит + шлем). Вместо создания класса для каждой комбинации, используются вложенные декораторы.

```csharp
public abstract class BuffDecorator : IUnit
{
    protected IUnit _unit;  // Обёрнутый юнит
    
    public override int EffectiveAttack => _unit.EffectiveAttack + BonusAttack;
    public override int EffectiveDefence => _unit.EffectiveDefence + BonusDefence;
}

// Применение нескольких баффов
IUnit unit = new Archer(1);
unit = new HorseBuffDecorator(unit);      // +2 атака
unit = new ShieldBuffDecorator(unit);     // +3 защита
unit = new HelmetBuffDecorator(unit);     // +5 здоровья
```


9. **Команда (Command)**

**Назначение:** Инкапсуляция запроса в объект, позволяя параметризировать клиентов с различными запросами, ставить запросы в очередь, логировать запросы и поддерживать отмену операций.

**Реализация:**
- [`Game/Commands/ICommand.cs`](Game/Commands/ICommand.cs) — интерфейс команды
- [`Game/Commands/MakeMoveCommand.cs`](Game/Commands/MakeMoveCommand.cs)
- [`Game/Commands/AutoBattleCommand.cs`](Game/Commands/AutoBattleCommand.cs)
- [`Game/Commands/ChangeFormationCommand.cs`](Game/Commands/ChangeFormationCommand.cs)
- [`Game/Commands/CommandManager.cs`](Game/Commands/CommandManager.cs)

**Зачем:** Битва состоит из последовательности команд (ход, смена формации, автобитва). Команды можно ставить в очередь, логировать и потенциально отменять.

```csharp
public interface ICommand
{
    void Execute();
}

public class MakeMoveCommand : ICommand
{
    private BattleEngine _engine;
    
    public void Execute()
    {
        _engine.MakeMove();
    }
}

// Менеджер команд
public class CommandManager
{
    private List<ICommand> _commandHistory = new();
    
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _commandHistory.Add(command);  // Логирование
    }
}
```

Структура папок

```
ArmyGame/
├── Models/
│   ├── Units/                    # Различные типы юнитов (Archer, Wizard и т.д.)
│   ├── Decorators/               # Баффы (HorseBuffDecorator, ShieldBuffDecorator и т.д.)
│   ├── Observers/                # Наблюдатели (DeathBeepObserver, DamageLogObserver)
│   ├── Interfaces/               # Интерфейсы (IUnit, IUnitObserver, IUnitAdapter)
│   ├── Unit.cs                   # Базовый класс для всех юнитов
│   ├── CloneAbility.cs           # Прототип (копирование)
│   └── UnitExtensions.cs         # Вспомогательные методы (GetRootUnit и т.д.)
├── Services/
│   ├── DamageService.cs          # Синглтон для подсчёта урона
│   ├── UnitFactory.cs            # Фабричный метод, абстрактная фабрика
│   ├── BuffFactory.cs            # Абстрактная фабрика
│   ├── ObserverManager.cs        # Управление наблюдателями
│   └── ArmyManager.cs            # Сохранение и загрузка
├── Game/
│   ├── Formations/               # Стратегия (разные построения)
│   │   ├── IFormationStrategy.cs
│   │   ├── OneColumnStrategy.cs
│   │   ├── ThreeColumnsStrategy.cs
│   │   └── WallStrategy.cs
│   ├── Commands/                 # Команда (действия в битве)
│   │   ├── ICommand.cs
│   │   ├── MakeMoveCommand.cs
│   │   ├── CommandManager.cs
│   │   └── ...
│   └── Battle/                   # Логика битвы
└── README.md                     # Этот файл
```

---

Как паттерны работают вместе

1. **Синглтон** предоставляет единственный доступ к сервисам (урон, фабрика).
2. **Фабричный метод** и **абстрактная фабрика** создают юниты и баффы.
3. **Наблюдатель** уведомляет о событиях (урон, смерть).
4. **Декоратор** добавляет баффы к юнитам.
5. **Адаптер** позволяет использовать "Гуляй-город" как обычного юнита.
6. **Прототип** копирует юнитов через мага.
7. **Стратегия** выбирает боевое построение.
8. **Команда** инкапсулирует действия в битве.

---

Примеры использования

#Создание и применение баффа (Декоратор + Абстрактная фабрика)
```csharp
IUnit unit = new Archer(1);
unit = BuffFactory.ApplyBuff(unit, "Horse");      // Добавляем коня
unit = BuffFactory.ApplyBuff(unit, "Shield");     // Добавляем щит
```

#Загрузка игры (Фабричный метод)
```csharp
IUnit unit = UnitFactoryProvider.Instance.CreateFromType("Archer", fighterNumber);
```

#Добавление наблюдателей (Наблюдатель)
```csharp
unit.AttachObserver(new DamageLogObserver());
unit.AttachObserver(new DeathBeepObserver());
```

#Выполнение хода (Команда)
```csharp
var moveCmd = new MakeMoveCommand(battleEngine);
commandManager.ExecuteCommand(moveCmd);
```

---

**Автор:** Alex  
**Язык:** C# (.NET 9)  
**Паттерны:** 9 основных паттернов проектирования
