using System;
using System.Linq;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Специальная способность мага: с небольшой вероятностью клонирует случайного союзника (только легкого бойца или лучника).
    /// Клон вставляется в армию перед магом (в любом месте перед ним).
    /// </summary>
    public class CloneAbility : ISpecialAbility
    {
        private static readonly Random random = new Random();
        public string Name { get; set; }
        public int Range { get; set; }
        public int Power { get; set; } // Шанс в процентах

        // Добавлено для отображения выбранного бойца
        public IUnit? ChosenToClone { get; private set; }

        public CloneAbility(string name, int range, int power)
        {
            Name = name;
            Range = range;
            Power = power;
        }

        public void Execute(IUnit user, IUnit? target)
        {
            if (user == null || user.Army == null)
                return;

            // Сбрасываем предыдущий результат перед каждой попыткой
            ChosenToClone = null;

            // Шанс сработать
            if (random.Next(100) >= Power)
                return;

            var army = user.Army;

            // Выбираем случайного кандидата для клона (только слабый боец или лучник, не маг и не лекарь, и может быть клонирован)
            var candidates = army.Units
                .Where(u => u != user && (u is WeakFighter || u is Archer) && u.CanBeCloned())
                .ToList();

            if (candidates.Count == 0)
                return;

            var chosen = candidates[random.Next(candidates.Count)];
            ChosenToClone = chosen;

            // Создаем клон
            IUnit clone;
            int newFighterNumber = army.Units.Count + 1;

            if (chosen is Archer)
            {
                clone = new Archer(newFighterNumber);
            }
            else // WeakFighter
            {
                clone = new WeakFighter(newFighterNumber);
            }

            // Копируем текущее состояние здоровья
            clone.Health = chosen.Health;
            clone.MaxHealth = chosen.MaxHealth;

            // Добавляем клон в армию
            army.AddUnit(clone);

            // Устанавливаем позицию клона перед магом (в любом месте перед магом)
            int wizardIndex = army.Units.IndexOf(user);
            if (wizardIndex > 0)
            {
                // Вставляем в случайную позицию до мага
                int insertIndex = random.Next(0, wizardIndex);
                army.Units.Remove(clone);
                army.Units.Insert(insertIndex, clone);
            }

            // Если клон жив, добавляем в порядок боя перед магом
            if (clone.IsAlive)
            {
                int wizardBattleIndex = army.AliveFightersInBattleOrder.IndexOf(user);
                if (wizardBattleIndex >= 0)
                {
                    int insertIndex = random.Next(0, wizardBattleIndex + 1);
                    army.AliveFightersInBattleOrder.Insert(insertIndex, clone);

                    // Если вставили до текущего индекса, корректируем указатель, чтобы не пропустить ход
                    if (insertIndex <= army.CurrentFighterIndex)
                    {
                        army.CurrentFighterIndex++;
                    }
                }
            }
        }
    }
}
