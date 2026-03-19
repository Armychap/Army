namespace TaskPrototypeDemo;

public sealed class TaskDetails
{
    public List<string> Tags { get; } = new();
    public List<SubTask> SubTasks { get; } = new();

    public TaskDetails DeepClone()
    {
        var clone = new TaskDetails();
        clone.Tags.AddRange(Tags);
        clone.SubTasks.AddRange(SubTasks.Select(s => s.DeepClone()));
        return clone;
    }
}

