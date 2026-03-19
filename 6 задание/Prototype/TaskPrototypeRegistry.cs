namespace TaskPrototypeDemo;

public sealed class TaskPrototypeRegistry
{
    private readonly Dictionary<string, IPrototype<TaskItem>> _prototypes =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(string key, IPrototype<TaskItem> prototype)
    {
        if (_prototypes.ContainsKey(key))
            throw new InvalidOperationException($"Прототип '{key}' уже зарегистрирован.");

        _prototypes[key] = prototype;
    }

    public TaskItem CreateClone(string key)
    {
        if (!_prototypes.TryGetValue(key, out var prototype))
            throw new KeyNotFoundException($"Неизвестный прототип '{key}'.");

        return prototype.Clone();
    }
}

