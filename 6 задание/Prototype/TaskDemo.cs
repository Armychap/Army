namespace TaskPrototypeDemo;

public static class TaskDemo
{
    public static void Run()
    {
        // Создание шаблона задачи (прототипа) с базовыми данными
        var template = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Исправить баг входа",
            Description = "Users sometimes fail to authenticate.",
            DueDate = DateTime.Today.AddDays(2),
            Priority = TaskPriority.High,
            Details = new TaskDetails()
        };

        template.Details.Tags.AddRange(new[] { "ошибка", "аутентификация" });
        template.Details.SubTasks.Add(new SubTask { Title = "Воспроизвести проблему", IsDone = false });
        template.Details.SubTasks.Add(new SubTask { Title = "Добавить проверку MFA", IsDone = false });

        var registry = new TaskPrototypeRegistry();
        registry.Register("login-bug", template);

        var manager = new TaskManager(registry);

        Console.WriteLine("ШАБЛОН ПРОТОТИПА (источник)");
        Print(template);

        manager.CopyFromTemplate("login-bug");

        var task1 = manager.PasteFromClipboard("Исправить баг входа (для пользователя A)");
        var task2 = manager.PasteFromClipboard("Исправить баг входа (для пользователя B)");

        Console.WriteLine();
        Console.WriteLine("ЗАДАЧА 1 (вставленная копия)");
        Print(task1);

        Console.WriteLine();
        Console.WriteLine("ЗАДАЧА 2 (вставленная копия)");
        Print(task2);

        Console.WriteLine();
        Console.WriteLine("Изменяем ЗАДАЧУ 1: добавим тег 'срочно' и отметим первую подзадачу как выполненную.");
        task1.Details.Tags.Add("срочно");
        task1.Details.SubTasks[0].IsDone = true;

        Console.WriteLine();
        Console.WriteLine("ПОСЛЕ ИЗМЕНЕНИЙ");
        Console.WriteLine("Шаблон должен остаться без изменений (deep clone).");
        Console.WriteLine();
        Console.WriteLine("Шаблон");
        Print(template);

        Console.WriteLine();
        Console.WriteLine("Задача 1");
        Print(task1);

        Console.WriteLine();
        Console.WriteLine("Задача 2");
        Print(task2);
    }

    private static void Print(TaskItem task)
    {
        Console.WriteLine($"Id:           {task.Id}");
        Console.WriteLine($"Название:     {task.Title}");
        Console.WriteLine($"Срок:         {task.DueDate:yyyy-MM-dd}");
        Console.WriteLine($"Приоритет:    {ToPriorityRu(task.Priority)}");

        Console.WriteLine("Теги:         " + string.Join(", ", task.Details.Tags));
        Console.WriteLine("Подзадачи:");
        foreach (var st in task.Details.SubTasks)
            Console.WriteLine("  - " + st);

        static string ToPriorityRu(TaskPriority priority) => priority switch
        {
            TaskPriority.Low => "Низкий",
            TaskPriority.Medium => "Средний",
            TaskPriority.High => "Высокий",
            _ => priority.ToString()
        };
    }
}
