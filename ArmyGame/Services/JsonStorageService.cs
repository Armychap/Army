using System;
using System.IO;
using System.Text.Json;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Сервис для сохранения и загрузки JSON данных.
    /// SRP: отвечает ТОЛЬКО за работу с JSON файлами.
    /// </summary>
    public class JsonStorageService
    {
        private readonly FileSystemService _fileSystemService;

        public JsonStorageService(string directory)
        {
            _fileSystemService = new FileSystemService(directory);
        }

        /// <summary>
        /// Сохраняет объект в JSON файл
        /// </summary>
        public void SaveToJson<T>(T data, string fileName)
        {
            try
            {
                fileName = FileSystemService.SanitizeFileName(fileName);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(data, options);
                string filePath = _fileSystemService.GetFilePath(fileName);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка сохранения JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает объект из JSON файла
        /// </summary>
        public T? LoadFromJson<T>(string fileName)
        {
            try
            {
                fileName = FileSystemService.SanitizeFileName(fileName);
                string filePath = _fileSystemService.GetFilePath(fileName);

                if (!File.Exists(filePath))
                    return default;

                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка загрузки JSON: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Проверяет существование JSON файла
        /// </summary>
        public bool JsonFileExists(string fileName)
        {
            fileName = FileSystemService.SanitizeFileName(fileName);
            return _fileSystemService.FileExists(fileName);
        }

        /// <summary>
        /// Получает список имён JSON файлов (без расширения)
        /// </summary>
        public string[] GetJsonFileNames()
        {
            return _fileSystemService.GetFileNamesWithoutExtension("*.json");
        }
    }
}
