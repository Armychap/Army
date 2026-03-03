using System;
using System.Collections.Generic;
using System.Linq;

namespace NotificationSystem
{
    public class Menu
    {
        private NotificationManager _manager;

        public Menu()
        {
            _manager = NotificationManager.Instance;
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine("\n1. Инфо");
                Console.WriteLine("2. Успех");
                Console.WriteLine("3. Предупреждение");
                Console.WriteLine("4. Ошибка");
                Console.WriteLine("5. Показать все");
                Console.WriteLine("6. Непрочитанные");
                Console.WriteLine("7. Отметить прочитанным");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");

                switch (Console.ReadLine())
                {
                    case "1": _manager.Send("Информация", NotificationType.Info); break;
                    case "2": _manager.Send("Операция успешна", NotificationType.Success); break;
                    case "3": _manager.Send("Внимание!", NotificationType.Warning); break;
                    case "4": _manager.Send("Ошибка!", NotificationType.Error); break;
                    case "5": _manager.ShowAll(); break;
                    case "6": _manager.ShowUnread(); break;
                    case "7":
                        Console.Write("Номер: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                            _manager.MarkAsRead(id);
                        else
                            Console.WriteLine("Некорректный номер");
                        break;
                    case "0": return;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }
        }
    }
}