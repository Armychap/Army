using System;
using System.IO;
using System.Media;

class TestSound
{
    static void Main()
    {
        try
        {
            if (File.Exists("death_sound.wav"))
            {
                Console.WriteLine("Файл death_sound.wav найден");
                var player = new SoundPlayer("death_sound.wav");
                player.Load();
                Console.WriteLine("Звук загружен, воспроизводим...");
                player.Play();
                Console.WriteLine("Звук воспроизведен");
            }
            else
            {
                Console.WriteLine("Файл death_sound.wav не найден");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}