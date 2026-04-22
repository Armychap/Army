using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ArmyBattle.Models;
using ArmyBattle.Services;

namespace ArmyBattle.Services
{
    /// <summary>
    /// сохранение/загрузка в JSON
    /// отвечает только за работу с файловой системой и сериализацией
    /// Зависит от интерфейса IArmy, а не от конкретной реализации (DIP)
    /// Основные обязанности:
    /// Сохранение состояния армий в JSON формат
    /// Загрузка армий из JSON файлов 
    /// Управление файловой структурой для сохранений
    /// Сериализация/десериализация юнитов
    /// </summary>
    public class ArmyManager
    {
        // Папка для хранения файлов сохранений
        private string savesDirectory = "Saves";

        /// <summary>
        /// Конструктор инициализирует директорию для сохранений.
        /// При создании экземпляра проверяет и создает папку "Saves" если её нет.
        /// </summary>
        public ArmyManager()
        {
            CreateDirectoriesIfNeeded();
        }

        /// <summary>
        /// Создает директорию для сохранений, если её еще нет в файловой системе.
        /// Вспомогательный метод для инициализации окружения.
        /// </summary>
        private void CreateDirectoriesIfNeeded()
        {
            // Проверяем наличие директории Saves
            if (!Directory.Exists(savesDirectory))
                // Если директория не существует создаем её
                Directory.CreateDirectory(savesDirectory);
        }

        /// <summary>
        /// Сохраняет две армии в JSON файл с опциональным названием.
        /// Если название не указано, использует текущую дату и время.
        /// Параметры:
        /// army1: первая армия для сохранения
        /// army2: вторая армия для сохранения
        /// saveName: опциональное название файла без расширения
        /// currentRound: текущий раунд битвы
        /// attackTurn: текущий ход атаки
        /// firstAttackerIsArmy1: кто первый атакующий
        /// needNewRoundHeader: нужен ли новый заголовок раунда
        /// battleLogName: имя файла лога битвы
        /// </summary>
        public void SaveArmies(IArmy army1, IArmy army2, string? saveName = null, int currentRound = 1, int attackTurn = 0, bool firstAttackerIsArmy1 = false, bool needNewRoundHeader = true, string? battleLogName = null, int moveCount = 0, FormationType currentFormation = FormationType.OneColumn)
        {
            try
            {
                // Если название не указано или пусто используем автогенерируемое имя с временем
                if (string.IsNullOrWhiteSpace(saveName))
                {
                    saveName = $"Armies_{DateTime.Now:yyyyMMdd_HHmmss}";
                }

                // Конвертируем армии в сериализуемый формат
                var saveData = SerializeArmies(army1, army2, currentRound, attackTurn, firstAttackerIsArmy1, needNewRoundHeader, battleLogName, moveCount, currentFormation);

                // Преобразуем объект в JSON строку с отступами для читаемости
                string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });

                // Формируем полный путь: Saves/[имя].json
                string filePath = Path.Combine(savesDirectory, $"{saveName}.json");

