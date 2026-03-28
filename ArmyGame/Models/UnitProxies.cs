using System;
using System.IO;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Прокси-обертка для IUnit; позволяет добавлять поведение до/после делегирования.
    /// </summary>
    public abstract class UnitProxy : IUnit
    {
        protected IUnit inner;

        protected UnitProxy(IUnit inner)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public IUnit InnerUnit => inner;

        public string Name { get => inner.Name; set => inner.Name = value; }
        public int Attack { get => inner.Attack; set => inner.Attack = value; }
        public int Defence { get => inner.Defence; set => inner.Defence = value; }
        public int Health { get => inner.Health; set => inner.Health = value; }
        public int MaxHealth { get => inner.MaxHealth; set => inner.MaxHealth = value; }
        public int Cost { get => inner.Cost; set => inner.Cost = value; }
        public string PowerLevel { get => inner.PowerLevel; set => inner.PowerLevel = value; }
        public int DamageDealt { get => inner.DamageDealt; set => inner.DamageDealt = value; }
        public int FighterNumber { get => inner.FighterNumber; set => inner.FighterNumber = value; }
        public ISpecialAbility SpecialAbility { get => inner.SpecialAbility; set => inner.SpecialAbility = value; }
        public IArmy Army { get => inner.Army; set => inner.Army = value; }

        public bool IsAlive => inner.IsAlive;

        public virtual void TakeDamage(int damage, string attackerName)
        {
            inner.TakeDamage(damage, attackerName);
        }

        public virtual void AttackUnit(IUnit target)
        {
            inner.AttackUnit(target);
        }

        public virtual bool CanUseSpecialAbility(IUnit? target)
        {
            return inner.CanUseSpecialAbility(target);
        }

        public virtual void UseSpecialAbility(IUnit target)
        {
            inner.UseSpecialAbility(target);
        }

        public virtual string GetDisplayName(string prefix)
        {
            return inner.GetDisplayName(prefix);
        }

        public virtual bool CanBeCloned()
        {
            return inner.CanBeCloned();
        }

        public virtual bool CanBeHealed()
        {
            return inner.CanBeHealed();
        }
    }

    /// <summary>
    /// Прокси для записи урона в файл при каждом получении урона.
    /// </summary>
    public class DamageLogUnitProxy : UnitProxy
    {
        private static readonly string logDirectory = "Logs";
        private static readonly string logFile = Path.Combine(logDirectory, "unit_damage.log");

        public DamageLogUnitProxy(IUnit inner) : base(inner)
        {
            Directory.CreateDirectory(logDirectory);
        }

        public override void TakeDamage(int damage, string attackerName)
        {
            int healthBefore = inner.Health;
            inner.TakeDamage(damage, attackerName);
            int actualDamage = healthBefore - inner.Health;

            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {inner.Army?.Name ?? "Unknown Army"} {inner.GetDisplayName("")}: {actualDamage} урона от {attackerName}. HP {Math.Max(inner.Health,0)}/{inner.MaxHealth}";
            try
            {
                File.AppendAllText(logFile, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Если логирование рушит программу, хотя бы пропускаем
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[LOG ERROR] Не удалось записать лог: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    /// <summary>
    /// Прокси для сигнала (бип) при смерти юнита.
    /// </summary>
    public class DeathBeepUnitProxy : UnitProxy
    {
        public DeathBeepUnitProxy(IUnit inner) : base(inner)
        {
        }

        public override void TakeDamage(int damage, string attackerName)
        {
            bool wasAlive = inner.IsAlive;
            inner.TakeDamage(damage, attackerName);
            bool isDeadNow = !inner.IsAlive;

            if (wasAlive && isDeadNow)
            {
                Console.WriteLine("бииип");
            }
        }
    }
}
