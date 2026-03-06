using System;

// Интерфейсы семейства продуктов
// Это то, что делает Abstract Factory — создаёт семейство связанных объектов

public interface IHotelBooking
{
    string GetHotelName();
    decimal GetPrice();
}

public interface IFlightBooking
{
    string GetAirline();
    decimal GetPrice();
}

// Конкретные продукты
public class CheapHotel : IHotelBooking
{
    public string GetHotelName() => "Бюджетная гостиница";
    public decimal GetPrice() => 1500;
}

public class CheapFlight : IFlightBooking
{
    public string GetAirline() => "Лоукостер";
    public decimal GetPrice() => 3000;
}


public class LuxuryHotel : IHotelBooking
{
    public string GetHotelName() => "Пятизвёздочный отель";
    public decimal GetPrice() => 8000;
}

public class LuxuryFlight : IFlightBooking
{
    public string GetAirline() => "Бизнес-авиалинии";
    public decimal GetPrice() => 15000;
}

// Абстрактная фабрика (интерфейс для создания семейства)
public interface ITravelBookingFactory
{
    IHotelBooking CreateHotelBooking();   // метод для создания отеля
    IFlightBooking CreateFlightBooking(); // метод для создания авиабилета
}

// Конкретные фабрики (реализуют создание семейства)
public class CheapTravelFactory : ITravelBookingFactory
{
    public IHotelBooking CreateHotelBooking() => new CheapHotel();
    public IFlightBooking CreateFlightBooking() => new CheapFlight();
}

public class LuxuryTravelFactory : ITravelBookingFactory
{
    public IHotelBooking CreateHotelBooking() => new LuxuryHotel();
    public IFlightBooking CreateFlightBooking() => new LuxuryFlight();
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Абстрактная фабрика: Система бронирования\n");
        
        // Выбор фабрики в зависимости от бюджета
        Console.WriteLine("Выберите тип путешествия: 1 - Эконом, 2 - Вип");
        var choice = Console.ReadLine();
        
        // Создаём нужную конкретную фабрику (это единственное место, где знаем о конкретном классе)
        ITravelBookingFactory factory = choice == "1" 
            ? new CheapTravelFactory() 
            : new LuxuryTravelFactory();

        // Формирование бронирования через фабрику (не знаем, какие конкретно объекты создаются)
        var hotel = factory.CreateHotelBooking();
        var flight = factory.CreateFlightBooking();

        Console.WriteLine($"\nЗабронировано:");
        Console.WriteLine($"Отель: {hotel.GetHotelName()} - {hotel.GetPrice()} руб.");
        Console.WriteLine($"Авиабилет: {flight.GetAirline()} - {flight.GetPrice()} руб.");
        Console.WriteLine($"Итого: {hotel.GetPrice() + flight.GetPrice()} руб.");
        
        Console.ReadKey();
    }
}