using System;
using System.Collections.Generic;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Контейнер для сохранения данных двух армий в JSON формате.
    /// </summary>
    public class ArmySaveData
    {
        // ДАННЫЕ ПЕРВОЙ АРМИИ
        public string? Army1Name { get; set; }
        
        public string? Army1Prefix { get; set; }
        
        // Цвет консоли для отображения первой армии (есть, Blue, Green и т.д.).

        public ConsoleColor Army1Color { get; set; }
        
        /// <summary>
        /// Список всех юнитов первой армии в сохраняемом формате.
        /// Каждый элемент содержит параметры и характеристики одного боя.
        /// </summary>
        public List<UnitSaveData> ?Army1Units { get; set; }
        
        /// <summary>
        /// Общая стоимость всех юнитов первой армии.
        /// Используется для информации о бюджете при просмотре сохранения.
        /// </summary>
        public int TotalCost1 { get; set; }

        //ДАННЫЕ ВТОРОЙ АРМИИ
        
        public string? Army2Name { get; set; }
        public string? Army2Prefix { get; set; }
        
        // Цвет консоли для отображения второй армии 
        public ConsoleColor Army2Color { get; set; }
        
        /// <summary>
        /// Список всех юнитов второй армии в сохраняемом формате.
        /// Каждый элемент содержит параметры и характеристики одного боя.
        /// </summary>
        public List<UnitSaveData> ?Army2Units { get; set; }
        
        /// <summary>
        /// Общая стоимость всех юнитов второй армии.
        /// Используется для информации о бюджете при просмотре сохранения.
        /// </summary>
        public int TotalCost2 { get; set; }
        
        /// <summary>
        /// Дата и время создания этого сохранения.
        /// Используется для отслеживания истории боев.
        /// </summary>
        public DateTime SaveDate { get; set; }
        
        /// <summary>
        /// Текущий раунд битвы.
        /// </summary>
        public int CurrentRound { get; set; }
        
        /// <summary>
        /// Текущий ход атаки (0 или 1).
        /// </summary>
        public int AttackTurn { get; set; }
        
        /// <summary>
        /// Флаг, кто первый атакующий (true - армия 1, false - армия 2).
        /// </summary>
        public bool FirstAttackerIsArmy1 { get; set; }
        
        /// <summary>
        /// Флаг, нужно ли новый заголовок раунда.
        /// </summary>
        public bool NeedNewRoundHeader { get; set; }
        
        /// <summary>
        /// Имя файла лога битвы для продолжения.
        /// </summary>
        public string? BattleLogName { get; set; }
    }

    /// <summary>
    /// Сохраняемое состояние одного юнита (боя)
    /// </summary>
    public class UnitSaveData
    {
        /// <summary>
        /// Тип юнита в виде строки (например: "WeakFighter", "Archer", "StrongFighter").
        /// Используется для определения, какой класс создавать при загрузке.
        /// </summary>
        public string? Type { get; set; }
        
        /// <summary>
        /// Номер боя внутри армии (1, 2, 3 и т.д.).
        /// Используется для идентификации юнита и отображения в интерфейсе.
        /// </summary>
        public int FighterNumber { get; set; }
        
        /// <summary>
        /// Текущее здоровье юнита.
        /// Может быть меньше максимума, если юнит был поврежден в предыдущем бою.
        /// </summary>
        public int Health { get; set; }
        
        /// <summary>
        /// Параметр атаки юнита - определяет урон при атаке.
        /// </summary>
        public int Attack { get; set; }
        
        /// <summary>
        /// Параметр защиты юнита - уменьшает получаемый урон.
        /// </summary>
        public int Defence { get; set; }
        
        /// <summary>
        /// Стоимость юнита
        /// </summary>
        public int Cost { get; set; }
    }
}
