namespace SmartHomeBridge;

/// <summary>Абстракция моста: устройство умного дома, не привязанное к способу управления.</summary>
public abstract class Device
{
    protected IControlImplementor Implementor { get; private set; }

    protected Device(IControlImplementor implementor)
    {
        Implementor = implementor;
    }

    /// <summary>Сменить канал управления без смены класса устройства.</summary>
    public void ChangeImplementor(IControlImplementor implementor) =>
        Implementor = implementor;
}
