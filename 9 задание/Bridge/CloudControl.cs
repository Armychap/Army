namespace SmartHomeBridge;

/// <summary>Реализация: команды через облачный API провайдера.</summary>
public sealed class CloudControl : IControlImplementor
{
    private readonly TextWriter _out;

    public CloudControl(TextWriter? output = null) => _out = output ?? Console.Out;

    public void Transmit(string deviceKind, string action, string? parameter = null)
    {
        string body = string.IsNullOrEmpty(parameter) ? "" : $", body: \"{parameter}\"";
        _out.WriteLine($"  [Облако] POST /v1/devices/{deviceKind}/{action}{body}");
    }
}
