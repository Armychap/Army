namespace TaxiMediator
{
    /// <summary>
    /// Базовый абстрактный класс для всех участников системы
    /// </summary>
    public abstract class TaxiParticipant
    {
        protected ITaxiMediator Mediator;
        public string Name { get; }

        protected TaxiParticipant(ITaxiMediator mediator, string name)
        {
            Mediator = mediator;
            Name = name;
        }
    }
}