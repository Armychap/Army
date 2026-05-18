using System;
using System.Linq;
using System.Threading;
using ArmyBattle.Models;
using ArmyBattle.UI;
using ArmyBattle.Game;
using ArmyBattle.Models.Decorators;
using ArmyBattle.Services;

namespace ArmyBattle.Game
{
    /// <summary>
    /// Главный движок битвы. Управляет всеми аспектами боя между двумя армиями:
    /// инициализация, ходы, атаки, специальные способности, баффы, проверка патовых ситуаций.
    /// Реализован как partial класс с логикой разбита по отдельным файлам для удобства.
    /// </summary>
    public partial class BattleEngine
    {
        // Представление для отображения боя (UI)
        private ArmyBattle.UI.IBattleView? _view;

        public void SetView(ArmyBattle.UI.IBattleView view)
        {
            _view = view;
        }

        // Две сражающиеся армии
        private readonly IArmy army1;
        private readonly IArmy army2;
        // Генератор случайных чисел для выбора целей и действий
        private readonly Random random;
        // Скорость отображения боя в миллисекундах (100-1000 мс)
        private int battleSpeed;

        // Статистика битвы
        public int Army1AddedFightersCount { get; private set; }
        public int Army2AddedFightersCount { get; private set; }
        public int Army1BuffsAppliedCount { get; private set; }
        public int Army2BuffsAppliedCount { get; private set; }

        /// <summary>
        /// Инициализирует движок с двумя армиями и скоростью боя
        /// </summary>
        public BattleEngine(IArmy army1, IArmy army2, int speed = 400)
        {
            this.army1 = army1;
            this.army2 = army2;
            random = new Random();
            battleSpeed = Math.Max(100, Math.Min(1000, speed));

            Army1AddedFightersCount = 0;
            Army2AddedFightersCount = 0;
            Army1BuffsAppliedCount = 0;
            Army2BuffsAppliedCount = 0;
        }

        /// <summary>
        /// Запускает битву: показывает информацию о начале, инициализирует, выполняет все ходы
        /// </summary>
        public void StartBattle()
        {
            try { Console.Clear(); } catch { }

            if (_view != null)
            {
                _view.DisplayStart(army1.Name, army2.Name, army1.TotalCost);
            }
            else
            {
                Console.WriteLine("НАЧАЛО БИТВЫ");
                Console.WriteLine($"{army1.Name} против {army2.Name}");
                Console.WriteLine($"Бюджет каждой команды: {army1.TotalCost}");
                Console.WriteLine();
                Thread.Sleep(1000);
            }

            InitializeBattle();
            DoAllMoves();
        }

        /// <summary>
        /// Завершает битву: определяет победителя и выводит статистику
        /// </summary>
        private void EndBattle()
        {
            if (_view != null)
            {
                string winner = null;
                if (stalemateReached && army1.HasAliveUnits() && army2.HasAliveUnits())
                    winner = null;
                else if (army1.HasAliveUnits())
                    winner = army1.Name;
                else if (army2.HasAliveUnits())
                    winner = army2.Name;

                _view.DisplayWinner(winner, army1.HasAliveUnits() ? army1.Color : army2.Color);
                _view.DisplayStatistics(moveCount, army1.AliveCount(), army2.AliveCount(), Army1AddedFightersCount, Army2AddedFightersCount, Army1BuffsAppliedCount, Army2BuffsAppliedCount);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("БИТВА ЗАВЕРШЕНА");
                Console.WriteLine(new string('=', 40));

                bool army1Wins = army1.HasAliveUnits();
                bool army2Wins = army2.HasAliveUnits();

                if (stalemateReached && army1Wins && army2Wins)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("НИЧЬЯ!");
                    Console.ResetColor();
                }
                else if (army1Wins)
                {
                    Console.ForegroundColor = army1.Color;
                    Console.WriteLine($"ПОБЕДИТЕЛЬ: {army1.Name}!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = army2.Color;
                    Console.WriteLine($"ПОБЕДИТЕЛЬ: {army2.Name}!");
                    Console.ResetColor();
                }

                Console.WriteLine();
                Console.WriteLine($"Армия {army1.Name}: добавлено бойцов {Army1AddedFightersCount}, баффов надето {Army1BuffsAppliedCount}");
                Console.WriteLine($"Армия {army2.Name}: добавлено бойцов {Army2AddedFightersCount}, баффов надето {Army2BuffsAppliedCount}");
            }
        }
    }
}