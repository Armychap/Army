namespace GameFacade;

/// <summary>Подсистема: уровни и прогресс забега.</summary>
public class LevelManager
{
    public int CurrentLevel { get; private set; }
    public int RunScore { get; private set; }

    public void LoadLevel(int levelNumber)
    {
        CurrentLevel = levelNumber;
    }

    public void AddRunPoints(int points) => RunScore += points;

    public void SimulateLevelProgress()
    {
        AddRunPoints(Random.Shared.Next(50, 200));
    }

    public void ResetRun()
    {
        CurrentLevel = 0;
        RunScore = 0;
    }
}
