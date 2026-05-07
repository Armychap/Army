using System;

namespace TaxiMediator
{
    /// <summary>
    /// Водитель такси
    /// </summary>
    public class Driver : TaxiParticipant
    {
        public bool IsAvailable { get; private set; } = true;
        public Guid CurrentOrderId { get; private set; }
        public string CarModel { get; }
        public string LicensePlate { get; }

        public Driver(ITaxiMediator mediator, string name, string carModel, string licensePlate) 
            : base(mediator, name)
        {
            CarModel = carModel;
            LicensePlate = licensePlate;
        }

        public void AcceptOrder(Guid orderId, Passenger passenger, string pickupLocation)
        {
            if (!IsAvailable)
            {
                Console.WriteLine($"[Водитель {Name}] Я занят, не могу принять заказ");
                return;
            }

            IsAvailable = false;
            CurrentOrderId = orderId;
            Console.WriteLine($"[Водитель {Name}] Принял заказ #{orderId.ToString().Substring(0, 8)} от пассажира {passenger.Name}");
            Console.WriteLine($"[Водитель {Name}] Еду на {pickupLocation} (машина: {CarModel}, номер: {LicensePlate})");
            
            // Имитируем выполнение заказа
            CompleteOrder(orderId, passenger);
        }

        private void CompleteOrder(Guid orderId, Passenger passenger)
        {
            Console.WriteLine($"[Водитель {Name}] Забрал {passenger.Name}. Выполняю заказ...");
            IsAvailable = true;
            CurrentOrderId = Guid.Empty;
            Console.WriteLine($"[Водитель {Name}] Заказ завершен. Теперь я свободен.");
            
            // Уведомляем диспетчера, что водитель свободен
            Mediator.NotifyDriverAvailable(this);
        }

        public void SetAvailable(bool available)
        {
            IsAvailable = available;
        }
    }
}