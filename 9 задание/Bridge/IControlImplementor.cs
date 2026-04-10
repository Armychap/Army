namespace SmartHomeBridge;

// Реализация моста: способ доставки команд до «железа» (локально, облако, голос).
public interface IControlImplementor
{
    //Тип устройства (light, thermostat, door), действие (on, off, set_temp, lock…)
    //Доп. данные (температура, имя комнаты).</param>
    void Transmit(string deviceKind, string action, string? parameter = null);
}
