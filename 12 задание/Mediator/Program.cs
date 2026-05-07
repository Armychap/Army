using System;
using System.Threading;

namespace TaxiMediator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Создаем диспетчера (медиатора)
            var dispatcher = new TaxiDispatcher();

            // Регистрируем водителей
            var driver1 = new Driver(dispatcher, "Алексей", "Toyota Camry", "А123ВС");
            var driver2 = new Driver(dispatcher, "Дмитрий", "Hyundai Solaris", "В456ММ");
            var driver3 = new Driver(dispatcher, "Сергей", "Kia Rio", "С789НН");
            
            dispatcher.RegisterDriver(driver1);
            dispatcher.RegisterDriver(driver2);
            dispatcher.RegisterDriver(driver3);

            // Регистрируем пассажиров
            var passenger1 = new Passenger(dispatcher, "Иван", "+7-999-123-45-67");
            var passenger2 = new Passenger(dispatcher, "Мария", "+7-999-234-56-78");
            var passenger3 = new Passenger(dispatcher, "Петр", "+7-999-345-67-89");
            var passenger4 = new Passenger(dispatcher, "Елена", "+7-999-456-78-90");

            dispatcher.RegisterPassenger(passenger1);
            dispatcher.RegisterPassenger(passenger2);
            dispatcher.RegisterPassenger(passenger3);
            dispatcher.RegisterPassenger(passenger4);

            Console.WriteLine("\nНачало рабочего дня\n");

            // Пассажиры делают заказы
            passenger1.RequestTaxi("Улица Ленина, 10", "Ж/д Вокзал");
            Thread.Sleep(500);
            
            passenger2.RequestTaxi("Проспект Мира, 25", "Аэропорт");
            Thread.Sleep(500);
            
            passenger3.RequestTaxi("Школа №5", "Центральный парк");
            Thread.Sleep(500);
            
            passenger4.RequestTaxi("Бульвар Гагарина, 7", "ТРЦ 'Мега'");
            Thread.Sleep(500);
            
            // Дополнительный заказ, который попадет в очередь
            var passenger5 = new Passenger(dispatcher, "Анна", "+7-999-567-89-01");
            dispatcher.RegisterPassenger(passenger5);
            passenger5.RequestTaxi("Театральная площадь", "Торговый центр");

            Console.WriteLine("\nОжидаем выполнения заказов");
            Thread.Sleep(2000);

            // Показываем статистику
            dispatcher.ShowStatistics();

            // Оцениваем поездку
            passenger1.RateTrip(5);
            passenger2.RateTrip(4);

            Console.WriteLine("\nРабочий день завершен");
            Console.ReadKey();
        }
    }
}