namespace RobotDecorator;

public class RobotDemo
{
    private List<IRobot> _availableRobots = new()
    {
        new CombatRobot(),
        new WorkerRobot(),
        new ScoutRobot()
    };

    public void Run()
    {
        bool exit = false;

        while (!exit)
        {
            ShowMainMenu();
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    BuildRobotWithDecorators();
                    break;
                case "0":
                    exit = true;
                    Console.WriteLine("\nВыход из программы...");
                    break;
                default:
                    Console.WriteLine("\nНеверный выбор. Попробуйте снова.");
                    break;
            }

            if (!exit)
            {
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }

    private void ShowMainMenu()
    {
        Console.WriteLine("Декоратор робота");
        Console.WriteLine("1. Собрать робота с улучшениями");
        Console.WriteLine("0. Выход");
        Console.Write("\nВаш выбор: ");
    }

    private void BuildRobotWithDecorators()
    {
        Console.Clear();
        Console.WriteLine("Доступные базовые модели роботов:");
        for (int i = 0; i < _availableRobots.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {_availableRobots[i].GetDescription()} (мощность: {_availableRobots[i].GetPower()})");
        }
        Console.Write("\nВыберите базу (1-3): ");
        if (!int.TryParse(Console.ReadLine(), out int baseChoice) || baseChoice < 1 || baseChoice > _availableRobots.Count)
        {
            Console.WriteLine("Неверный выбор базы.");
            return;
        }

        IRobot robot = _availableRobots[baseChoice - 1];
        bool adding = true;

        while (adding)
        {
            Console.Clear();
            Console.WriteLine(" ТЕКУЩАЯ КОНФИГУРАЦИЯ РОБОТА:");
            robot.ShowInfo();

            Console.WriteLine("\n Доступные улучшения:");
            Console.WriteLine("1. Добавить броню (+20 к мощности)");
            Console.WriteLine("2. Добавить оружие (+30 к мощности)");
            Console.WriteLine("3. Добавить спец. оснащение (+15 к мощности)");
            Console.WriteLine("0. Завершить сборку и показать результат");
            Console.Write("\nВаш выбор: ");

            string decoratorChoice = Console.ReadLine();

            switch (decoratorChoice)
            {
                case "1":
                    Console.Write("Введите тип брони (например: 'титановая'): ");
                    string armor = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(armor)) armor = "стандартная";
                    robot = new ArmorDecorator(robot, armor);
                    Console.WriteLine($"Добавлена {armor} броня!");
                    break;
                case "2":
                    Console.Write("Введите тип оружия (например: 'плазменная пушка'): ");
                    string weapon = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(weapon)) weapon = "стандартное оружие";
                    robot = new WeaponDecorator(robot, weapon);
                    Console.WriteLine($"Добавлено оружие: {weapon}!");
                    break;
                case "3":
                    Console.Write("Введите тип спец. оснащения (например: 'система ночного видения'): ");
                    string special = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(special)) special = "стандартное оснащение";
                    robot = new SpecialEquipmentDecorator(robot, special);
                    Console.WriteLine($"Добавлено оснащение: {special}!");
                    break;
                case "0":
                    adding = false;
                    break;
                default:
                    Console.WriteLine("Неверный выбор.");
                    break;
            }

            if (adding && decoratorChoice != "0")
            {
                Console.WriteLine("\nНажмите любую клавишу, чтобы продолжить...");
                Console.ReadKey();
            }
        }

        Console.Clear();
        Console.WriteLine("Итоговая конфигурация:");
        robot.ShowInfo();
        Console.WriteLine("\nСборка завершена!");
    }
}