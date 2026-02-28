using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ArmyBattle.Models;

namespace ArmyBattle.Services
{
    /// <summary>
    /// сохранение/загрузка в JSON
    /// отвечает только за работу с файловой системой и сериализацией
    /// Зависит от интерфейса IArmy, а не от конкретной реализации (DIP)
    /// Основные обязанности:
    /// - Сохранение состояния армий в JSON формат
    /// - Загрузка армий из JSON файлов 
    /// - Управление файловой структурой для сохранений
    /// - Сериализация/десериализация юнитов
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
                // Если директория не существует - создаем её
                Directory.CreateDirectory(savesDirectory);
        }

        /// <summary>
        /// Сохраняет две армии в JSON файл с опциональным названием.
        /// Если название не указано, использует текущую дату и время.
        /// Параметры:
        /// - army1: первая армия для сохранения
        /// - army2: вторая армия для сохранения
        /// - saveName: опциональное название файла без расширения
        /// </summary>
        public void SaveArmies(IArmy army1, IArmy army2, string saveName = null)
        {
            // Если название не указано или пусто - используем автогенерируемое имя с временем
            if (string.IsNullOrWhiteSpace(saveName))
            {
                saveName = $"Armies_{DateTime.Now:yyyyMMdd_HHmmss}";
            }

            // Конвертируем армии в сериализуемый формат
            var saveData = SerializeArmies(army1, army2);
            
            // Преобразуем объект в JSON строку с отступами для читаемости
            string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
            
            // Формируем полный путь: Saves/[имя].json
            string filePath = Path.Combine(savesDirectory, $"{saveName}.json");

            // Записываем JSON в файл
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Загружает две армии из JSON файла.
        /// Восстанавливает все параметры армий и их юнитов.
        /// Параметры:
        /// - filePath: полный путь к файлу JSON
        /// - army1: первая восстановленная армия (out параметр)
        /// - army2: вторая восстановленная армия (out параметр)
        /// Возвращает: true если загрузка успешна, false при ошибке
        /// </summary>
        public bool LoadArmies(string filePath, out IArmy army1, out IArmy army2)
        {
            // Инициализируем выходные параметры нулевыми значениями
            army1 = null;
            army2 = null;

            try
            {
                // Читаем содержимое JSON файла
                string json = File.ReadAllText(filePath);
                
                // Десериализуем JSON в объект ArmySaveData
                var saveData = JsonSerializer.Deserialize<ArmySaveData>(json);

                // Проверяем что данные корректно загружены
                if (saveData == null)
                    return false;

                // Создаем первую армию с восстановленными параметрами
                army1 = new Army(saveData.Army1Name, saveData.Army1Color);
                
                // Создаем вторую армию с восстановленными параметрами
                army2 = new Army(saveData.Army2Name, saveData.Army2Color);

                // Добавляем юнитов в первую армию из сохраненных данных
                DeserializeUnits(saveData.Army1Units, army1);
                
                // Добавляем юнитов во вторую армию из сохраненных данных
                DeserializeUnits(saveData.Army2Units, army2);

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
        /// Формирует полный путь к файлу сохранения по его названию.
        /// Параметры:
        /// - saveName: название сохранения без расширения
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
        /// - army1: первая армия
        /// - army2: вторая армия
        /// Возвращает: объект ArmySaveData готовый к JSON сериализации
        /// </summary>
        public ArmySaveData SerializeArmies(IArmy army1, IArmy army2)
        {
            // Создаем новый объект для сохранения данных армий
            return new ArmySaveData
            {
                // === ПЕРВАЯ АРМИЯ ===
                // Сохраняем название первой армии
                Army1Name = army1.Name,
                
                // Сохраняем цвет для отображения первой армии в консоли
                Army1Color = army1.Color,
                
                // Сериализуем список юнитов первой армии
                Army1Units = SerializeUnits(army1.Units),
                
                // Сохраняем общую стоимость всех юнитов первой армии
                TotalCost1 = army1.TotalCost,

                // === ВТОРАЯ АРМИЯ ===
                // Сохраняем название второй армии
                Army2Name = army2.Name,
                
                // Сохраняем цвет для отображения второй армии в консоли
                Army2Color = army2.Color,
                
                // Сериализуем список юнитов второй армии
                Army2Units = SerializeUnits(army2.Units),
                
                // Сохраняем общую стоимость всех юнитов второй армии
                TotalCost2 = army2.TotalCost,

                // === МЕТАДАННЫЕ ===
                // Сохраняем дату и время сохранения для истории
                SaveDate = DateTime.Now
            };
        }

        /// <summary>
        /// Преобразует список юнитов в список сериализуемых объектов UnitSaveData.
        /// Экстрактирует основные характеристики каждого юнита для сохранения.
        /// Параметры:
        /// - units: список юнитов для сериализации (List&lt;IUnit&gt;)
        /// Возвращает: список сериализуемых данных юнитов
        /// </summary>
        private List<UnitSaveData> SerializeUnits(List<IUnit> units)
        {
            // Создаем пустой список для результатов
            var result = new List<UnitSaveData>();
            
            // Итерируемся по каждому юниту в списке
            foreach (var unit in units)
            {
                // Добавляем сохраненные данные юнита в результат
                result.Add(new UnitSaveData
                {
                    // Сохраняем тип юнита (имя класса: WeakFighter, Archer, StrongFighter)
                    Type = unit.GetType().Name,
                    
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
            
            // Возвращаем заполненный список сохраненных юнитов
            return result;
        }

        /// <summary>
        /// Восстанавливает юнитов из сохраненных данных и добавляет их в армию.
        /// Создает нужные типы юнитов и восстанавливает их характеристики.
        /// Параметры:
        /// - unitsData: список сохраненных данных юнитов
        /// - army: армия, в которую нужно добавить восстановленные юниты
        /// </summary>
        private void DeserializeUnits(List<UnitSaveData> unitsData, IArmy army)
        {
            // Итерируемся по каждому сохраненному юниту
            foreach (var unitData in unitsData)
            {
                // Используем switch выражение для создания правильного типа юнита
                Unit unit = unitData.Type switch
                {
                    // Если тип был "WeakFighter" - создаем слабого бойца
                    nameof(WeakFighter) => new WeakFighter(unitData.FighterNumber),
                    
                    // Если тип был "Archer" - создаем лучника
                    nameof(Archer) => new Archer(unitData.FighterNumber),
                    
                    // Если тип был "StrongFighter" - создаем сильного бойца
                    nameof(StrongFighter) => new StrongFighter(unitData.FighterNumber),
                    
                    // Если тип неизвестен - выбрасываем исключение с описанием ошибки
                    _ => throw new Exception($"Неизвестный тип юнита: {unitData.Type}")
                };

                // Восстанавливаем здоровье юнита из сохраненного значения
                // (это важно если юнит был поврежден в предыдущем бою)
                unit.Health = unitData.Health;
                
                // Добавляем восстановленного юнита в армию
                army.AddUnit(unit);
            }
        }
    }
}

