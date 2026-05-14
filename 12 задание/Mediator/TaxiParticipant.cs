namespace TaxiMediator
{
    // Базовый абстрактный класс для всех участников системы
    public abstract class TaxiParticipant
    {
        protected ITaxiMediator Mediator; // Ссылка на посредника (диспетчера)
        public string Name { get; }

        protected TaxiParticipant(ITaxiMediator mediator, string name)
        {
            Mediator = mediator;
            Name = name;
        }
    }
}