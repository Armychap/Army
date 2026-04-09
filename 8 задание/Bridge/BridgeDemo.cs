namespace SmartHomeBridge;

/// <summary>Демонстрация: одни и те же устройства с разными реализациями канала управления.</summary>
public static class BridgeDemo
{
    public static void Run(TextWriter output)
    {
        var local = new LocalControl(output);
        var cloud = new CloudControl(output);
        var voice = new VoiceControl(output);

        output.WriteLine("=== Bridge: умный дом — абстракция «устройство», реализация «канал» ===\n");

        output.WriteLine("Один светильник, три способа управления (локально → облако → голос):");
        var lamp = new Light("гостиная", local);
        lamp.TurnOn();
        lamp.ChangeImplementor(cloud);
        lamp.TurnOff();
        lamp.ChangeImplementor(voice);
        lamp.TurnOn();

        output.WriteLine("\nТермостат через голос и через локальную сеть:");
        var heat = new Thermostat("спальня", voice);
        heat.SetTemperature(22);
        heat.ChangeImplementor(local);
        heat.SetTemperature(21);

        output.WriteLine("\nЗамок: сначала облако, потом голос:");
        var door = new DoorLock("входная", cloud);
        door.Lock();
        door.ChangeImplementor(voice);
        door.Unlock();
    }
}
