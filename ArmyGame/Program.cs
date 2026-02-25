using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using ArmyBattle.Models;
using ArmyBattle.Game;

namespace ArmyBattle
{
    class Program
    {
        private static string savesDirectory = "Saves";
        private static string logsDirectory = "Logs";

        // последние созданные/загруженные армии
        private static Army _lastArmy1 = null;
        private static Army _lastArmy2 = null;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Битва Армий";

            // Создаем директории для сохранений и логов, если их нет
            CreateDirectories();

            bool exit = false;

            while (!exit)
            {
                ClearConsole();
                Console.WriteLine("          ГЛАВНОЕ МЕНЮ");
                Console.WriteLine("-------------------------------------");
                Console.WriteLine("1 - Автоматическая генерация армий");
                Console.WriteLine("2 - Ручное создание армий");
                Console.WriteLine("3 - Загрузить армии с диска");
                Console.WriteLine("4 - Сохранить армии на диск");
                Console.WriteLine("5 - Показать информацию об армиях");
                Console.WriteLine("6 - Просмотреть логи битв");
                Console.WriteLine("0 - Выход");
                Console.WriteLine("-------------------------------------");
                Console.Write("Выбор: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateRandomBattle();
                        break;

                    case "2":
                        ClearConsole();
                        CreateManualArmies();
                        break;

                    case "3":
                        ClearConsole();
                        LoadArmiesFromDisk();
                        break;

                    case "4":
                        ClearConsole();
                        SaveCurrentArmies();
                        break;

                    case "5":
                        ShowStoredArmiesInfo();
                        break;

                    case "6":
                        ShowBattleLogs();
                        break;

                    case "0":
                        exit = true;
                        break;

                    default:
                        Console.WriteLine("Неверный выбор!");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void CreateDirectories()
        {
            if (!Directory.Exists(savesDirectory))
                Directory.CreateDirectory(savesDirectory);

            if (!Directory.Exists(logsDirectory))
                Directory.CreateDirectory(logsDirectory);
        }

        static void ClearConsole()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
                // Игнорируем ошибку, если консоль недоступна
            }
        }


        static (string name1, string name2) GetArmyNames()
        {
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("        НАЗВАНИЯ АРМИЙ");
            Console.WriteLine("-------------------------------------");

            Console.Write("Введите название первой армии: ");
            string name1 = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name1))
            {
                name1 = "Красная Армия";
                Console.WriteLine($"Использовано название по умолчанию: {name1}");
            }

