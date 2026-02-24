// Business/ScoreBoard.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WordChainGame.Models;

namespace WordChainGame.Business
{
    /// <summary>
    /// Класс для хранения и управления таблицей рекордов
    /// </summary>
    public class ScoreBoard
    {
        private readonly string _filePath; //путь
        private readonly List<Player> _scores; //список игроков с результатами

        public ScoreBoard(string filePath)
        {
            _filePath = filePath; //путь
            _scores = new List<Player>(); //список игроков
            LoadScores(); //метод загрузки рекордов из файла
        }

        /// <summary>
        /// Загрузка рекордов из txt файла
        /// </summary>
        private void LoadScores()
        {
            if (!File.Exists(_filePath)) // Проверка существования файла перед загрузкой
                return;

            var lines = File.ReadAllLines(_filePath); //чтение файла (все строки читает)
            foreach (var line in lines)
            {
                // Разделение строки по символу '|', удаление пустых элементов
                var parts = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4) continue; // Пропуск строки, если она содержит меньше 4 частей

                // Попытка преобразования строковых значений в числовые
                if (int.TryParse(parts[1], out int score) &&
                    int.TryParse(parts[2], out int games) &&
                    int.TryParse(parts[3], out int bestChain))
                {
                    // Создание нового объекта Player и добавление в список
                    _scores.Add(new Player
                    {
                        Name = parts[0].Trim(), //имя 
                        Score = score, //счет текущий
                        TotalGamesPlayed = games, //количество игр
                        BestChainLength = bestChain //лучшая длина цепочки
                    });
                }
            }
        }

        /// <summary>
        /// Перезагрузка рекордов из файла (для обновления данных после завершения игры)
        /// </summary>
        public void ReloadScores()
        {
            _scores.Clear(); // Очищаем текущие данные
            LoadScores(); // Загружаем заново из файла
        }

        /// <summary>
        /// Сохранение рекордов в txt файл
        /// </summary>
        public void SaveScores()
        {
            // Преобразование списка игроков в список строк формата "Имя|Счет|Игры|ЛучшаяЦепочка"
            var lines = _scores
                .Select(p => $"{p.Name}|{p.Score}|{p.TotalGamesPlayed}|{p.BestChainLength}")
                .ToList();
            File.WriteAllLines(_filePath, lines); // Запись всех строк в файл (создание или перезапись)
        }

        /// <summary>
        /// Получить топ 10 игроков по кол-ву очков
        /// </summary>
        public List<Player> GetTopPlayers(int count = 10)
        {
            // Сортировка по убыванию счета и выбор указанного количества игроков
            return _scores
                .OrderByDescending(p => p.Score)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Получить список всех игроков
        /// </summary>
        public List<Player> GetAllPlayers()
        {
            // Возвращение новой копии списка
            return new List<Player>(_scores);
        }

        /// <summary>
        /// Сохранение результатов в дополнительный JSON файл
        /// </summary>
        public void SaveScoresJson(string? jsonFilePath = null)
        {
            try
            {
                // Определение пути для JSON файла: если не указан, то создаем на основе основного пути
                var path = jsonFilePath ?? _filePath.Replace(".json", "_data.json");
                var options = new System.Text.Json.JsonSerializerOptions // Создание настроек сериализации JSON
                {
                    WriteIndented = true, // Форматирование с отступами для читаемости
                    // Использование camelCase для имен свойств
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };

                // Сериализация списка игроков в JSON строку
                var json = System.Text.Json.JsonSerializer.Serialize(_scores, options);
                File.WriteAllText(path, json); // Запись JSON строки в файл
                Console.WriteLine($"Результаты сохранены в JSON: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении в JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// Добавить нового игрока в таблицу рекордов
        /// </summary>
        public void AddNewPlayer(Player player)
        {
            // Проверка, что игрока с таким именем еще нет в списке (без учета регистра)
            if (!_scores.Any(p => p.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase)))
            {
                _scores.Add(player); // Добавление игрока в список
                SaveScores(); // Сохранение изменений в файл
            }
        }
        /// <summary>
        /// Обновление результатов после игры
        /// </summary>
        public void UpdateScore(GameResult gameResult)
        {
            // Проверка корректности переданного результата игры
            if (gameResult?.Winner == null || gameResult?.Loser == null)
            {
                Console.WriteLine("Ошибка: некорректный результат игры");
                return;
            }

            // Обновляем счет победителя
            UpdatePlayerScore(gameResult.Winner, gameResult.WinnerScore, gameResult.ChainLength);

            // Обновляем счет проигравшего - ИСПРАВЛЕНО: LooserScore -> LoserScore
            UpdatePlayerScore(gameResult.Loser, gameResult.LoserScore, gameResult.ChainLength);

            SaveScores(); // Сохранение обновленных результатов в файл
        }


//KISS
        /// <summary>
        /// метод для обновления счета конкретного игрока
        /// </summary>
        private void UpdatePlayerScore(Player player, int score, int chainLength)
        {
            // Поиск существующего игрока в списке по имени
            var existingPlayer = _scores.Find(p => p.Name == player.Name);

            if (existingPlayer != null) // Если игрок уже существует
            {
                existingPlayer.BestChainLength = Math.Max(existingPlayer.BestChainLength, chainLength); // Обновление лучшей длины цепочки (берется максимальное значение)
                existingPlayer.TotalGamesPlayed++; // Увеличение счетчика сыгранных игр
                existingPlayer.Score += Math.Max(0, score); // Добавление очков из текущей игры к общему счету (не меньше 0)
            }
            else // Если игрок новый, создаем запись для него
            {
                _scores.Add(new Player
                {
                    Name = player.Name, //имя
                    Score = Math.Max(0, score), //счет
                    TotalGamesPlayed = 1, //счетчик игр
                    BestChainLength = chainLength //лучная длина цепочки
                });
            }
        }
        /// <summary>
        /// Очистка таблицы рекордов
        /// </summary>
        public void Clear()
        {
            _scores.Clear(); // Очистка списка игроков в памяти
            SaveScores(); // Сохранение пустого списка в файл
        }
    }
}