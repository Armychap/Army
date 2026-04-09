namespace SmartHomeBridge;

/// <summary>Реализация: прямое управление по локальной сети / хабу.</summary>
public sealed class LocalControl : IControlImplementor
{
    private readonly TextWriter _out;

    public LocalControl(TextWriter? output = null) => _out = output ?? Console.Out;

    public void Transmit(string deviceKind, string action, string? parameter = null)
    {
        string extra = string.IsNullOrEmpty(parameter) ? "" : $" [{parameter}]";
        _out.WriteLine($"  [Локально] {deviceKind}.{action}{extra} → пакет в LAN");
    }
}
