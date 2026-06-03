using System;
using System.IO;
using System.Collections.Generic;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Сервис для управления файловой системой.
    /// SRP: отвечает ТОЛЬКО за работу с директориями и путями файлов.
    /// </summary>
    public class FileSystemService
    {
        private readonly string _directory;

        public FileSystemService(string directory)
        {
            _directory = directory;
            CreateDirectoryIfNeeded();
        }

        /// <summary>
        /// Создаёт директорию, если её нет
        /// </summary>
        public void CreateDirectoryIfNeeded()
        {
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
        }

        /// <summary>
        /// Получает все файлы с расширением из директории
        /// </summary>
        public string[] GetFiles(string pattern)
        {
            try
            {
                return Directory.GetFiles(_directory, pattern);
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Получает полный путь к файлу
        /// </summary>
        public string GetFilePath(string fileName)
        {
            return Path.Combine(_directory, fileName);
        }

        /// <summary>
        /// Проверяет существование файла
        /// </summary>
        public bool FileExists(string fileName)
        {
            return File.Exists(GetFilePath(fileName));
        }

        /// <summary>
        /// Удаляет недопустимые символы из имени файла
        /// </summary>
        public static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            foreach (var invalid in Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');

            return name.Trim();
        }

        /// <summary>
        /// Получает имя файла без расширения
        /// </summary>
        public static string GetFileNameWithoutExtension(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>
        /// Получает имена всех файлов (без расширения) с паттерном
        /// </summary>
        public string[] GetFileNamesWithoutExtension(string pattern)
        {
            var files = GetFiles(pattern);
            var result = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
                result[i] = GetFileNameWithoutExtension(files[i]);

            return result;
        }
    }
}
