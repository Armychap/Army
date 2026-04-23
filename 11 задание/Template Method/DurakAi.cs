namespace TemplateMethod;

// Небольшой AI для решения в Дураке: рисковать защитой или нет.
public sealed class DurakAi
{
    private readonly string _name;
    private readonly double _riskTolerance;

    public DurakAi(string name, double riskTolerance)
    {
        _name = name;
        _riskTolerance = riskTolerance;
    }

    public bool ShouldDefend(int highAttackPower)
    {
        // Чем выше риск и ниже сила атаки, тем охотнее защищается.
        return _riskTolerance * 10 >= highAttackPower;
    }

    public override string ToString() => _name;
}
