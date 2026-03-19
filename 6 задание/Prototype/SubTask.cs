namespace TaskPrototypeDemo;

public sealed class SubTask
{
    public string Title { get; set; } = "";
    public bool IsDone { get; set; }

    public SubTask DeepClone() =>
        new()
        {
            Title = Title,
            IsDone = IsDone
        };

    public override string ToString() => $"{(IsDone ? "[x]" : "[ ]")} {Title}";
}

