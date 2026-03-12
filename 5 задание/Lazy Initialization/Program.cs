using System;

namespace LazyInitializationReports
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Lazy Initialization — генерация отчётов (собственный Lazy<T>)\n");

            var generator = new ReportGenerator();

            Console.WriteLine($"Хранилище инициализировано? {generator.IsWarehouseInitialized}");
            Console.WriteLine("Создали ReportGenerator, но DataWarehouse ещё НЕ создавался.\n");

            Console.WriteLine("1) Ничего не генерируем — тяжёлого подключения нет.");
            Console.WriteLine($"Хранилище инициализировано? {generator.IsWarehouseInitialized}\n");

            var today = DateOnly.FromDateTime(DateTime.Now);

            Console.WriteLine("2) Генерируем Daily отчёт — тут впервые потребуется DataWarehouse.");
            var daily = generator.Generate(ReportType.Daily, today);
            Print(daily);
            Console.WriteLine($"Хранилище инициализировано? {generator.IsWarehouseInitialized}\n");

            Console.WriteLine("3) Генерируем Weekly отчёт — DataWarehouse уже готов, повторного подключения нет.");
            var weekly = generator.Generate(ReportType.Weekly, today);
            Print(weekly);

            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

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

