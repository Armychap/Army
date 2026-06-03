using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ArmyBattle.Models;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Сервис для логирования и сохранения битв.
    /// SRP: отвечает ТОЛЬКО за захват вывода битвы и управление логами.
    /// </summary>
    public class BattleLogService
    {
        private readonly TextFileService _textFileService;
        private readonly JsonStorageService _jsonStorageService;

        public BattleLogService()
        {
            _textFileService = new TextFileService("Logs");
            _jsonStorageService = new JsonStorageService("Logs");
        }

        /// <summary>
        /// Захватывает вывод битвы и возвращает записанный текст
        /// </summary>
        public string CaptureBattleOutput(Action battleAction)
        {
            var originalOutput = Console.Out;
            var logCapture = new StringWriter();
            var compositeWriter = new CompositeTextWriter(originalOutput, logCapture);

            Console.SetOut(compositeWriter);
            try
            {
                battleAction();
                return logCapture.ToString();
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }

        /// <summary>
        /// Сохраняет лог битвы с временной меткой
        /// </summary>
        public void SaveBattleLog(string log, string battleName, ArmySaveData armyData)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string logFileName = $"{battleName}_{timestamp}.txt";
            string jsonFileName = $"{battleName}_{timestamp}.json";

            _textFileService.SaveText(log, logFileName);
            _jsonStorageService.SaveToJson(armyData, jsonFileName);
        }

        /// <summary>
        /// Дописывает лог в существующий файл или создаёт новый
        /// </summary>
        public void AppendToBattleLog(string log, string battleName, ArmySaveData armyData)
        {
            string textFileName = $"{battleName}.txt";
            string jsonFileName = $"{battleName}.json";

            if (_textFileService.TextFileExists(textFileName))
            {
                _textFileService.AppendText(log, textFileName);
            }
            else
            {
                _textFileService.SaveText(log, textFileName);
            }

            _jsonStorageService.SaveToJson(armyData, jsonFileName);
        }

        /// <summary>
        /// Получает список имен всех сохраненных логов битв
        /// </summary>
        public string[] GetSavedBattleNames()
        {
            return _textFileService.GetTextFileNames();
        }

        /// <summary>
        /// Получает список только завершенных битв
        /// </summary>
        public string[] GetFinishedBattleNames()
        {
            return _textFileService.GetFilesNotContaining("ИГРА НЕ ЗАВЕРШЕНА");
        }

        /// <summary>
        /// Получает содержимое лога битвы
        /// </summary>
        public string GetBattleLog(string battleName)
        {
            return _textFileService.LoadText($"{battleName}.txt");
        }

        /// <summary>
        /// Получает отображаемое имя битвы без временной метки
        /// </summary>
        public string GetBattleDisplayName(string fileName)
        {
            int lastUnderscoreIndex = fileName.LastIndexOf('_');

            if (lastUnderscoreIndex > 0)
            {
                string timeStr = fileName.Substring(lastUnderscoreIndex + 1);
                if (timeStr.Length == 6 && int.TryParse(timeStr, out _))
                {
                    int prevUnderscoreIndex = fileName.LastIndexOf('_', lastUnderscoreIndex - 1);

                    if (prevUnderscoreIndex > 0)
                    {
                        string dateStr = fileName.Substring(prevUnderscoreIndex + 1, lastUnderscoreIndex - prevUnderscoreIndex - 1);
                        if (dateStr.Length == 8 && int.TryParse(dateStr, out _))
                        {
                            string namePart = fileName.Substring(0, prevUnderscoreIndex);
                            if (!string.IsNullOrEmpty(namePart) && namePart != "_")
                                fileName = namePart;
                        }
                    }
                }
            }

            return fileName.Replace("_vs_", " vs ");
        }

        /// <summary>
        /// Формирует полный путь к файлу лога битвы
        /// </summary>
        public string GetLogPath(string battleName)
        {
            return Path.Combine("Logs", FileSystemService.SanitizeFileName(battleName) + ".txt");
        }

        /// <summary>
        /// Получает список имен всех сохраненных данных армий
        /// </summary>
        public string[] GetSavedBattleArmies()
        {
            return _jsonStorageService.GetJsonFileNames();
        }

        /// <summary>
        /// Загружает данные армий из сохраненной битвы
        /// </summary>
        public ArmySaveData? LoadBattleArmies(string battleName)
        {
            return _jsonStorageService.LoadFromJson<ArmySaveData>($"{battleName}.json");
        }
    }
}
