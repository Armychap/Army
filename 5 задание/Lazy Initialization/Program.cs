using System;
using System.Threading.Tasks;

namespace LazyInitializationExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Демонстрация паттерна Lazy Initialization (собственная реализация Lazy<T>)\n");

            var soldier1 = new Soldier("Боец 1");
            var soldier2 = new Soldier("Боец 2");

            Console.WriteLine("Пока ни один боец не обращался к тяжёлому ресурсу.");
            Console.WriteLine($"Soldier1.IsEquipmentLoaded = {soldier1.IsEquipmentLoaded}");
            Console.WriteLine($"Soldier2.IsEquipmentLoaded = {soldier2.IsEquipmentLoaded}");

            // первый боец певым запрашивает тяжёлый ресурс
            await soldier1.InspectAsync();

            Console.WriteLine("\nВторой боец обращается к своему ресурсу (создаётся свой экземпляр).");
            await soldier2.InspectAsync();

            Console.WriteLine("\nПовторный доступ к ресурсу не запускает инициализацию повторно.");
            await soldier1.InspectAsync();

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
