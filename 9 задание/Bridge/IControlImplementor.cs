namespace SmartHomeBridge;

/// <summary>
/// Реализация моста: способ доставки команд до «железа» (локально, облако, голос).
/// Абстракция <see cref="Device"/> не зависит от конкретного канала.
/// </summary>
public interface IControlImplementor
{
    /// <param name="deviceKind">Тип устройства (light, thermostat, door).</param>
    /// <param name="action">Действие (on, off, set_temp, lock…).</param>
    /// <param name="parameter">Доп. данные (температура, имя комнаты).</param>
    void Transmit(string deviceKind, string action, string? parameter = null);
}
