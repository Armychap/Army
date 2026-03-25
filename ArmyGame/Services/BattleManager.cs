using System;
using System.IO;
using System.Text.Json;
using ArmyBattle.Models;
using ArmyBattle.Game;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Управляет проведением битв и организацией логирования их результатов.
    /// Основные обязанности:
    /// - Проведение боевого симулятора между двумя армиями
    /// - Захват вывода битвы (console output capture для логирования)
    /// - Сохранение логов битв в TXT файлы
    /// - Сохранение состояния армий при завершении битвы в JSON
    /// - Управление доступом к сохраненным битвам и логам
    /// </summary>
    public class BattleManager
    {
        // Папка для хранения текстовых логов битв
        private string logsDirectory = "Logs";
        
        // Сервис для управления сохранением/загрузкой данных армий
        private ArmyManager armyManager;

        /// <summary>
        /// Конструктор инициализирует сервис логирования и создает необходимые директории.
        /// </summary>
        public BattleManager()
        {
            // Создаем новый экземпляр сервиса для управления армиями
            armyManager = new ArmyManager();
            
            // Инициализируем директорию для логов
            CreateDirectoriesIfNeeded();
        }

        /// <summary>
        /// Создает директорию для логов битв, если её еще нет в файловой системе.
        /// </summary>
        private void CreateDirectoriesIfNeeded()
        {
            // Проверяем наличие директории логов
            if (!Directory.Exists(logsDirectory))
                // Если директория не существует - создаем её
                Directory.CreateDirectory(logsDirectory);
        }

        public void StartBattle(IArmy army1, IArmy army2, bool saveLog = false)
        {
            // Сохраняем оригинальный вывод консоли для восстановления позже
            var originalOutput = Console.Out;
            
            // Создаем StringWriter для захвата всего вывода в памяти
            var logCapture = new StringWriter();
            
            // Создаем составной писатель, который пишет одновременно в консоль и в буфер
            var compositeWriter = new CompositeTextWriter(originalOutput, logCapture);
            
            // Перенаправляем консоль на наш составной писатель
            Console.SetOut(compositeWriter);

            try
            {
                // Создаем боевой симулятор между двумя армиями
                // Параметр 400 - это максимальное количество раундов боя
                BattleEngine battle = new BattleEngine(army1, army2, 400);
                
                // Запускаем боевой симулятор
                // Все вывод будет захвачен в logCapture благодаря CompositeTextWriter
                battle.StartBattle();

                // Восстанавливаем оригинальный вывод консоли
                Console.SetOut(originalOutput);

                // Если требуется сохранение лога - сохраняем его с захваченным текстом
                if (saveLog)
                {
                    SaveBattleLog(logCapture.ToString(), $"{army1.Name}_vs_{army2.Name}", army1, army2);
                }
            }
            catch (Exception ex)
            {
                // Восстанавливаем оригинальный вывод даже при ошибке
                Console.SetOut(originalOutput);
                
                // Выводим сообщение об ошибке
                Console.WriteLine($"Ошибка во время битвы: {ex.Message}");
            }
        }

        /// <summary>
        /// Сохраняет полный лог битвы в TXT файл и состояние армий в JSON
        /// Создает две строки файлов для каждой битвы - текстовый и данные армий
        /// </summary>
        /// <summary>
        /// Сохраняет лог битвы. Если useTimestamp=true, добавляет временную метку (для новых битв).
        /// Если useTimestamp=false, перезаписывает файл без метки (для продолжений игр).
        /// </summary>
        public void SaveBattleLog(string log, string battleName, IArmy army1, IArmy army2, bool useTimestamp = true)
        {
            string logFileName;
            string jsonFileName;
            
            if (useTimestamp)
            {
                // Для новых битв - добавляем временную метку
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                logFileName = $"{battleName}_{timestamp}.txt";
                jsonFileName = $"{battleName}_{timestamp}.json";
            }
            else
            {
                // Для продолжаемых игр - перезаписываем без метки
                logFileName = $"{battleName}.txt";
                jsonFileName = $"{battleName}.json";
            }

            // СОХРАНЕНИЕ ТЕКСТОВОГО ЛОГА
            string logFilePath = Path.Combine(logsDirectory, logFileName);
            File.WriteAllText(logFilePath, log);

            // СОХРАНЕНИЕ ДАННЫХ АРМИЙ В JSON
            var armyData = armyManager.SerializeArmies(army1, army2);
            string jsonFilePath = Path.Combine(logsDirectory, jsonFileName);
            string jsonContent = JsonSerializer.Serialize(armyData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonFilePath, jsonContent);
        }

        /// <summary>
        /// Получает список имен всех сохраненных логов битв
        /// </summary>
        public string[] GetSavedBattles()
        {
            // Получаем все TXT файлы из директории логов
            var files = Directory.GetFiles(logsDirectory, "*.txt");
            
            // Создаем массив результатов нужного размера
            var result = new string[files.Length];
            
            // Итерируемся по каждому найденному файлу
            for (int i = 0; i < files.Length; i++)
            {
                // Извлекаем имя файла без расширения .txt
                result[i] = Path.GetFileNameWithoutExtension(files[i]);
            }
            
            // Возвращаем массив имен логов
            return result;
        }

        /// <summary>
        /// Получает содержимое лога битвы по имени файла (без расширения)
        /// </summary>
        public string GetBattleLog(string battleName)
        {
            // Ищем файл с точным именем
            string filePath = Path.Combine(logsDirectory, $"{battleName}.txt");
            
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            
            return "";
        }
        
        public string GetBattleDisplayName(string fileName)
        {
            // Находим последний подчеркивание (это время HHMMSS)
            int lastUnderscoreIndex = fileName.LastIndexOf('_');
            
            if (lastUnderscoreIndex > 0)
            {
                // Проверяем что это 6 цифр (время)
                string timeStr = fileName.Substring(lastUnderscoreIndex + 1);
                if (timeStr.Length == 6 && int.TryParse(timeStr, out _))
                {
                    // Ищем второй последний подчеркивание (дата YYYYMMDD)
                    int prevUnderscoreIndex = fileName.LastIndexOf('_', lastUnderscoreIndex - 1);
                    
                    if (prevUnderscoreIndex > 0)
                    {
                        // Проверяем что это 8 цифр (дата)
                        string dateStr = fileName.Substring(prevUnderscoreIndex + 1, lastUnderscoreIndex - prevUnderscoreIndex - 1);
                        if (dateStr.Length == 8 && int.TryParse(dateStr, out _))
                        {
                            // Берем только часть до даты и временной метки
                            fileName = fileName.Substring(0, prevUnderscoreIndex);
                        }
                    }
                }
            }
            
            // Заменяем _vs_ на " vs "
            fileName = fileName.Replace("_vs_", " vs ");
            
            return fileName;
        }

        /// <summary>
        /// Формирует полный путь к файлу лога битвы по названию
        /// </summary>
        public string GetLogPath(string battleName)
        {
            // Объединяем путь: папка логов + имя файла + расширение
            return Path.Combine(logsDirectory, $"{battleName}.txt");
        }

        /// <summary>
        /// Получает список имен всех сохраненных данных армий из завершенных битв.
        /// </summary>
        public string[] GetSavedBattleArmies()
        {
            // Получаем все JSON файлы из директории логов
            var files = Directory.GetFiles(logsDirectory, "*.json");
            
            // Создаем массив результатов нужного размера
            var result = new string[files.Length];
            
            // Итерируемся по каждому найденному JSON файлу
            for (int i = 0; i < files.Length; i++)
            {
                // Извлекаем имя файла без расширения .json
                result[i] = Path.GetFileNameWithoutExtension(files[i]);
            }
            
            // Возвращаем массив всех доступных боев
            return result;
        }

        /// <summary>
        /// Загружает данные армий из сохраненной битвы
        /// </summary>
        public ArmySaveData LoadBattleArmies(string battleName)
        {
            // Объединяем путь к JSON файлу в папке логов
            string jsonPath = Path.Combine(logsDirectory, $"{battleName}.json");
            
            // Проверяем существование файла
            if (!File.Exists(jsonPath))
                // Если файла нет - возвращаем null
                return null;

            try
            {
                // Читаем содержимое JSON файла
                string jsonContent = File.ReadAllText(jsonPath);
                
                // Десериализуем JSON в объект ArmySaveData
                return JsonSerializer.Deserialize<ArmySaveData>(jsonContent);
            }
            catch
            {
                // При любой ошибке десериализации возвращаем null
                return null;
            }
        }

        /// <summary>
        /// Удаляет все логи битв и сохраненные данные армий
        /// </summary>
        public bool DeleteAllBattleLogs()
        {
            try
            {
                // Удаляем все файлы из папки логов
                if (Directory.Exists(logsDirectory))
                {
                    var txtFiles = Directory.GetFiles(logsDirectory, "*.txt");
                    foreach (var file in txtFiles)
                    {
                        File.Delete(file);
                    }

                    var jsonFiles = Directory.GetFiles(logsDirectory, "*.json");
                    foreach (var file in jsonFiles)
                    {
                        File.Delete(file);
                    }
                }

                // Удаляем все файлы из папки сохранений игр (незавершённые игры)
                string savesDirectory = "Saves";
                if (Directory.Exists(savesDirectory))
                {
                    var saveFiles = Directory.GetFiles(savesDirectory, "*.json");
                    foreach (var file in saveFiles)
                    {
                        File.Delete(file);
                    }
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

