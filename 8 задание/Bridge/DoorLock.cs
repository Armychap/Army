namespace SmartHomeBridge;

public sealed class DoorLock : Device
{
    public string Entrance { get; }

    public DoorLock(string entrance, IControlImplementor implementor) : base(implementor)
    {
        Entrance = entrance;
    }

    public void Lock() => Implementor.Transmit("door", "lock", Entrance);

    public void Unlock() => Implementor.Transmit("door", "unlock", Entrance);
}
