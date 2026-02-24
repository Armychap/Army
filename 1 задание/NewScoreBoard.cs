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

        //DRY (нет магических чисел)
        private const char SEPARATOR = '|';
        private const int REQUIRED_PARTS_COUNT = 4;

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
            if (!File.Exists(_filePath))
                return;

            foreach (var line in File.ReadAllLines(_filePath))
            {
                var player = ParseLineToPlayer(line);
                if (player != null)
                    _scores.Add(player);
            }
        }

        // (парсинга вынесена в отдельный метод)
        private Player? ParseLineToPlayer(string line)
        {
            //константы
            var parts = line.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < REQUIRED_PARTS_COUNT)
                return null;

            if (!TryParsePlayerStats(parts, out int score, out int games, out int bestChain))
                return null;

            return CreatePlayerFromParts(parts[0], score, games, bestChain);
        }

        // DRY (парсинг числовых значений)
        private bool TryParsePlayerStats(string[] parts, out int score, out int games, out int bestChain)
        {
            score = 0;
            games = 0;
            bestChain = 0;

            return int.TryParse(parts[1], out score) &&
                   int.TryParse(parts[2], out games) &&
                   int.TryParse(parts[3], out bestChain);
        }

        // DRY (создание игрока)
        private Player CreatePlayerFromParts(string name, int score, int games, int bestChain)
        {
            return new Player
            {
                Name = name.Trim(),
                Score = score,
                TotalGamesPlayed = games,
                BestChainLength = bestChain
            };
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
                .Select(p => FormatPlayerToString(p))
                .ToList();
            File.WriteAllLines(_filePath, lines); // Запись всех строк в файл (создание или перезапись)
        }

        //DRY (нет дублирование формата "Имя|Счет|Игры|ЛучшаяЦепочка")
        private string FormatPlayerToString(Player player)
        {
            return $"{player.Name}{SEPARATOR}{player.Score}{SEPARATOR}" +
                   $"{player.TotalGamesPlayed}{SEPARATOR}{player.BestChainLength}";
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
            // YAGNI (упрощена проверка - убрано избыточное логирование)
            if (gameResult?.Winner == null || gameResult?.Loser == null)
                return;

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
                UpdateExistingPlayer(existingPlayer, score, chainLength);
            }
            else // Если игрок новый, создаем запись для него
            {
                AddNewPlayerWithScore(player, score, chainLength);
            }
        }

        /// <summary>
        /// метод для обновления счета существующего игрока
        /// </summary>
        private void UpdateExistingPlayer(Player player, int score, int chainLength)
        {
            player.BestChainLength = Math.Max(player.BestChainLength, chainLength);
            player.TotalGamesPlayed++;
            player.Score += Math.Max(0, score);
        }

        /// <summary>
        /// метод для обновления счета для нового игрока
        /// </summary>
        private void AddNewPlayerWithScore(Player player, int score, int chainLength)
        {
            _scores.Add(CreatePlayerFromParts(player.Name, Math.Max(0, score), 1, chainLength));
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




