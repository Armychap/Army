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
    public partial class BattleEngine
    {
        private readonly IArmy army1;
        private readonly IArmy army2;
        private readonly Random random;
        private int battleSpeed;

        public int Army1AddedFightersCount { get; private set; }
        public int Army2AddedFightersCount { get; private set; }
        public int Army1BuffsAppliedCount { get; private set; }
        public int Army2BuffsAppliedCount { get; private set; }


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

        public void StartBattle()
        {
            try
            {
                Console.Clear();
            }
            catch { }

            Console.WriteLine("НАЧАЛО БИТВЫ");
            Console.WriteLine($"{army1.Name} против {army2.Name}");
            Console.WriteLine($"Бюджет каждой команды: {army1.TotalCost}");
            Console.WriteLine();
            Thread.Sleep(1000);

            InitializeBattle();
            DoAllMoves();
        }

        private void EndBattle()
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