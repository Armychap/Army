using System;
using ArmyBattle.Models;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Singleton для расчёта урона между юнитами.
    /// </summary>
    public sealed class DamageService
    {
        public static DamageService Instance { get; } = new DamageService();

        private DamageService() { }

        public void ResolveAttack(IUnit attacker, IUnit defender)
        {
            if (attacker == null || defender == null)
                return;

            defender.TakeDamage(attacker.EffectiveAttack, attacker.Name);
            attacker.DamageDealt += Math.Max(1, attacker.EffectiveAttack - defender.EffectiveDefence);
        }
    }
}
