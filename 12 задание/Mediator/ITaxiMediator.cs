using System;

namespace TaxiMediator
{
    // Интерфейс диспетчерской службы (Mediator)
    public interface ITaxiMediator
{
    void RegisterDriver(Driver driver);           // добавить водителя
    void RegisterPassenger(Passenger passenger);  // добавить пассажира
    void RequestTaxi(Passenger passenger, string pickupLocation, string destination);
    void AcceptOrder(Driver driver, Guid orderId);
    void NotifyDriverAvailable(Driver driver);    // водитель освободился
}
}