                // Записываем JSON в файл
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка при сохранении: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает две армии из JSON файла.
        /// Восстанавливает все параметры армий и их юнитов.
        /// Параметры:
        /// filePath: полный путь к файлу JSON
        /// army1: первая восстановленная армия (out параметр)
        /// army2: вторая восстановленная армия (out параметр)
        /// currentRound: текущий раунд (out)
        /// attackTurn: ход атаки (out)
        /// firstAttackerIsArmy1: первый атакующий (out)
        /// needNewRoundHeader: заголовок раунда (out)
        /// Возвращает: true если загрузка успешна, false при ошибке
        /// </summary>
        public bool LoadArmies(string filePath, out IArmy? army1, out IArmy? army2, out int currentRound, out int attackTurn, out bool firstAttackerIsArmy1, out bool needNewRoundHeader, out string? battleLogName, out int moveCount, out FormationType currentFormation)
        {
            // Инициализируем выходные параметры нулевыми значениями
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
                // Читаем содержимое JSON файла
                string json = File.ReadAllText(filePath);

                // Десериализуем JSON в объект ArmySaveData
                var saveData = JsonSerializer.Deserialize<ArmySaveData>(json);

                // Проверяем что данные корректно загружены
                if (saveData == null)
                    return false;
                    
                currentFormation = saveData.CurrentFormation;

                // Создаем первую армию с восстановленными параметрами
                if (string.IsNullOrWhiteSpace(saveData.Army1Name))
                    return false;

                army1 = new Army(saveData.Army1Name, (ConsoleColor)saveData.Army1Color);

                // Создаем вторую армию с восстановленными параметрами
                if (string.IsNullOrWhiteSpace(saveData.Army2Name))
                    return false;

                army2 = new Army(saveData.Army2Name, (ConsoleColor)saveData.Army2Color);

                // Добавляем юнитов в первую армию из сохраненных данных
                if (saveData.Army1Units != null)
                    DeserializeUnits(saveData.Army1Units, army1);

                // Добавляем юнитов во вторую армию из сохраненных данных
                if (saveData.Army2Units != null)
                    DeserializeUnits(saveData.Army2Units, army2);

                // Восстанавливаем порядок и текущие индексы
                if (saveData.Army1AliveOrder != null && saveData.Army1AliveOrder.Count > 0)
                {
                    var order1 = new List<IUnit>();
                    foreach (var number in saveData.Army1AliveOrder)
                    {
                        var unit = army1.Units.FirstOrDefault(u => u.FighterNumber == number && u.IsAlive);
                        if (unit != null) order1.Add(unit);
                    }
                    army1.AliveFightersInBattleOrder = order1;
                }
                else
                {
                    army1.RefreshAliveFighters();
                }

                if (saveData.Army2AliveOrder != null && saveData.Army2AliveOrder.Count > 0)
                {
                    var order2 = new List<IUnit>();
                    foreach (var number in saveData.Army2AliveOrder)
                    {
                        var unit = army2.Units.FirstOrDefault(u => u.FighterNumber == number && u.IsAlive);
                        if (unit != null) order2.Add(unit);
                    }
                    army2.AliveFightersInBattleOrder = order2;
                }
                else
                {
                    army2.RefreshAliveFighters();
                }

                if (army1.AliveFightersInBattleOrder.Count > 0)
                {
                    // Если индекс выходит за границы (уже был равен Count), сбрасываем на 0
                    army1.CurrentFighterIndex = Math.Min(saveData.Army1CurrentFighterIndex, army1.AliveFightersInBattleOrder.Count - 1);
                    if (saveData.Army1CurrentFighterIndex >= army1.AliveFightersInBattleOrder.Count)
                        army1.CurrentFighterIndex = 0; // Новый раунд - начинаем с первого
                }

                if (army2.AliveFightersInBattleOrder.Count > 0)
                {
                    // Если индекс выходит за границы (уже был равен Count), сбрасываем на 0
                    army2.CurrentFighterIndex = Math.Min(saveData.Army2CurrentFighterIndex, army2.AliveFightersInBattleOrder.Count - 1);
                    if (saveData.Army2CurrentFighterIndex >= army2.AliveFightersInBattleOrder.Count)
                        army2.CurrentFighterIndex = 0; // Новый раунд - начинаем с первого
                }

                // Восстанавливаем состояние битвы
                currentRound = saveData.CurrentRound;
                attackTurn = saveData.AttackTurn;
                firstAttackerIsArmy1 = saveData.FirstAttackerIsArmy1;
                needNewRoundHeader = saveData.NeedNewRoundHeader;
                battleLogName = saveData.BattleLogName;
                moveCount = saveData.MoveCount;

                // Успешная загрузка
                return true;
            }
            catch (Exception ex)
            {
                // Выводим сообщение об ошибке и возвращаем false
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получает список имен всех сохраненных армий.
        /// Ищет все JSON файлы в папке Saves и возвращает их имена без расширения.
        /// Возвращает: массив строк с именами сохранений
        /// </summary>
        public string[] GetSavedArmies()
        {
            // Получаем все JSON файлы из папки Saves
            var files = Directory.GetFiles(savesDirectory, "*.json");

            // Создаем массив результатов такого же размера, как количество файлов
            var result = new string[files.Length];

            // Итерируемся по каждому файлу и извлекаем имя без расширения
            for (int i = 0; i < files.Length; i++)
            {
                // Path.GetFileNameWithoutExtension удаляет расширение .json
                result[i] = Path.GetFileNameWithoutExtension(files[i]);
            }

            // Возвращаем массив имен сохранений
            return result;
        }

        /// <summary>
        /// Получает список имен только незаконченных игр (где обе армии имеют живых юнитов).
        /// Возвращает: массив строк с именами незаконченных сохранений
        /// </summary>
        public string[] GetUnfinishedGames()
        {
            var allFiles = Directory.GetFiles(savesDirectory, "*.json");
            var unfinished = new List<string>();

            foreach (var file in allFiles)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var saveData = JsonSerializer.Deserialize<ArmySaveData>(json);

                    if (saveData != null && !string.IsNullOrWhiteSpace(saveData.Army1Name) && !string.IsNullOrWhiteSpace(saveData.Army2Name))
                    {
                        // Создаем временные армии для проверки
                        var army1 = new Army(saveData.Army1Name, (ConsoleColor)saveData.Army1Color);
                        var army2 = new Army(saveData.Army2Name, (ConsoleColor)saveData.Army2Color);

                        if (saveData.Army1Units != null)
                            DeserializeUnits(saveData.Army1Units, army1);
                        if (saveData.Army2Units != null)
                            DeserializeUnits(saveData.Army2Units, army2);

                        // Если обе армии имеют живых юнитов, добавляем в список
                        if (army1.HasAliveUnits() && army2.HasAliveUnits())
                        {
                            unfinished.Add(Path.GetFileNameWithoutExtension(file));
                        }
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
        /// Формирует полный путь к файлу сохранения по его названию.
        /// Параметры:
        /// saveName: название сохранения без расширения
        /// Возвращает: полный путь типа Saves/[name].json
        /// </summary>
        public string GetSavePath(string saveName)
        {
            // Объединяем путь: папка + имя файла + расширение
            return Path.Combine(savesDirectory, $"{saveName}.json");
        }

        /// <summary>
        /// Преобразует две армии в сериализуемый формат ArmySaveData.
        /// Экстрактирует все необходимые данные для сохранения в JSON.
        /// Параметры:
        /// army1: первая армия
        /// army2: вторая армия
        /// Возвращает: объект ArmySaveData готовый к JSON сериализации
        /// </summary>
        public ArmySaveData SerializeArmies(IArmy army1, IArmy army2, int currentRound = 1, int attackTurn = 0, bool firstAttackerIsArmy1 = false, bool needNewRoundHeader = true, string? battleLogName = null, int moveCount = 0, FormationType currentFormation = FormationType.OneColumn)
        {
            // Создаем новый объект для сохранения данных армий
            return new ArmySaveData
            {
                // === ПЕРВАЯ АРМИЯ ===
                // Сохраняем название первой армии
                Army1Name = army1.Name,

                // Сохраняем цвет для отображения первой армии в консоли
                Army1Color = (int)army1.Color,

                // Сериализуем список юнитов первой армии
                Army1Units = SerializeUnits(army1.Units),

                // Сохраняем общую стоимость всех юнитов первой армии
                TotalCost1 = army1.TotalCost,

                // === ВТОРАЯ АРМИЯ ===
                // Сохраняем название второй армии
                Army2Name = army2.Name,

                // Сохраняем цвет для отображения второй армии в консоли
                Army2Color = (int)army2.Color,

                // Сериализуем список юнитов второй армии
                Army2Units = SerializeUnits(army2.Units),

                // Сохраняем общую стоимость всех юнитов второй армии
                TotalCost2 = army2.TotalCost,

                CurrentFormation = currentFormation,

                // === МЕТАДАННЫЕ ===
                // Сохраняем дату и время сохранения для истории
                SaveDate = DateTime.Now,

                // Сохраняем состояние битвы
                CurrentRound = currentRound,
                AttackTurn = attackTurn,
                Army1CurrentFighterIndex = army1.CurrentFighterIndex,
                Army2CurrentFighterIndex = army2.CurrentFighterIndex,
                FirstAttackerIsArmy1 = firstAttackerIsArmy1,
                NeedNewRoundHeader = needNewRoundHeader,
                BattleLogName = battleLogName,
                MoveCount = moveCount,

                // Сохраняем реальный порядок жизни бойцов (для корректного продолжения)
                Army1AliveOrder = army1.AliveFightersInBattleOrder?.Select(u => u.FighterNumber).ToList(),
                Army2AliveOrder = army2.AliveFightersInBattleOrder?.Select(u => u.FighterNumber).ToList()
            };
        }

        /// <summary>
        /// Преобразует список юнитов в список сериализуемых объектов UnitSaveData.
        /// Экстрактирует основные характеристики каждого юнита для сохранения.
        /// Параметры:
        /// units: список юнитов для сериализации (List&lt;IUnit&gt;)
        /// Возвращает: список сериализуемых данных юнитов
        /// </summary>
        private List<UnitSaveData> SerializeUnits(List<IUnit> units)
        {
            try
            {
                // Создаем пустой список для результатов
                var result = new List<UnitSaveData>();

                // Итерируемся по каждому юниту в списке
                foreach (var unit in units)
                {
                    try
                    {
                        // Добавляем сохраненные данные юнита в результат
                        result.Add(new UnitSaveData
                        {
                            // Сохраняем тип юнита (имя класса: WeakFighter, Archer, StrongFighter)
                            // Используем GetRootType() чтобы сохранить реальный тип, не прокси
                            Type = unit.GetRootType().Name,

                            // Сохраняем номер боя для идентификации внутри армии
                            FighterNumber = unit.FighterNumber,

                            // Сохраняем текущее здоровье юнита (может быть повреждено в бою)
                            Health = unit.Health,

                            // Сохраняем параметр атаки юнита
                            Attack = unit.Attack,

                            // Сохраняем параметр защиты юнита
                            Defence = unit.Defence,

                            // Сохраняем стоимость юнита для подсчета бюджета
                            Cost = unit.Cost
                        });
                    }
                    catch (Exception unitEx)
                    {
                        Console.WriteLine($"\nОшибка при сериализации юнита: {unitEx.Message}");
                    }
                }

                // Возвращаем заполненный список сохраненных юнитов
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка в SerializeUnits: {ex.Message}");
                return new List<UnitSaveData>();
            }
        }

        /// <summary>
        /// Восстанавливает юнитов из сохраненных данных и добавляет их в армию.
        /// Создает нужные типы юнитов и восстанавливает их характеристики.
        /// Параметры:
        /// unitsData: список сохраненных данных юнитов
        /// army: армия, в которую нужно добавить восстановленные юниты
        /// </summary>
        private void DeserializeUnits(List<UnitSaveData> unitsData, IArmy army)
        {
            // Итерируемся по каждому сохраненному юниту
            foreach (var unitData in unitsData)
            {
                // Используем switch выражение для создания правильного типа юнита
                Unit unit = unitData.Type switch
                {
                    // Если тип был "WeakFighter" создаем слабого бойца
                    nameof(WeakFighter) => new WeakFighter(unitData.FighterNumber),

                    // Если тип был "Archer" создаем лучника
                    nameof(Archer) => new Archer(unitData.FighterNumber),

                    // Если тип был "StrongFighter" создаем сильного бойца
                    nameof(StrongFighter) => new StrongFighter(unitData.FighterNumber),

                    // Если тип был "Healer" создаем лекаря
                    nameof(Healer) => new Healer(unitData.FighterNumber),

                    // Если тип был "Wizard" создаем мага
                    nameof(Wizard) => new Wizard(unitData.FighterNumber),

                    // Если тип был "ShieldWall" создаем стену щитов
                    nameof(ShieldWall) => new ShieldWall(unitData.FighterNumber),

                    // Если тип неизвестен выбрасываем исключение с описанием ошибки
                    _ => throw new Exception($"Неизвестный тип юнита: {unitData.Type}")
                };

                // Восстанавливаем здоровье юнита из сохраненного значения
                // (это важно если юнит был поврежден в предыдущем бою)
                unit.Health = unitData.Health;

                army.AddUnit(unit);
            }
        }
    }
}

