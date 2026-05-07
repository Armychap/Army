using System;

namespace TaxiMediator
{
    /// <summary>
    /// Пассажир такси
    /// </summary>
    public class Passenger : TaxiParticipant
    {
        public string PhoneNumber { get; set; }
        
        public Passenger(ITaxiMediator mediator, string name, string phoneNumber = "") 
            : base(mediator, name)
        {
            PhoneNumber = phoneNumber;
        }

        public void RequestTaxi(string pickupLocation, string destination)
        {
            Console.WriteLine($"\n[Пассажир {Name}] Заказываю такси от {pickupLocation} до {destination}");
            Mediator.RequestTaxi(this, pickupLocation, destination);
        }

        public void ReceiveTaxiArrived(Driver driver)
        {
            Console.WriteLine($"[Пассажир {Name}] Водитель {driver.Name} ({driver.CarModel}) приехал. Сажусь в такси.");
        }

        public void RateTrip(int rating)
        {
            Console.WriteLine($"[Пассажир {Name}] Оценил поездку на {rating} звезд");
        }
    }
}