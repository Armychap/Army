using System;

namespace LazyInitializationReports
{
    
    // лениво создаёт DataWarehouse только при первом запросе

    internal class Program
    {
        static void Main(string[] args)
        {
            // Сообщение для пользователя о назначении примера
            Console.WriteLine("Lazy Initialization — генерация отчётов (собственный Lazy<T>)\n");

            // Создаём генератор, но пока не обращаемся к его warehouse
            var generator = new ReportGenerator();

            // Поведение lazy: при создании ReportGenerator экземпляр DataWarehouse ещё не был создан.
            Console.WriteLine($"Хранилище инициализировано? {generator.IsWarehouseInitialized}");
            Console.WriteLine("Создали ReportGenerator, но DataWarehouse ещё НЕ создавался.\n");

            Console.WriteLine("1) Ничего не генерируем — тяжёлого подключения нет.");
            Console.WriteLine($"Хранилище инициализировано? {generator.IsWarehouseInitialized}\n");

            var today = DateOnly.FromDateTime(DateTime.Now);

            Console.WriteLine("2) Генерируем Daily отчёт — тут впервые потребуется DataWarehouse.");
            // Первый вызов Generate приводит к обращению generator._warehouse.Value,
            // что инициализирует DataWarehouse лениво.
            var daily = generator.Generate(ReportType.Daily, today);
            Print(daily);
            Console.WriteLine($"Хранилище инициализировано? {generator.IsWarehouseInitialized}\n");

            Console.WriteLine("3) Генерируем Weekly отчёт — DataWarehouse уже готов, повторного подключения нет.");
            var weekly = generator.Generate(ReportType.Weekly, today);
            Print(weekly);

            Console.WriteLine("4) Сбрасываем хранилище с помощью Reset — теперь оно не инициализировано.");
            generator.ResetWarehouse();
            Console.WriteLine($"Хранилище инициализировано? {generator.IsWarehouseInitialized}\n");

            Console.WriteLine("5) Генерируем Monthly отчёт — DataWarehouse создастся заново после сброса.");
            var monthly = generator.Generate(ReportType.Monthly, today);
            Print(monthly);
            Console.WriteLine($"Хранилище инициализировано? {generator.IsWarehouseInitialized}");

            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

        
        // Выводит информационный отчет в консоль.
        
        private static void Print(Report report)
        {
            Console.WriteLine($"\n[REPORT] {report.Type} за {report.Date:yyyy-MM-dd}");
            Console.WriteLine($"Сгенерирован за: {report.GenerationTime.TotalMilliseconds:0}мс");
            foreach (var row in report.Rows)
            {
                Console.WriteLine($"- {row.Metric}: {row.Value}");
            }
            Console.WriteLine();
        }
    }
}

