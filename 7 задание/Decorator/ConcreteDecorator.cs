namespace RobotDecorator;

// Декоратор: броня
public class ArmorDecorator : RobotDecorator
{
    private string _armorType;

    public ArmorDecorator(IRobot robot, string armorType) : base(robot)
    {
        _armorType = armorType;
    }

    public override string GetDescription() => $"{_robot.GetDescription()} + {_armorType} броня";
    public override int GetPower() => _robot.GetPower() + 20;
}


// Декоратор: оружие
public class WeaponDecorator : RobotDecorator
{
    private string _weaponType;

    public WeaponDecorator(IRobot robot, string weaponType) : base(robot)
    {
        _weaponType = weaponType;
    }

    public override string GetDescription() => $"{_robot.GetDescription()} + {_weaponType}";
    public override int GetPower() => _robot.GetPower() + 30;
}


// Декоратор: спец. оснащение
public class SpecialEquipmentDecorator : RobotDecorator
{
    private string _equipment;

    public SpecialEquipmentDecorator(IRobot robot, string equipment) : base(robot)
    {
        _equipment = equipment;
    }

    public override string GetDescription() => $"{_robot.GetDescription()} + {_equipment}";
    public override int GetPower() => _robot.GetPower() + 15;
}


