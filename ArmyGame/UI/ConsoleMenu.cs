using System;
using System.Collections.Generic;
using ArmyBattle.Models;
using ArmyBattle.Services;

namespace ArmyBattle.UI
{
    /// <summary>
    /// Управляет всем выводом в консоль и вспомогательным интерфейсом.
    /// - Очистка и форматирование консоли
    /// - Вывод данных армий в красивом виде
    /// - Управление диалогами и выбором файлов
    /// - Получение пользовательского ввода
    /// - Справочные данные о типах юнитов
    /// </summary>
    public class ConsoleMenu
    {
        /// <summary>
        /// Очищает консоль безопасно (игнорирует ошибки если консоль недоступна).
        /// Используется перед отображением нового меню.
        /// </summary>
        public static void ClearConsole()
        {
            try
            {
                // Пытаемся очистить консоль
                Console.Clear();
            }
            catch
            {
                // Если консоль недоступна (например, редирект в файл) - игнорируем ошибку
                // Программа продолжит работу нормально
            }
        }

        public static void PrintHeader(string title)
        {
            // Выводим текст заголовка в центре внимания
            Console.WriteLine(title);
        }

        /// <summary>
        /// Выводит информацию о составе и характеристиках одной армии.
        /// Показывает название, бюджет, и список всех юнитов с их параметрами.
        /// </summary>
        public static void DisplayArmyComposition(string armyName, ConsoleColor color, List<UnitSaveData> units, int totalCost)
        {
            // Устанавливаем цвет для армии (красный, синий и т.д.)
            Console.ForegroundColor = color;
            
            // Выводим название армии и её общий бюджет
            Console.WriteLine($"\n{armyName} (Бюджет: {totalCost}):");
            
            // Сбрасываем цвет в стандартный белый
            Console.ResetColor();
            
            // Выводим заголовок для списка юнитов
            Console.WriteLine("Состав:");
            
            // Итерируемся по каждому юниту в армии
            foreach (var unit in units)
            {
                // Преобразуем тип юнита из английского в русское название
                string unitType = unit.Type switch
                {
                    "WeakFighter" => "Слабый боец",
                    "Archer" => "Лучник",
                    "StrongFighter" => "Сильный боец",
                    // Если тип неизвестен - используем оригинальное имя
                    _ => unit.Type
                };
                
                // Получаем максимальное здоровье для этого типа юнита
                int maxHealth = GetMaxHealth(unit.Type);
                
                // Выводим информацию о юните в формате: 1 - Слабый боец (HP: 25/25, ATK: 10, DEF: 8, Стоимость: 15)
                Console.WriteLine($"  {unit.FighterNumber} - {unitType} (HP: {unit.Health}/{maxHealth}, ATK: {unit.Attack}, DEF: {unit.Defence}, Стоимость: {unit.Cost})");
            }
        }

        /// <summary>
        /// Отображает список файлов и позволяет пользователю выбрать один.
        /// Возвращает номер выбранного файла (1-based индекс) или 0 для выхода.
        /// </summary>
        public static int ShowFileMenu(string[] files, string title)
        {
            // Очищаем экран консоли перед отображением меню
            ClearConsole();
            
            // Выводим красивый заголовок меню
            PrintHeader($"        {title}");
            
            // Выводим подзаголовок для списка
            Console.WriteLine("Доступные варианты:");
            
            // Итерируемся по всем файлам в массиве
            for (int i = 0; i < files.Length; i++)
            {
                // Выводим номер (начиная с 1) и имя файла
                // Пример: "1. Armies_20260228_130603"
                Console.WriteLine($"{i + 1}. {files[i]}");
            }

            // Просим пользователя выбрать опцию
            Console.Write("\nВыберите номер (0 - выход): ");
            
            // Пытаемся прочитать числовой ввод пользователя
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                // Если ввод успешно преобразован в число - возвращаем его
                return choice;
            }
            
            // Если ввод не число - возвращаем -1 (ошибка ввода)
            return -1;
        }

        /// <summary>
        /// Получает строку от пользователя с подсказкой (prompt).
        /// </summary>
        public static string GetInput(string prompt)
        {
            // Выводим подсказку без переноса строки
            Console.Write(prompt);
            
            // Читаем строку с клавиатуры и возвращаем её
            return Console.ReadLine();
        }

        /// <summary>
        /// Получает целое число от пользователя с возвращением значения по умолчанию при ошибке.
        /// </summary>
        public static int GetIntInput(string prompt, int defaultValue = 0)
        {
            // Выводим подсказку для пользователя
            Console.Write(prompt);
            
            // Пытаемся прочитать введенное число
            if (int.TryParse(Console.ReadLine(), out int result))
                // Если успешно - возвращаем введенное число
                return result;
            
            // Если ошибка парсинга - возвращаем значение по умолчанию
            return defaultValue;
        }

        /// <summary>
        /// Ждет нажатия любой клавиши и выводит сообщение об этом.
        /// Используется для паузы перед переходом на следующий экран.
        /// </summary>
        public static void WaitForKey(string message = "\nНажмите любую клавишу")
        {
            // Выводим сообщение об ожидании
            Console.WriteLine(message);
            
            // Ждем нажатия любой клавиши
            Console.ReadKey();
        }

        /// <summary>
        /// Возвращает максимальное здоровье юнита по его типу.
        /// Используется для отображения текущего/ максимального здоровья.
        /// </summary>
        private static int GetMaxHealth(string unitType)
        {
            // Используем switch выражение для определения максимального здоровья
            return unitType switch
            {
                // Слабые бойцы имают максимум 25 HP
                "WeakFighter" => 25,
                
                // Лучники имеют максимум 18 HP
                "Archer" => 18,
                
                // Сильные бойцы имают максимум 60 HP
                "StrongFighter" => 60,
                
                // Если тип неизвестен - возвращаем 0
                _ => 0
            };
        }

        /// <summary>
        /// Выводит сообщение об ошибке в красном цвете.
        /// </summary>
        public static void ShowError(string message)
        {
            // Выводим сообщение об ошибке с префиксом "ОШИБКА:"
            Console.WriteLine($"ОШИБКА: {message}");
        }

        /// <summary>
        /// Выводит сообщение об успехе
        /// </summary>
        public static void ShowSuccess(string message)
        {
           
            Console.WriteLine($"{message}");
        }

        /// <summary>
        /// Выводит обычное информационное сообщение.
        /// </summary>
        public static void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
