namespace TemplateMethod;

public sealed class Demo
{
    public void Run()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=== До Template Method (дублирование шага раунда) ===\n");
        ProgramBeforeTemplateMethod.Run();

        Console.WriteLine("\n=== После Template Method (единый шаблон PlayRound) ===\n");

        // Здесь используется Template Method: конкретные игры вызывают общий PlayRound().
        var rounds = new CardGameRound[]
        {
            new PokerRound(),
            new BlackjackRound(),
            new DurakRound()
        };

        foreach (var round in rounds)
        {
            round.PlayRound();
            Console.WriteLine(new string('-', 70));
        }
    }
}
