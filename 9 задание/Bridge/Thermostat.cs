namespace SmartHomeBridge;

public sealed class Thermostat : Device
{
    public string Zone { get; }

    public Thermostat(string zone, IControlImplementor implementor) : base(implementor)
    {
        Zone = zone;
    }

    public void SetTemperature(int celsius) =>
        Implementor.Transmit("thermostat", "set_temp", $"{celsius}°C:{Zone}");
}
