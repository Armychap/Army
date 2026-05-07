using System;

namespace TaxiMediator
{
    /// <summary>
    /// Класс заявки на такси
    /// </summary>
    public class Order
    {
        public Guid Id { get; }
        public Passenger Passenger { get; }
        public string PickupLocation { get; }
        public string Destination { get; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; }

        public Order(Passenger passenger, string pickupLocation, string destination)
        {
            Id = Guid.NewGuid();
            Passenger = passenger;
            PickupLocation = pickupLocation;
            Destination = destination;
            IsCompleted = false;
            CreatedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return $"Заказ #{Id.ToString().Substring(0, 8)} | От: {PickupLocation} | До: {Destination}";
        }
    }
}