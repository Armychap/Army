using System;

namespace TaxiMediator
{
    // Водитель такси
    public class Driver : TaxiParticipant
    {
        public bool IsAvailable { get; private set; } = true; // доступность водителя
        public Guid CurrentOrderId { get; private set; } // текущий заказ 
        public string CarModel { get; } // модель машины
        public string LicensePlate { get; } // номер машины

        public Driver(ITaxiMediator mediator, string name, string carModel, string licensePlate)
            : base(mediator, name)
        {
            CarModel = carModel;
            LicensePlate = licensePlate;
        }

        // Водитель принимает заказ
        // В классе Driver
        public void AcceptOrder(Guid orderId, Passenger passenger, string pickupLocation)
        {
            if (!IsAvailable) return;

            IsAvailable = false;
            CurrentOrderId = orderId;
            Console.WriteLine($"[Водитель {Name}] Принял заказ. Еду на {pickupLocation}");

            // Запускаем выполнение в отдельном потоке или Task
            Task.Run(() => ExecuteOrder(orderId, passenger));
        }

        private async Task ExecuteOrder(Guid orderId, Passenger passenger)
        {
            // Имитируем поездку
            await Task.Delay(3000);

            Console.WriteLine($"[Водитель {Name}] Завершил поездку с {passenger.Name}");
            IsAvailable = true;
            CurrentOrderId = Guid.Empty;
            Mediator.NotifyDriverAvailable(this);
        }

        // Водитель завершает заказ
        private void CompleteOrder(Guid orderId, Passenger passenger)
        {
            Console.WriteLine($"[Водитель {Name}] Забрал {passenger.Name}. Выполняю заказ...");
            IsAvailable = true;
            CurrentOrderId = Guid.Empty;
            Console.WriteLine($"[Водитель {Name}] Заказ завершен. Теперь я свободен.");

            // Уведомляем диспетчера, что водитель свободен
            Mediator.NotifyDriverAvailable(this);
        }

        // Метод для установки доступности водителя
        public void SetAvailable(bool available)
        {
            IsAvailable = available;
        }
    }
}