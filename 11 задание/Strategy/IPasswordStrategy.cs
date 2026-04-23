namespace Strategy
{
    public interface IPasswordStrategy
    {
        string Generate(int length);
        string GetPolicyDescription();
        int GetMinLength();
        string GetName();
    }
}