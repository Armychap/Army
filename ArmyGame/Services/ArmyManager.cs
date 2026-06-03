using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArmyBattle.Models;
using ArmyBattle.Models.Interfaces;
using ArmyBattle.Models.Decorators;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Управляет сохранением и загрузкой состояния армий.
    /// SRP: отвечает ТОЛЬКО за логику сохранения/загрузки армий и сериализацию.
    /// Работа с файловой системой делегируется сервисам.
    /// </summary>
    public class ArmyManager
    {
        /// <summary>
        /// Сервис для работы с JSON файлами
        /// </summary>
        private readonly JsonStorageService _jsonStorageService;

        /// <summary>
        /// Конструктор инициализирует сервис работы с JSON
        /// </summary>
        public ArmyManager()
        {
            _jsonStorageService = new JsonStorageService("Saves");
        }

        /// <summary>
        /// Сохраняет две армии в JSON файл
        /// </summary>
        public void SaveArmies(IArmy army1, IArmy army2, string? saveName = null, int currentRound = 1, int attackTurn = 0, 
            bool firstAttackerIsArmy1 = false, bool needNewRoundHeader = true, string? battleLogName = null, 
            int moveCount = 0, FormationType currentFormation = FormationType.OneColumn)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(saveName))
                    saveName = $"Armies_{DateTime.Now:yyyyMMdd_HHmmss}";

                var saveData = SerializeArmies(army1, army2, currentRound, attackTurn, firstAttackerIsArmy1, 
                    needNewRoundHeader, battleLogName, moveCount, currentFormation);

                _jsonStorageService.SaveToJson(saveData, $"{saveName}.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка при сохранении: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает две армии из JSON файла
        /// </summary>
        public bool LoadArmies(string filePath, out IArmy? army1, out IArmy? army2, out int currentRound, out int attackTurn, 
            out bool firstAttackerIsArmy1, out bool needNewRoundHeader, out string? battleLogName, 
            out int moveCount, out FormationType currentFormation)
        {
            army1 = null;
            army2 = null;
            currentRound = 1;
            attackTurn = 0;
            firstAttackerIsArmy1 = false;
            needNewRoundHeader = true;
            battleLogName = null;
            moveCount = 0;
            currentFormation = FormationType.OneColumn;

            try
            {
                string fileName = FileSystemService.GetFileNameWithoutExtension(filePath);
                var saveData = _jsonStorageService.LoadFromJson<ArmySaveData>($"{fileName}.json");

                if (saveData == null)
                    return false;

                currentFormation = saveData.CurrentFormation;

                if (string.IsNullOrWhiteSpace(saveData.Army1Name) || string.IsNullOrWhiteSpace(saveData.Army2Name))
                    return false;

                army1 = new Army(saveData.Army1Name, (ConsoleColor)saveData.Army1Color);
                army2 = new Army(saveData.Army2Name, (ConsoleColor)saveData.Army2Color);

                if (saveData.Army1Units != null)
                    DeserializeUnits(saveData.Army1Units, army1);
                if (saveData.Army2Units != null)
                    DeserializeUnits(saveData.Army2Units, army2);

                RestoreBattleOrder(army1, saveData.Army1AliveOrder, saveData.Army1CurrentFighterIndex);
                RestoreBattleOrder(army2, saveData.Army2AliveOrder, saveData.Army2CurrentFighterIndex);

                currentRound = saveData.CurrentRound;
                attackTurn = saveData.AttackTurn;
                firstAttackerIsArmy1 = saveData.FirstAttackerIsArmy1;
                needNewRoundHeader = saveData.NeedNewRoundHeader;
                battleLogName = saveData.BattleLogName;
                moveCount = saveData.MoveCount;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получает список имен всех сохраненных армий
        /// </summary>
        public string[] GetSavedArmies()
        {
            return _jsonStorageService.GetJsonFileNames();
        }

        /// <summary>
        /// Получает список только незаконченных игр
        /// </summary>
        public string[] GetUnfinishedGames()
        {
            var allFiles = _jsonStorageService.GetJsonFileNames();
            var unfinished = new List<string>();

            foreach (var fileName in allFiles)
            {
                try
                {
                    var saveData = _jsonStorageService.LoadFromJson<ArmySaveData>($"{fileName}.json");

                    if (saveData != null && !string.IsNullOrWhiteSpace(saveData.Army1Name) && 
                        !string.IsNullOrWhiteSpace(saveData.Army2Name))
                    {
                        var army1 = new Army(saveData.Army1Name, (ConsoleColor)saveData.Army1Color);
                        var army2 = new Army(saveData.Army2Name, (ConsoleColor)saveData.Army2Color);

                        if (saveData.Army1Units != null)
                            DeserializeUnits(saveData.Army1Units, army1);
                        if (saveData.Army2Units != null)
                            DeserializeUnits(saveData.Army2Units, army2);

                        if (army1.HasAliveUnits() && army2.HasAliveUnits())
                            unfinished.Add(fileName);
                    }
                }
                catch
                {
                    // Пропускаем поврежденные файлы
                }
            }

            return unfinished.ToArray();
        }

        /// <summary>
        /// Формирует полный путь к файлу сохранения
        /// </summary>
        public string GetSavePath(string saveName)
        {
            return Path.Combine("Saves", FileSystemService.SanitizeFileName(saveName) + ".json");
        }

        /// <summary>
        /// Сериализует армии в сохраняемый формат
        /// </summary>
        public ArmySaveData SerializeArmies(IArmy army1, IArmy army2, int currentRound = 1, int attackTurn = 0, 
            bool firstAttackerIsArmy1 = false, bool needNewRoundHeader = true, string? battleLogName = null, 
            int moveCount = 0, FormationType currentFormation = FormationType.OneColumn)
        {
            return new ArmySaveData
            {
                Army1Name = army1.Name,
                Army1Color = (int)army1.Color,
                Army1Units = SerializeUnits(army1.Units),
                Army1AliveOrder = army1.AliveFightersInBattleOrder.Select(u => u.FighterNumber).ToList(),
                Army1CurrentFighterIndex = army1.CurrentFighterIndex,

                Army2Name = army2.Name,
                Army2Color = (int)army2.Color,
                Army2Units = SerializeUnits(army2.Units),
                Army2AliveOrder = army2.AliveFightersInBattleOrder.Select(u => u.FighterNumber).ToList(),
                Army2CurrentFighterIndex = army2.CurrentFighterIndex,

                CurrentRound = currentRound,
                AttackTurn = attackTurn,
                FirstAttackerIsArmy1 = firstAttackerIsArmy1,
                NeedNewRoundHeader = needNewRoundHeader,
                BattleLogName = battleLogName,
                MoveCount = moveCount,
                CurrentFormation = currentFormation
            };
        }

        /// <summary>
        /// Сериализует единицу юнита для сохранения
        /// </summary>
        private UnitSaveData SerializeUnit(IUnit unit)
        {
            var rootUnit = unit.GetRootUnit();

            return new UnitSaveData
            {
                FighterNumber = unit.FighterNumber,
                Type = rootUnit.GetType().Name,
                Health = unit.Health,
                AppliedBuffs = GetBuffNames(unit)
            };
        }

        /// <summary>
        /// Получает список названий баффов, надетых на юнита
        /// </summary>
        private List<string> GetBuffNames(IUnit unit)
        {
            var buffNames = new List<string>();

            while (unit is BuffDecorator decorator)
            {
                buffNames.Add(decorator.GetType().Name);
                unit = decorator.GetInnerUnit();
            }

            return buffNames;
        }

        /// <summary>
        /// Сериализует коллекцию юнитов
        /// </summary>
        private List<UnitSaveData> SerializeUnits(IEnumerable<IUnit> units)
        {
            return units.Select(SerializeUnit).ToList();
        }

        /// <summary>
        /// Десериализует юнитов и добавляет их в армию
        /// </summary>
        private void DeserializeUnits(List<UnitSaveData> unitDataList, IArmy army)
        {
            var factory = UnitFactoryProvider.Instance;

            foreach (var unitData in unitDataList)
            {
                // используется Абстрактная фабрика
                var unit = factory.CreateFromType(unitData.Type ?? "", unitData.FighterNumber);
                unit.Health = unitData.Health;

                // Применяем сохраненные баффы
                foreach (var buffName in unitData.AppliedBuffs ?? new List<string>())
                {
                    unit = ApplyBuffByName(unit, buffName);
                }

                army.AddUnit(unit);
            }
        }

        /// <summary>
        /// Применяет бафф по названию класса
        /// </summary>
        private IUnit ApplyBuffByName(IUnit unit, string buffName)
        {
            return buffName switch
            {
                nameof(HorseBuffDecorator) => new HorseBuffDecorator(unit),
                nameof(ShieldBuffDecorator) => new ShieldBuffDecorator(unit),
                nameof(HelmetBuffDecorator) => new HelmetBuffDecorator(unit),
                nameof(SpearBuffDecorator) => new SpearBuffDecorator(unit),
                _ => unit
            };
        }

        /// <summary>
        /// Восстанавливает порядок боя армии
        /// </summary>
        private void RestoreBattleOrder(IArmy army, List<int>? aliveOrder, int currentFighterIndex)
        {
            if (aliveOrder != null && aliveOrder.Count > 0)
            {
                var order = new List<IUnit>();
                foreach (var number in aliveOrder)
                {
                    var unit = army.Units.FirstOrDefault(u => u.FighterNumber == number && u.IsAlive);
                    if (unit != null) 
                        order.Add(unit);
                }
                army.AliveFightersInBattleOrder = order;
            }
            else
            {
                army.RefreshAliveFighters();
            }

            if (army.AliveFightersInBattleOrder.Count > 0)
                army.CurrentFighterIndex = Math.Min(currentFighterIndex, army.AliveFightersInBattleOrder.Count - 1);
        }
    }
}
