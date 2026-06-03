using System;
using System.IO;
using ArmyBattle.Models;
using ArmyBattle.Game;
using ArmyBattle.Services;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Управляет проведением битв.
    /// SRP: отвечает ТОЛЬКО за логику запуска боевого симулятора.
    /// Все остальные обязанности делегирует специализированным сервисам.
    /// </summary>
    public class BattleManager
    {
        /// <summary>
        /// Сервис для логирования битв (захват вывода + сохранение)
        /// </summary>
        private readonly BattleLogService _battleLogService;

        /// <summary>
        /// Сервис для управления состоянием армий
        /// </summary>
        private readonly ArmyManager _armyManager;

        /// <summary>
        /// Конструктор инициализирует зависимые сервисы
        /// </summary>
        public BattleManager()
        {
            _battleLogService = new BattleLogService();
            _armyManager = new ArmyManager();
        }

        public void StartBattle(IArmy army1, IArmy army2, bool saveLog = false)
        {
            // Применяем настройки наблюдателей к армиям перед боем
            ObserverManager.LoadSettings(army1, army2);

            try
            {
                // Создаем боевой симулятор между двумя армиями
                BattleEngine battle = new BattleEngine(army1, army2, 400);

                // Запускаем боевой симулятор с захватом вывода
                string log = _battleLogService.CaptureBattleOutput(() => battle.StartBattle());

                // Если требуется сохранение лога - сохраняем его
                if (saveLog)
                {
                    _battleLogService.SaveBattleLog(log, FileSystemService.SanitizeFileName($"{army1.Name}_vs_{army2.Name}"), 
                        _armyManager.SerializeArmies(army1, army2));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка во время битвы: {ex.Message}");
            }
        }

        /// <summary>
        /// Получает список имен всех сохраненных логов битв
        /// </summary>
        public string[] GetSavedBattles()
        {
            return _battleLogService.GetSavedBattleNames();
        }

        /// <summary>
        /// Получает список только завершенных битв (исключая незавершенные игры)
        /// </summary>
        public string[] GetFinishedBattles()
        {
            return _battleLogService.GetFinishedBattleNames();
        }

        /// <summary>
        /// Получает содержимое лога битвы по имени файла
        /// </summary>
        public string GetBattleLog(string battleName)
        {
            return _battleLogService.GetBattleLog(battleName);
        }

        /// <summary>
        /// Получает отображаемое имя битвы, удаляя временные метки
        /// </summary>
        public string GetBattleDisplayName(string fileName)
        {
            return _battleLogService.GetBattleDisplayName(fileName);
        }

        /// <summary>
        /// Формирует полный путь к файлу лога битвы по названию
        /// </summary>
        public string GetLogPath(string battleName)
        {
            return _battleLogService.GetLogPath(battleName);
        }

        /// <summary>
        /// Получает список имен всех сохраненных данных армий из завершенных битв
        /// </summary>
        public string[] GetSavedBattleArmies()
        {
            return _battleLogService.GetSavedBattleArmies();
        }

        /// <summary>
        /// Сохраняет или дописывает лог битвы в зависимости от useTimestamp
        /// </summary>
        public void SaveBattleLog(string log, string battleName, IArmy army1, IArmy army2, bool useTimestamp = true)
        {
            var armyData = _armyManager.SerializeArmies(army1, army2);
            
            if (useTimestamp)
            {
                _battleLogService.SaveBattleLog(log, battleName, armyData);
            }
            else
            {
                _battleLogService.AppendToBattleLog(log, battleName, armyData);
            }
        }

        /// <summary>
        /// Дописывает лог в существующий файл битвы или создаёт новый
        /// </summary>
        public void AppendToBattleLog(string log, string battleName, IArmy army1, IArmy army2)
        {
            _battleLogService.AppendToBattleLog(log, battleName, 
                _armyManager.SerializeArmies(army1, army2));
        }

        /// <summary>
        /// Загружает данные армий из сохраненной битвы
        /// </summary>
        public ArmySaveData? LoadBattleArmies(string battleName)
        {
            return _battleLogService.LoadBattleArmies(battleName);
        }

        /// <summary>
        /// Удаляет все логи битв и сохраненные данные
        /// </summary>
        public bool DeleteAllBattleLogs()
        {
            try
            {
                // Удаляем все логи (текст и JSON)
                if (Directory.Exists("Logs"))
                {
                    var logsDir = new DirectoryInfo("Logs");
                    foreach (var file in logsDir.GetFiles("*.txt"))
                        file.Delete();
                    foreach (var file in logsDir.GetFiles("*.json"))
                        file.Delete();
                }

                // Удаляем сохраненные игры
                if (Directory.Exists("Saves"))
                {
                    var savesDir = new DirectoryInfo("Saves");
                    foreach (var file in savesDir.GetFiles("*.json"))
                        file.Delete();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении логов: {ex.Message}");
                return false;
            }
        }
    }
}

