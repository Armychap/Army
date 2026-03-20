namespace RobotBuilder;

public class RobotDemo
{
    private readonly CombatRobotBuilder _combatBuilder = new();
    private readonly WorkerRobotBuilder _workerBuilder = new();
    private readonly CarrierRobotBuilder _carrierBuilder = new();

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
                    BuildCombatRobot();
                    break;
                case "2":
                    BuildWorkerRobot();
                    break;
                case "3":
                    BuildCarrierRobot();
                    break;
                case "4":
                    BuildCustomRobot();
                    break;
                case "0":
                    exit = true;
                    Console.WriteLine("\nВыход из программы...");
                    break;
                default:
                    Console.WriteLine("\nНеверный выбор.");
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
        Console.WriteLine("  КОНСТРУКТОР РОБОТОВ  ");
        Console.WriteLine("1. Собрать боевого робота");
        Console.WriteLine("2. Собрать рабочего робота");
        Console.WriteLine("3. Собрать робота-перевозчика");
        Console.WriteLine("4. Собрать своего робота");
        Console.WriteLine("0. Выход");
        Console.Write("\nВаш выбор: ");
    }

    private void BuildCombatRobot()
    {
        Console.WriteLine("\n   СБОРКА БОЕВОГО РОБОТА ");
        RobotDirector director = new(_combatBuilder);
        director.ConstructFullRobot();
        Robot robot = _combatBuilder.GetRobot();
        robot.ShowComponents();
    }

    private void BuildWorkerRobot()
    {
        Console.WriteLine("\n   СБОРКА РАБОЧЕГО РОБОТА ");
        RobotDirector director = new(_workerBuilder);
        director.ConstructFullRobot();
        Robot robot = _workerBuilder.GetRobot();
        robot.ShowComponents();
    }

    private void BuildCarrierRobot()
    {
        Console.WriteLine("\n   СБОРКА РОБОТА-ПЕРЕВОЗЧИКА ");
        RobotDirector director = new(_carrierBuilder);
        director.ConstructFullRobot();
        Robot robot = _carrierBuilder.GetRobot();
        robot.ShowComponents();
    }

    private void BuildCustomRobot()
    {
        Robot robot = new Robot();
        robot.SetName("Кастомный робот");

        Console.Write("Введите имя робота (Enter - имя по умолчанию): ");
        string robotName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(robotName))
        {
            robot.SetName(robotName);
        }

        // пошаговая сборка
        bool assembling = true;
        while (assembling)
        {
            Console.Clear();
            Console.WriteLine($"\n   СБОРКА: {robotName ?? "Кастомный робот"} ");
            Console.WriteLine("Текущие компоненты:");
            robot.ShowComponents();

            Console.WriteLine("\nЧто хотите добавить?");
            Console.WriteLine("1. Голову");
            Console.WriteLine("2. Корпус");
            Console.WriteLine("3. Руки");
            Console.WriteLine("4. Ноги");
            Console.WriteLine("5. Оружие");
            Console.WriteLine("6. Спец. оснащение");
            Console.WriteLine("0. Завершить сборку и показать результат");
            Console.Write("Ваш выбор: ");

            string partChoice = Console.ReadLine();

            switch (partChoice)
            {
                case "1": // голова
                    Console.WriteLine("\nВыберите тип головы:");
                    Console.WriteLine("1. Боевой процессор");
                    Console.WriteLine("2. Стандартный процессор");
                    Console.WriteLine("3. Дорожный сканер");
                    Console.Write("Ваш выбор: ");

                    string head = Console.ReadLine() switch
                    {
                        "1" => "Боевой процессор",
                        "2" => "Стандартный процессор",
                        "3" => "Гуманоидная голова с эмоциональным ИИ",
                        _ => "Базовая голова"
                    };
                    robot.AddHead(head);
                    Console.WriteLine($"  Добавлено: {head}");
                    break;

                case "2": // корпус
                    Console.WriteLine("\nВыберите тип корпуса:");
                    Console.WriteLine("1. Титановая броня");
                    Console.WriteLine("2. Легкий алюминиевый корпус");
                    Console.WriteLine("3. Корпус с магнитом");
                    Console.Write("Ваш выбор: ");

                    string body = Console.ReadLine() switch
                    {
                        "1" => "Титановая броня",
                        "2" => "Легкий алюминиевый корпус",
                        "3" => "Корпус с магнитом",
                        _ => "Стандартный корпус"
                    };
                    robot.AddBody(body);
                    Console.WriteLine($"  Добавлено: {body}");
                    break;

                case "3": // руки
                    Console.WriteLine("\nВыберите тип рук:");
                    Console.WriteLine("1. Гидравлические манипуляторы");
                    Console.WriteLine("2. Клешни и инструменты");
                    Console.WriteLine("3. Поворотные краны");
                    Console.Write("Ваш выбор: ");

                    string arms = Console.ReadLine() switch
                    {
                        "1" => "Гидравлические манипуляторы",
                        "2" => "Клешни и инструменты",
                        "3" => "Поворотные краны",
                        _ => "Стандартные манипуляторы"
                    };
                    robot.AddArms(arms);
                    Console.WriteLine($"  Добавлено: {arms}");
                    break;

                case "4": // ноги
                    Console.WriteLine("\nВыберите тип ног:");
                    Console.WriteLine("1. Гусеничная база");
                    Console.WriteLine("2. Двухколесная база");
                    Console.WriteLine("3. Четырехколесная база");
                    Console.Write("Ваш выбор: ");

                    string legs = Console.ReadLine() switch
                    {
                        "1" => "Гусеничная база",
                        "2" => "Двухколесная база",
                        "3" => "Четырехколесная база",
                        _ => "Стандартные ноги"
                    };
                    robot.AddLegs(legs);
                    Console.WriteLine($"  Добавлено: {legs}");
                    break;

                case "5": // оружие
                    Console.WriteLine("\nВыберите вооружение:");
                    Console.WriteLine("1. Плазменная пушка");
                    Console.WriteLine("2. Электрошокер");
                    Console.WriteLine("3. Без оружия");
                    Console.Write("Ваш выбор: ");

                    string weapon = Console.ReadLine() switch
                    {
                        "1" => "Плазменная пушка",
                        "2" => "Электрошокер",
                        "3" => "Без оружия",
                        _ => "Без оружия"
                    };

                    if (weapon != "Без оружия")
                    {
                        robot.AddWeapon(weapon);
                        Console.WriteLine($"  Добавлено: {weapon}");
                    }
                    else
                    {
                        Console.WriteLine("  Оружие не добавлено.");
                    }
                    break;

                case "6": // спец оснащение
                    Console.WriteLine("\nВыберите спец оснащение:");
                    Console.WriteLine("1. Система ночного видения");
                    Console.WriteLine("2. Грузовой отсек");
                    Console.WriteLine("3. Канаты со стропами");
                    Console.WriteLine("4. Без спец. оснащения");
                    Console.Write("Ваш выбор: ");

                    string special = Console.ReadLine() switch
                    {
                        "1" => "Система ночного видения",
                        "2" => "Грузовой отсек",
                        "3" => "Канаты со стропами",
                        "4" => "Без спец. оснащения",
                        _ => "Без спец. оснащения"
                    };

                    if (special != "Без спец. оснащения")
                    {
                        robot.AddSpecialEquipment(special);
                        Console.WriteLine($"  Добавлено: {special}");
                    }
                    else
                    {
                        Console.WriteLine("  Спец. оснащение не добавлено.");
                    }
                    break;

                case "0":
                    assembling = false;
                    break;

                default:
                    Console.WriteLine("Неверный выбор! Нажмите любую клавишу и попробуйте снова.");
                    Console.ReadKey();
                    break;
            }

            if (partChoice != "0" && partChoice != "default")
            {
                Console.WriteLine("\nНажмите любую клавишу для продолжения сборки...");
                Console.ReadKey();
            }
        }

        Console.Clear();
        Console.WriteLine(" СБОРКА ЗАВЕРШЕНА ");
        robot.ShowComponents();

        Console.WriteLine("\nЧто делаем дальше?");
        Console.WriteLine("1. Сохранить робота (имитация)");
        Console.WriteLine("2. Отправить на задание (имитация)");
        Console.WriteLine("Любая другая клавиша - вернуться в меню");
        Console.Write("Ваш выбор: ");

        string finalChoice = Console.ReadLine();
        if (finalChoice == "1")
        {
            Console.WriteLine("\nРобот сохранён в базу данных!");
        }
        else if (finalChoice == "2")
        {
            Console.WriteLine($"\nРобот \"{robotName}\" отправлен на задание!");
        }
    }
}