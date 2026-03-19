namespace RobotBuilder;

public class RobotDirector
{
    private readonly IRobotBuilder _builder;

    public RobotDirector(IRobotBuilder builder)
    {
        _builder = builder;
    }

    // сборка
    public void ConstructFullRobot()
    {
        _builder.Reset();
        Console.WriteLine("Директор: запускаю полную сборку...");
        _builder.BuildHead();
        _builder.BuildBody();
        _builder.BuildArms();
        _builder.BuildLegs();
        _builder.BuildWeapon();
        _builder.BuildSpecial();
    }
}