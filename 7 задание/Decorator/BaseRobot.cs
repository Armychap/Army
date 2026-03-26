namespace RobotDecorator;

// Базовые модели роботов (конкретные компоненты)
public class CombatRobot : IRobot
{
    public string GetDescription() => "Боевой робот";
    public int GetPower() => 80;
    public void ShowInfo() => Console.WriteLine($"{GetDescription()} | Мощность: {GetPower()}");
}

public class WorkerRobot : IRobot
{
    public string GetDescription() => "Рабочий робот";
    public int GetPower() => 50;
    public void ShowInfo() => Console.WriteLine($"{GetDescription()} | Мощность: {GetPower()}");
}

public class ScoutRobot : IRobot
{
    public string GetDescription() => "Робот-разведчик";
    public int GetPower() => 40;
    public void ShowInfo() => Console.WriteLine($"{GetDescription()} | Мощность: {GetPower()}");
}