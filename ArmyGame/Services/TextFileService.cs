using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Сервис для работы с текстовыми файлами.
    /// SRP: отвечает ТОЛЬКО за чтение/запись текстовых файлов.
    /// </summary>
    public class TextFileService
    {
        private readonly FileSystemService _fileSystemService;

        public TextFileService(string directory)
        {
            _fileSystemService = new FileSystemService(directory);
        }

        /// <summary>
        /// Сохраняет текст в файл (перезаписывает)
        /// </summary>
        public void SaveText(string text, string fileName)
        {
            try
            {
                fileName = FileSystemService.SanitizeFileName(fileName);
                string filePath = _fileSystemService.GetFilePath(fileName);
                File.WriteAllText(filePath, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка сохранения текста: {ex.Message}");
            }
        }

        /// <summary>
        /// Дописывает текст в существующий файл или создаёт новый
        /// </summary>
        public void AppendText(string text, string fileName)
        {
            try
            {
                fileName = FileSystemService.SanitizeFileName(fileName);
                string filePath = _fileSystemService.GetFilePath(fileName);
                File.AppendAllText(filePath, text + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка дописания текста: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает текст из файла
        /// </summary>
        public string LoadText(string fileName)
        {
            try
            {
                fileName = FileSystemService.SanitizeFileName(fileName);
                string filePath = _fileSystemService.GetFilePath(fileName);

                if (!File.Exists(filePath))
                    return string.Empty;

                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка загрузки текста: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Получает список имён текстовых файлов (без расширения)
        /// </summary>
        public string[] GetTextFileNames()
        {
            return _fileSystemService.GetFileNamesWithoutExtension("*.txt");
        }

        /// <summary>
        /// Получает список файлов, которые НЕ содержат строку
        /// </summary>
        public string[] GetFilesNotContaining(string searchText)
        {
            var allFiles = _fileSystemService.GetFiles("*.txt");
            var result = new List<string>();

            foreach (var file in allFiles)
            {
                try
                {
                    string content = File.ReadAllText(file);
                    if (!content.Contains(searchText))
                        result.Add(FileSystemService.GetFileNameWithoutExtension(file));
                }
                catch
                {
                    // Пропускаем поврежденные файлы
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Проверяет существование текстового файла
        /// </summary>
        public bool TextFileExists(string fileName)
        {
            fileName = FileSystemService.SanitizeFileName(fileName);
            return _fileSystemService.FileExists(fileName);
        }
    }
}
