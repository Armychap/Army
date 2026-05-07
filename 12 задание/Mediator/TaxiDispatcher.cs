using System;
using System.Collections.Generic;
using System.Linq;

namespace TaxiMediator
{
    /// <summary>
    /// Диспетчерская служба такси (конкретный Mediator)
    /// </summary>
    public class TaxiDispatcher : ITaxiMediator
    {
        private List<Driver> _drivers = new List<Driver>();
        private List<Passenger> _passengers = new List<Passenger>();
        private Queue<Order> _pendingOrders = new Queue<Order>();
        private List<Order> _completedOrders = new List<Order>();

        public void RegisterDriver(Driver driver)
        {
            _drivers.Add(driver);
            Console.WriteLine($"[Диспетчер] Водитель {driver.Name} ({driver.CarModel}) зарегистрирован");
        }

        public void RegisterPassenger(Passenger passenger)
        {
            _passengers.Add(passenger);
            Console.WriteLine($"[Диспетчер] Пассажир {passenger.Name} зарегистрирован");
        }

        public void RequestTaxi(Passenger passenger, string pickupLocation, string destination)
        {
            // Создаем новую заявку
            var order = new Order(passenger, pickupLocation, destination);
            Console.WriteLine($"[Диспетчер] Новая заявка: {order}");

            // Ищем свободного водителя
            var availableDriver = _drivers.FirstOrDefault(d => d.IsAvailable);
            
            if (availableDriver != null)
            {
                // Назначаем заказ сразу
                AssignOrderToDriver(availableDriver, order);
            }
            else
            {
                // Ставим в очередь ожидания
                _pendingOrders.Enqueue(order);
                Console.WriteLine($"[Диспетчер] Нет свободных водителей. Заявка в очереди. Очередь: {_pendingOrders.Count}");
            }
        }

        public void AcceptOrder(Driver driver, Guid orderId)
        {
            var pendingOrder = _pendingOrders.FirstOrDefault(o => o.Id == orderId);
            if (pendingOrder != null)
            {
                _pendingOrders = new Queue<Order>(_pendingOrders.Where(o => o.Id != orderId));
                AssignOrderToDriver(driver, pendingOrder);
            }
            else
            {
                Console.WriteLine($"[Диспетчер] Заказ #{orderId.ToString().Substring(0, 8)} не найден");
            }
        }

        public void NotifyDriverAvailable(Driver driver)
        {
            Console.WriteLine($"[Диспетчер] Водитель {driver.Name} освободился");
            TryProcessNextOrder();
        }

        private void AssignOrderToDriver(Driver driver, Order order)
        {
            if (!driver.IsAvailable)
            {
                Console.WriteLine($"[Диспетчер] Водитель {driver.Name} недоступен. Заказ возвращается в очередь");
                _pendingOrders.Enqueue(order);
                return;
            }

            Console.WriteLine($"[Диспетчер] Назначаю {order} водителю {driver.Name}");
            driver.AcceptOrder(order.Id, order.Passenger, order.PickupLocation);
            order.Passenger.ReceiveTaxiArrived(driver);
            order.IsCompleted = true;
            _completedOrders.Add(order);

            // Статистика
            Console.WriteLine($"[Диспетчер] Выполнено заказов: {_completedOrders.Count}, в очереди: {_pendingOrders.Count}");
        }

        private void TryProcessNextOrder()
        {
            if (_pendingOrders.Count > 0)
            {
                var nextOrder = _pendingOrders.Peek();
                var freeDriver = _drivers.FirstOrDefault(d => d.IsAvailable);
                
                if (freeDriver != null)
                {
                    _pendingOrders.Dequeue();
                    AssignOrderToDriver(freeDriver, nextOrder);
                }
            }
        }

        public void ShowStatistics()
        {
            Console.WriteLine("\n=== СТАТИСТИКА ДИСПЕТЧЕРСКОЙ СЛУЖБЫ ===");
            Console.WriteLine($"Всего водителей: {_drivers.Count}");
            Console.WriteLine($"Свободных водителей: {_drivers.Count(d => d.IsAvailable)}");
            Console.WriteLine($"Всего пассажиров: {_passengers.Count}");
            Console.WriteLine($"Выполнено заказов: {_completedOrders.Count}");
            Console.WriteLine($"Заказов в очереди: {_pendingOrders.Count}");
        }
    }
}