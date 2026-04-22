// IFormationStrategy.cs
namespace ArmyBattle.Game.Formations
{
    /// <summary>
    /// Интерфейс стратегии боевого построения
    /// </summary>
    public interface IFormationStrategy
    {
        string Name { get; }
        void Initialize(BattleEngine battle);
        bool IsCombatActive(BattleEngine battle);
        void DisplayRoundHeader(BattleEngine battle, int round);
        void DisplayBattleOrder(BattleEngine battle);
        bool ProcessMove(BattleEngine battle);
        void Reinitialize(BattleEngine battle);
    }
}