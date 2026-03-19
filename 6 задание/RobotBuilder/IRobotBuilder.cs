namespace RobotBuilder;

public interface IRobotBuilder
{
    void BuildHead();
    void BuildBody();
    void BuildArms();
    void BuildLegs();
    void BuildWeapon();
    void BuildSpecial();
    Robot GetRobot();
    void Reset();
}