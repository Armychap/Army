namespace RobotBuilder;

// класс продукта - робот
public class Robot
{
    private readonly List<string> _components = new();
    private string _name = "Безымянный";

    public void SetName(string name) => _name = name;

    // добавление частей робота
    public void AddHead(string headType) => _components.Add($"Голова: {headType}");
    public void AddBody(string bodyType) => _components.Add($"Корпус: {bodyType}");
    public void AddArms(string armsType) => _components.Add($"Руки: {armsType}");
    public void AddLegs(string legsType) => _components.Add($"Ноги: {legsType}");
    public void AddWeapon(string weaponType) => _components.Add($"Оружие: {weaponType}");
    public void AddSpecialEquipment(string equipment) => _components.Add($"Спец. оснащение: {equipment}");

    // демонстрация компонентов
    public void ShowComponents()
    {
        Console.WriteLine($"\nРобот \"{_name}\" собран. Компоненты:");
        if (_components.Count == 0)
        {
            Console.WriteLine(" Не добавлено ни одной детали!! ");
        }
        else
        {
            foreach (var component in _components)
            {
                Console.WriteLine($" - {component}");
            }
        }
        Console.WriteLine("");
    }

    public void Clear()
    {
        _components.Clear();
        _name = "Безымянный";
    }
}