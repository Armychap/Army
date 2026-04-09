namespace SmartHomeBridge;

public sealed class Light : Device
{
    public string Room { get; }

    public Light(string room, IControlImplementor implementor) : base(implementor)
    {
        Room = room;
    }

    public void TurnOn() => Implementor.Transmit("light", "on", Room);

    public void TurnOff() => Implementor.Transmit("light", "off", Room);
}