            Console.Write("Введите название второй армии: ");
            string name2 = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name2))
            {
                name2 = "Синяя Армия";
                Console.WriteLine($"Использовано название по умолчанию: {name2}");
            }

            return (name1, name2);
        }


        static void CreateManualArmies()
        {
            ClearConsole();

            // Ввод названий армий
            var (name1, name2) = GetArmyNames();

            // Создание армий
            _lastArmy1 = new Army(name1, "A", ConsoleColor.Red);
            _lastArmy2 = new Army(name2, "B", ConsoleColor.Blue);

            // Ручная настройка состава
            Console.WriteLine("\n-------------------------------------");
            Console.WriteLine("     НАСТРОЙКА ПЕРВОЙ АРМИИ");
            Console.WriteLine("-------------------------------------");
            SetupArmyManually(_lastArmy1);

            Console.WriteLine("\n-------------------------------------");
            Console.WriteLine("     НАСТРОЙКА ВТОРОЙ АРМИИ");
            Console.WriteLine("-------------------------------------");
            SetupArmyManually(_lastArmy2);

            // Показываем итоговый состав
            ClearConsole();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("        ИТОГОВЫЙ СОСТАВ АРМИЙ");
            Console.WriteLine("-------------------------------------");
            _lastArmy1.DisplayArmyInfo(true);
            Console.WriteLine();
            _lastArmy2.DisplayArmyInfo(true);

            Console.WriteLine("\nНажмите Enter для начала битвы или любую другую клавишу для сохранения и выхода...");
            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.Enter)
            {
                // Запуск битвы
                StartBattle(_lastArmy1, _lastArmy2);
            }
            else
            {
                // Сохранение армий
                SaveArmies(_lastArmy1, _lastArmy2);
            }
        }

        static void SetupArmyManually(Army army)
        {
            int fighterNumber = 1;
            int totalCost = 0;
            int maxBudget = 500; // Максимальный бюджет для ручной настройки

            while (true)
            {
                Console.WriteLine($"\nТекущий состав {army.Name}:");
                Console.WriteLine($"Всего бойцов: {army.Units.Count}");
                Console.WriteLine($"Потрачено: {totalCost}/{maxBudget} монет");
                Console.WriteLine("-------------------------------------");

                if (army.Units.Count > 0)
                {
                    foreach (var unit in army.Units)
                    {
                        Console.WriteLine($"  {unit.GetDisplayName(army.Prefix)} - {unit.Name} (Стоимость: {unit.Cost})");
                    }
                }

                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1 - Добавить слабого бойца (ATK 8, DEF 3, HP 25) - 15 монет");
                Console.WriteLine("2 - Добавить лучника (ATK 10, DEF 2, HP 18) - 25 монет");
                Console.WriteLine("3 - Добавить сильного бойца (ATK 18, DEF 10, HP 60) - 40 монет");
                Console.WriteLine("4 - Удалить последнего бойца");
                Console.WriteLine("5 - Завершить настройку");
                Console.Write("Выбор: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        if (totalCost + 15 <= maxBudget)
                        {
                            var fighter = new WeakFighter(fighterNumber++);
                            army.AddUnit(fighter);
                            totalCost += 15;
                            Console.WriteLine("Слабый боец добавлен!");
                        }
                        else
                        {
                            Console.WriteLine("Недостаточно бюджета!");
                        }
                        break;

                    case "2":
                        if (totalCost + 25 <= maxBudget)
                        {
                            var fighter = new Archer(fighterNumber++);
                            army.AddUnit(fighter);
                            totalCost += 25;
                            Console.WriteLine("Лучник добавлен!");
                        }
                        else
                        {
                            Console.WriteLine("Недостаточно бюджета!");
                        }
                        break;

                    case "3":
                        if (totalCost + 40 <= maxBudget)
                        {
                            var fighter = new StrongFighter(fighterNumber++);
                            army.AddUnit(fighter);
                            totalCost += 40;
                            Console.WriteLine("Сильный боец добавлен!");
                        }
                        else
                        {
                            Console.WriteLine("Недостаточно бюджета!");
                        }
                        break;

                    case "4":
                        if (army.Units.Count > 0)
                        {
                            var removed = army.Units[army.Units.Count - 1];
                            totalCost -= removed.Cost;
                            army.Units.RemoveAt(army.Units.Count - 1);
                            fighterNumber--;
                            Console.WriteLine($"Боец удален! Возвращено {removed.Cost} монет.");
                        }
                        else
                        {
                            Console.WriteLine("Нет бойцов для удаления!");
                        }
                        break;

                    case "5":
                        Console.WriteLine("Настройка завершена.");
                        return;

                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                ClearConsole();
            }
        }


        static void SaveCurrentArmies()
        {
            ClearConsole();

            if (_lastArmy1 == null || _lastArmy2 == null)
            {
                Console.WriteLine("Сначала создайте или загрузите армии через меню 1, 2 или 3!");
                Console.ReadKey();
                return;
            }

            SaveArmies(_lastArmy1, _lastArmy2);
        }

        static void SaveArmies(Army army1, Army army2)
        {
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("        СОХРАНЕНИЕ АРМИЙ");
            Console.WriteLine("-------------------------------------");

            Console.Write("Введите название для сохранения (без пробелов): ");
            string saveName = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(saveName))
            {
                saveName = $"Armies_{DateTime.Now:yyyyMMdd_HHmmss}";
            }

            var saveData = new ArmySaveData
            {
                Army1Name = army1.Name,
                Army1Prefix = army1.Prefix,
                Army1Color = army1.Color,
                Army1Units = SerializeUnits(army1.Units),

                Army2Name = army2.Name,
                Army2Prefix = army2.Prefix,
                Army2Color = army2.Color,
                Army2Units = SerializeUnits(army2.Units),

                SaveDate = DateTime.Now,
                TotalCost1 = army1.TotalCost,
                TotalCost2 = army2.TotalCost
            };

            string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
            string filePath = Path.Combine(savesDirectory, $"{saveName}.json");

            File.WriteAllText(filePath, json);
            Console.WriteLine($"Армии сохранены в файл: {filePath}");
            Console.ReadKey();
        }

        static List<UnitSaveData> SerializeUnits(List<Unit> units)
        {
            var result = new List<UnitSaveData>();
            foreach (var unit in units)
            {
                string type = unit.GetType().Name;
                result.Add(new UnitSaveData
                {
                    Type = type,
                    FighterNumber = unit.FighterNumber,
                    Health = unit.Health,
                    Attack = unit.Attack,
                    Defence = unit.Defence,
                    Cost = unit.Cost
                });
            }
            return result;
        }

        static void LoadArmiesFromDisk()
        {
            ClearConsole();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("        ЗАГРУЗКА АРМИЙ");
            Console.WriteLine("-------------------------------------");

            var files = Directory.GetFiles(savesDirectory, "*.json");

            if (files.Length == 0)
            {
                Console.WriteLine("Нет сохраненных армий!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Доступные сохранения:");
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = Path.GetFileNameWithoutExtension(files[i]);
                Console.WriteLine($"{i + 1}. {fileName}");
            }

            Console.Write("\nВыберите номер сохранения: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= files.Length)
            {
                string filePath = files[choice - 1];
                string json = File.ReadAllText(filePath);

                try
                {
                    var saveData = JsonSerializer.Deserialize<ArmySaveData>(json);

                    _lastArmy1 = new Army(saveData.Army1Name, saveData.Army1Prefix, saveData.Army1Color);
                    _lastArmy2 = new Army(saveData.Army2Name, saveData.Army2Prefix, saveData.Army2Color);

                    DeserializeUnits(saveData.Army1Units, _lastArmy1);
                    DeserializeUnits(saveData.Army2Units, _lastArmy2);

                    Console.WriteLine("\nАрмии успешно загружены!");
                    Console.WriteLine($"Первая армия: {_lastArmy1.Name} - {_lastArmy1.Units.Count} бойцов");
                    Console.WriteLine($"Вторая армия: {_lastArmy2.Name} - {_lastArmy2.Units.Count} бойцов");

                    Console.WriteLine("\nНажмите Enter для начала битвы или любую другую клавишу для возврата...");
                    var key = Console.ReadKey();

                    if (key.Key == ConsoleKey.Enter)
                    {
                        StartBattle(_lastArmy1, _lastArmy2);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                    Console.ReadKey();
                }
            }
        }

        static void DeserializeUnits(List<UnitSaveData> unitsData, Army army)
        {
            foreach (var unitData in unitsData)
            {
                Unit unit = unitData.Type switch
                {
                    nameof(WeakFighter) => new WeakFighter(unitData.FighterNumber),
                    nameof(Archer) => new Archer(unitData.FighterNumber),
                    nameof(StrongFighter) => new StrongFighter(unitData.FighterNumber),
                    _ => throw new Exception($"Неизвестный тип юнита: {unitData.Type}")
                };

                // Восстанавливаем состояние
                unit.Health = unitData.Health;

                army.AddUnit(unit);
            }
        }

        static bool TryGetStoredArmies(out Army army1, out Army army2)
        {
            army1 = _lastArmy1;
            army2 = _lastArmy2;
            return army1 != null && army2 != null;
        }


        static void ShowBattleLogs()
        {
            ClearConsole();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("        ЛОГИ БИТВ");
            Console.WriteLine("-------------------------------------");

            var files = Directory.GetFiles(logsDirectory, "*.txt");

            if (files.Length == 0)
            {
                Console.WriteLine("Нет сохраненных логов битв!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Доступные логи:");
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = Path.GetFileNameWithoutExtension(files[i]);
                Console.WriteLine($"{i + 1}. {fileName}");
            }

            Console.Write("\nВыберите номер лога для просмотра (0 - выход): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= files.Length)
            {
                string filePath = files[choice - 1];
                string content = File.ReadAllText(filePath);

                ClearConsole();
                Console.WriteLine($"Содержимое файла: {Path.GetFileName(filePath)}");
                Console.WriteLine("-------------------------------------");
                Console.WriteLine(content);
                Console.WriteLine("-------------------------------------");
                Console.WriteLine("\nНажмите любую клавишу для возврата...");
                Console.ReadKey();
            }
        }

        static void SaveBattleLog(string log, string battleName)
        {
            string fileName = $"{battleName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(logsDirectory, fileName);
            File.WriteAllText(filePath, log);
            Console.WriteLine($"Лог битвы сохранен в файл: {filePath}");
        }


        static void CreateRandomBattle()
        {
            ClearConsole();

            // Ввод названий армий
            var (name1, name2) = GetArmyNames();

            Console.Write("\nВведите бюджет для обеих армий (стандарт 200): ");
            if (!int.TryParse(Console.ReadLine(), out int budget))
            {
                budget = 200;
            }

            // Создание армий
            _lastArmy1 = new Army(name1, "A", ConsoleColor.Red);
            _lastArmy2 = new Army(name2, "B", ConsoleColor.Blue);

            // Генерация армий с одинаковым бюджетом
            _lastArmy1.GenerateArmyWithBudget(budget);
            _lastArmy2.GenerateArmyWithBudget(budget);

            StartBattle(_lastArmy1, _lastArmy2);
        }

        static void StartBattle(Army army1, Army army2)
        {
            ClearConsole();

            // Вывод информации об армиях перед битвой
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("        НАЧАЛО БИТВЫ");
            Console.WriteLine("-------------------------------------");

            army1.DisplayArmyInfo(true);
            Console.WriteLine();
            army2.DisplayArmyInfo(true);

            Console.WriteLine("\nНажмите Enter для начала битвы...");
            Console.ReadLine();

            // Сохраняем оригинальный вывод
            var originalOutput = Console.Out;

            // Создаем логгер для захвата вывода
            var logCapture = new StringWriter();

            // Создаем составной writer, который пишет и в консоль, и в лог
            var compositeWriter = new CompositeTextWriter(originalOutput, logCapture);
            Console.SetOut(compositeWriter);

            try
            {
                BattleEngine battle = new BattleEngine(army1, army2, 400);
                battle.StartBattle();

                // Возвращаем оригинальный вывод
                Console.SetOut(originalOutput);

                Console.WriteLine("\nСохранить лог битвы? (д/н): ");
                if (Console.ReadLine()?.ToLower() == "д")
                {
                    SaveBattleLog(logCapture.ToString(), $"{army1.Name}_vs_{army2.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.SetOut(originalOutput);
                Console.WriteLine($"Ошибка во время битвы: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }

        static void ShowStoredArmiesInfo()
        {
            ClearConsole();

            if (!TryGetStoredArmies(out Army army1, out Army army2))
            {
                Console.WriteLine("Нет загруженных армий для отображения!");
                Console.WriteLine("Сначала создайте или загрузите армии.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("-------------------------------------");
            army1.DisplayArmyInfo(true);
            Console.WriteLine();
            army2.DisplayArmyInfo(true);
            Console.WriteLine("-------------------------------------");

            Console.ReadKey();
        }
    }

    // вспомогательные классы
    public class ArmySaveData
    {
        public string Army1Name { get; set; }
        public string Army1Prefix { get; set; }
        public ConsoleColor Army1Color { get; set; }
        public List<UnitSaveData> Army1Units { get; set; }

        public string Army2Name { get; set; }
        public string Army2Prefix { get; set; }
        public ConsoleColor Army2Color { get; set; }
        public List<UnitSaveData> Army2Units { get; set; }

        public DateTime SaveDate { get; set; }
        public int TotalCost1 { get; set; }
        public int TotalCost2 { get; set; }
    }

    public class UnitSaveData
    {
        public string Type { get; set; }
        public int FighterNumber { get; set; }
        public int Health { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
        public int Cost { get; set; }
    }

    // Вспомогательный класс для записи в несколько потоков
    public class CompositeTextWriter : TextWriter
    {
        private readonly TextWriter[] _writers;
        private readonly IFormatProvider _formatProvider;

        public CompositeTextWriter(params TextWriter[] writers)
        {
            _writers = writers ?? throw new ArgumentNullException(nameof(writers));
            _formatProvider = writers.Length > 0 ? writers[0].FormatProvider : null;
        }

        public override IFormatProvider FormatProvider => _formatProvider;

        public override void Write(char value)
        {
            foreach (var writer in _writers)
            {
                writer.Write(value);
            }
        }

        public override void Write(string value)
        {
            foreach (var writer in _writers)
            {
                writer.Write(value);
            }
        }

        public override void WriteLine()
        {
            foreach (var writer in _writers)
            {
                writer.WriteLine();
            }
        }

        public override void WriteLine(string value)
        {
            foreach (var writer in _writers)
            {
                writer.WriteLine(value);
            }
        }

        public override void WriteLine(string format, params object[] arg)
        {
            foreach (var writer in _writers)
            {
                writer.WriteLine(format, arg);
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            foreach (var writer in _writers)
            {
                writer.Write(buffer, index, count);
            }
        }

        public override void Flush()
        {
            foreach (var writer in _writers)
            {
                writer.Flush();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var writer in _writers)
                {
                    writer.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public override System.Text.Encoding Encoding
        {
            get { return _writers.Length > 0 ? _writers[0].Encoding : System.Text.Encoding.Default; }
        }
    }
}