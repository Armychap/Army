namespace GameFacade;

/// <summary>Форматирование консольного вывода для демо (отделено от игровой логики).</summary>
public sealed class DemoLog
{
    private readonly TextWriter _out;

    public DemoLog(TextWriter output) => _out = output;

    public void Line(string? text = null) => _out.WriteLine(text ?? string.Empty);

    public void LineFormatted(string format, params object?[] args) => _out.WriteLine(format, args);
}
