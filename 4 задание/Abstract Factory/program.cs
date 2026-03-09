using System;

// Интерфейсы семейства продуктов
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

// Абстрактная фабрика
public interface ITravelBookingFactory
{
    IHotelBooking CreateHotelBooking();
    IFlightBooking CreateFlightBooking();
}

// Конкретные фабрики
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

// ввод пользователя
public class TravelConsoleUI
{
    public void ShowHeader()
    {
        Console.WriteLine("Абстрактная фабрика: Система бронирования\n");
    }
    
    public int GetUserChoice()
    {
        Console.WriteLine("Выберите тип путешествия: 1 - Эконом, 2 - Вип");
        return Console.ReadLine() == "1" ? 1 : 2;
    }
    
    public void ShowBooking(IHotelBooking hotel, IFlightBooking flight)
    {
        Console.WriteLine($"\nЗабронировано:");
        Console.WriteLine($"Отель: {hotel.GetHotelName()} - {hotel.GetPrice()} руб.");
        Console.WriteLine($"Авиабилет: {flight.GetAirline()} - {flight.GetPrice()} руб.");
        Console.WriteLine($"Итого: {hotel.GetPrice() + flight.GetPrice()} руб.");
    }
    
    public void WaitForKey()
    {
        Console.ReadKey();
    }
}

// создание 
public class TravelFactoryCreator
{
    public ITravelBookingFactory CreateFactory(int choice)
    {
        return choice == 1 
            ? new CheapTravelFactory() 
            : new LuxuryTravelFactory();
    }
}

class Program
{
    static void Main()
    {
        // Создаём вспомогательные классы
        var ui = new TravelConsoleUI();
        var factoryCreator = new TravelFactoryCreator();
        
        // заголовок
        ui.ShowHeader();
        
        // выбор пользователя
        int choice = ui.GetUserChoice();
        
        // Создаём нужную фабрику
        ITravelBookingFactory factory = factoryCreator.CreateFactory(choice);
        
        // Создаём бронирование
        var hotel = factory.CreateHotelBooking();
        var flight = factory.CreateFlightBooking();
        
        // Показываем результат
        ui.ShowBooking(hotel, flight);
        ui.WaitForKey();
    }
}