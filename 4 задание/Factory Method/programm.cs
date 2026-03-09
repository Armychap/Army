using System;

// Интерфейс продукта
// ITransport — общий интерфейс для всех продуктов
// Конкретные классы реализуют этот интерфейс со своей логикой
public interface ITransport
{
    decimal CalculateCost(decimal distance);
    double CalculateTime(double distance);
    void Deliver(string cargo, decimal distance); //груз и расстояние
}

// Грузовик
public class Truck : ITransport
{
    private readonly decimal _baseRatePerKm = 10m; // стоимость за 1 км
    private readonly double _baseSpeed = 60; // скорость

    public decimal CalculateCost(decimal distance)
    {
        decimal cost = distance * _baseRatePerKm;
        
        if (distance > 1000m)
        {
            cost += 5000m; // надбавка
        }
        
        return cost;
    }

    public double CalculateTime(double distance)
    {
        double drivingTime = distance / _baseSpeed;
        int stops = (int)(distance / 400); // остановка каждые 400 км
        double stopTime = stops * 1.0; // 1 час на остановку
        
        return drivingTime + stopTime;
    }

    public void Deliver(string cargo, decimal distance)
    {
        Console.WriteLine($"Грузовик доставляет {cargo}");
        Console.WriteLine($"Расстояние: {distance} км");
        Console.WriteLine($"Стоимость: {CalculateCost(distance)} руб");
        Console.WriteLine($"Время: {CalculateTime((double)distance):F1} ч\n");
    }
}

// Поезд
public class Train : ITransport
{
    private readonly decimal _baseRatePerKm = 5m; // стоимость за 1 км
    private readonly double _baseSpeed = 80; // скорость

    public decimal CalculateCost(decimal distance)
    {
        decimal cost = distance * _baseRatePerKm;
        
        if (distance > 1500m)
        {
            cost *= 0.85m; // скидка 15
        }
        else if (distance > 800m)
        {
            cost *= 0.9m; // скидка 10
        }
        
        return cost;
    }

    public double CalculateTime(double distance)
    {
        double drivingTime = distance / _baseSpeed;
        int stops = (int)(distance / 150); // остановка каждые 150 км
        double stopTime = stops * 0.3; // 0.3 часа остановка
        
        return drivingTime + stopTime;
    }

    public void Deliver(string cargo, decimal distance)
    {
        Console.WriteLine($"Поезд доставляет {cargo}");
        Console.WriteLine($"Расстояние: {distance} км");
        Console.WriteLine($"Стоимость: {CalculateCost(distance)} руб");
        Console.WriteLine($"Время: {CalculateTime((double)distance):F1} ч\n");
    }
}

// Самолёт
public class Plane : ITransport
{
    private readonly decimal _baseRatePerKm = 50m; // стоимость за 1 км
    private readonly double _baseSpeed = 800; // скорость

    public decimal CalculateCost(decimal distance)
    {
        decimal fixedFee = 20000m; // фиксй сбор
        decimal variableCost = distance * _baseRatePerKm;
        
        if (distance > 2000m)
        {
            variableCost *= 1.2m; // надбавка 20
        }
        
        return fixedFee + variableCost;
    }

    public double CalculateTime(double distance)
    {
        double flightTime = distance / _baseSpeed;
        double groundTime = 1.5; // 1.5 часа на взлёт, посадку
        
        return flightTime + groundTime;
    }

    public void Deliver(string cargo, decimal distance)
    {
        Console.WriteLine($"Самолёт доставляет {cargo}");
        Console.WriteLine($"Расстояние: {distance} км");
        Console.WriteLine($"Стоимость: {CalculateCost(distance)} руб");
        Console.WriteLine($"Время: {CalculateTime((double)distance):F1} ч\n");
    }
}

// Корабль
public class Ship : ITransport
{
    private readonly decimal _baseRatePerKm = 2m; // стоимость за 1 км
    private readonly double _baseSpeed = 30; // скорость

    public decimal CalculateCost(decimal distance)
    {
        decimal portFee = 15000m; // фикс сбор
        decimal transportCost = distance * _baseRatePerKm;
        
        decimal channelFee = 0m;
        if (distance > 3000m)
        {
            channelFee = 5000m; // за проход через каналы
        }
        
        return portFee + transportCost + channelFee;
    }

    public double CalculateTime(double distance)
    {
        double sailingTime = distance / _baseSpeed;
        
        // задержки из-за погоды (10% времени)
        double weatherDelay = sailingTime * 0.1;
        
        return sailingTime + weatherDelay;
    }

    public void Deliver(string cargo, decimal distance)
    {
        Console.WriteLine($"Корабль доставляет {cargo}");
        Console.WriteLine($"Расстояние: {distance} км");
        Console.WriteLine($"Стоимость: {CalculateCost(distance)} руб");
        Console.WriteLine($"Время: {CalculateTime((double)distance):F1} ч\n");
    }
}

//CreateTransport() — фабричный метод
//Он абстрактный — потомки обязаны его реализовать
//Код в PlanDelivery не знает, какой конкретно транспорт будет создан
public abstract class Logistics
{
    public abstract ITransport CreateTransport();  // Фабричный метод

    public void PlanDelivery(string cargo, decimal distance)
    {
        ITransport transport = CreateTransport(); // Вызов фабричного метода
        transport.Deliver(cargo, distance);
    }
}

// переопределяем фабричный метод и создаём конкретный тип транспорта

public class RoadLogistics : Logistics
{
    public override ITransport CreateTransport() => new Truck(); // грузовик
}

public class RailLogistics : Logistics
{
    public override ITransport CreateTransport() => new Train(); // поезд
}

public class AirLogistics : Logistics
{
    public override ITransport CreateTransport() => new Plane(); //самолет
}

public class SeaLogistics : Logistics
{
    public override ITransport CreateTransport() => new Ship(); //корабль
}


class Program
{
    // получает Logistics и вызывает PlanDelivery
    // внутри автоматически срабатывает нужный фабричный метод
    static void TestDelivery(Logistics logistics, string cargo, decimal distance)
    {
        logistics.PlanDelivery(cargo, distance);
    }
    
    static void Main()
    {
        Console.WriteLine("Транспортная логистика\n");

        TestDelivery(new RoadLogistics(), "стройматериалы", 1200m);
        TestDelivery(new RailLogistics(), "уголь", 2500m);
        TestDelivery(new AirLogistics(), "почта", 3000m);
        TestDelivery(new SeaLogistics(), "сумки биркин", 8000m);

        Console.ReadKey();
    }

    
}