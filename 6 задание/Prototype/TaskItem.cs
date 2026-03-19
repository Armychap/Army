namespace TaskPrototypeDemo;

public sealed class TaskItem : IPrototype<TaskItem>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime DueDate { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskDetails Details { get; set; } = new();

    // Клон прототипа: дублирует данные, но вызывающий может "переназначить" идентификатор (Id).
    public TaskItem Clone()
    {
        return new TaskItem
        {
            Id = Id,
            Title = Title,
            Description = Description,
            DueDate = DueDate,
            Priority = Priority,
            Details = Details.DeepClone(),
        };
    }
}

