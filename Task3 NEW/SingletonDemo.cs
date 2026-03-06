using System;

namespace NotificationSystem
{
    public static class SingletonDemo
    {
        public static void ShowDemonstration()
        {
            Console.WriteLine("Получаем первый экземпляр...");
            NotificationManager instance1 = NotificationManager.Instance;
            Console.WriteLine($"instance1 получен");

            Console.WriteLine("\nПолучаем второй экземпляр...");
            NotificationManager instance2 = NotificationManager.Instance;
            Console.WriteLine($"instance2 получен");

            bool areEqual = instance1 == instance2;
            Console.WriteLine($"\ninstance1 == instance2? {areEqual}");
        }
    }
}