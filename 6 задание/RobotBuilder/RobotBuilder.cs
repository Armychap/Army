namespace RobotBuilder;

// боевой робот
public class CombatRobotBuilder : IRobotBuilder
{
    private Robot _robot = new();

    public CombatRobotBuilder()
    {
        Reset();
    }

    public void Reset()
    {
        _robot = new Robot();
        _robot.SetName("Боевой робот");
    }

    public void BuildHead()
    {
        _robot.AddHead("Боевой процессор");
    }

    public void BuildBody()
    {
        _robot.AddBody("Титановая броня");
    }

    public void BuildArms()
    {
        _robot.AddArms("Гидравлические манипуляторы");
    }

    public void BuildLegs()
    {
        _robot.AddLegs("Гусеничная база");
    }

    public void BuildWeapon()
    {
        _robot.AddWeapon("Плазменная пушка");
    }

    public void BuildSpecial()
    {
        _robot.AddSpecialEquipment("Система ночного видения");
    }

    public Robot GetRobot()
    {
        return _robot;
    }
}

// рабочий робот
public class WorkerRobotBuilder : IRobotBuilder
{
    private Robot _robot = new();

    public WorkerRobotBuilder()
    {
        Reset();
    }

    public void Reset()
    {
        _robot = new Robot();
        _robot.SetName("Рабочий робот");
    }

    public void BuildHead()
    {
        _robot.AddHead("Стандартный процессор");
    }

    public void BuildBody()
    {
        _robot.AddBody("Легкий алюминиевый корпус");
    }

    public void BuildArms()
    {
        _robot.AddArms("Клешни и инструменты");
    }

    public void BuildLegs()
    {
        _robot.AddLegs("Двухколесная база");
    }

    public void BuildWeapon()
    {
        // нет оружия
    }

    public void BuildSpecial()
    {
        _robot.AddSpecialEquipment("Грузовой отсек");
    }

    public Robot GetRobot()
    {
        return _robot;
    }
}

// робот-перевозчик
public class CarrierRobotBuilder : IRobotBuilder
{
    private Robot _robot = new();

    public CarrierRobotBuilder()
    {
        Reset();
    }

    public void Reset()
    {
        _robot = new Robot();
        _robot.SetName("Робот-перевозчик");
    }

    public void BuildHead()
    {
        _robot.AddHead("Дорожный сканер");
    }

    public void BuildBody()
    {
        _robot.AddBody("Корпус с магнитом");
    }

    public void BuildArms()
    {
        _robot.AddArms("Поворотные краны");
    }

    public void BuildLegs()
    {
        _robot.AddLegs("Четырехколесная база");
    }

    public void BuildWeapon()
    {
        // нет оружия
    }

    public void BuildSpecial()
    {
        _robot.AddSpecialEquipment("Канаты со стропами");
    }

    public Robot GetRobot()
    {
        return _robot;
    }
}