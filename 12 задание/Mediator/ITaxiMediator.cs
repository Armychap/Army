using System;

namespace TaxiMediator
{
    /// <summary>
    /// Интерфейс диспетчерской службы (Mediator)
    /// </summary>
    public interface ITaxiMediator
    {
        void RegisterDriver(Driver driver);
        void RegisterPassenger(Passenger passenger);
        void RequestTaxi(Passenger passenger, string pickupLocation, string destination);
        void AcceptOrder(Driver driver, Guid orderId);
        void NotifyDriverAvailable(Driver driver);
    }
}