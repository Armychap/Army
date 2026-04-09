namespace GameFacade;

/// <summary>Сценарий сравнения: сначала клиент без фасада, затем с фасадом.</summary>
public static class GameFacadeDemoRunner
{
    public static void Run(TextWriter output)
    {
        ProgramBeforeFacade.Run(output);
        output.WriteLine();
        ProgramAfterFacade.Run(output);
    }
}
