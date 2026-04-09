namespace SmartHomeBridge;

/// <summary>Реализация: интерпретация голосовых команд ассистентом.</summary>
public sealed class VoiceControl : IControlImplementor
{
    private readonly TextWriter _out;

    public VoiceControl(TextWriter? output = null) => _out = output ?? Console.Out;

    public void Transmit(string deviceKind, string action, string? parameter = null)
    {
        string phrase = BuildPhrase(deviceKind, action, parameter);
        _out.WriteLine($"  [Голос] «{phrase}» → отправлено на исполнение");
    }

    private static string BuildPhrase(string deviceKind, string action, string? parameter)
    {
        return (deviceKind, action) switch
        {
            ("light", "on") => $"Включи свет ({parameter})",
            ("light", "off") => $"Выключи свет ({parameter})",
            ("thermostat", "set_temp") => BuildThermostatPhrase(parameter),
            ("door", "lock") => $"Закрой замок ({parameter})",
            ("door", "unlock") => $"Открой замок ({parameter})",
            _ => $"{deviceKind} {action} {parameter}"
        };
    }

    private static string BuildThermostatPhrase(string? parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return "Установи температуру";
        // формат: "22°C:спальня"
        int sep = parameter.IndexOf(':');
        if (sep < 0)
            return $"Установи температуру {parameter}";
        string temp = parameter[..sep];
        string zone = parameter[(sep + 1)..];
        return $"Установи температуру {temp} для зоны «{zone}»";
    }
}
