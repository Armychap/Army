namespace RobotDecorator;

// Абстрактный декоратор
public abstract class RobotDecorator : IRobot
{
    protected IRobot _robot;

    protected RobotDecorator(IRobot robot)
    {
        _robot = robot;
    }

    public virtual string GetDescription() => _robot.GetDescription();
    public virtual int GetPower() => _robot.GetPower();
    public virtual void ShowInfo()
    {
        Console.WriteLine($"{GetDescription()} | Мощность: {GetPower()}");
    }
}