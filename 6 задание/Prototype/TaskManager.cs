namespace TaskPrototypeDemo;

public sealed class TaskManager
{
    private readonly TaskPrototypeRegistry _registry;
    private TaskItem? _clipboard;

    public TaskManager(TaskPrototypeRegistry registry)
    {
        _registry = registry;
    }

    // Имитируем действие "Копировать задачу": клонируем прототип в буфер.
    public void CopyFromTemplate(string templateKey)
    {
        _clipboard = _registry.CreateClone(templateKey);
    }

    // Имитируем действие "Вставить": клонируем буфер и создаём новую идентичность.
    public TaskItem PasteFromClipboard(string newTitle)
    {
        if (_clipboard is null)
            throw new InvalidOperationException("Буфер пуст. Сначала вызовите CopyFromTemplate.");

        var pasted = _clipboard.Clone();
        pasted.Id = Guid.NewGuid();
        pasted.Title = newTitle;
        return pasted;
    }
}

