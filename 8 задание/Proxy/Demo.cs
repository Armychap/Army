using Proxy;
using System;
using System.Collections.Generic;

namespace Proxy
{
    public class Demo
    {
        public void Run()
        {
            var guest = new User("Дмитрий", "Guest");
            var regularUser = new User("Яна", "User");
            var manager = new User("Ольга", "Manager");
            var admin = new User("Олег", "Admin");

            // Демонстрация для каждого пользователя
            Console.WriteLine("\n------------------------------");
            Console.WriteLine("Пользователь 'Guest'");
            DemonstrateUserActions(guest);

            Console.WriteLine("\n------------------------------");
            Console.WriteLine("Пользователь 'User'");
            DemonstrateUserActions(regularUser);

            Console.WriteLine("\n------------------------------");
            Console.WriteLine("Пользователь 'Manager'");
            DemonstrateUserActions(manager);

            Console.WriteLine("\n------------------------------");
            Console.WriteLine("Пользователь 'Admin'");
            DemonstrateUserActions(admin);
        }

        private void DemonstrateUserActions(User user)
        {
            IDocumentService service = new DocumentServiceProxy(user);

            Console.WriteLine($"\nДействия пользователя: {user.Name} (роль: {user.Role})");

            // 1. Получение списка документов
            Console.WriteLine("\n1. Запрос списка всех документов:");
            List<string> docs = service.GetAllDocuments();
            foreach (string doc in docs)
            {
                Console.WriteLine($"   {doc}");
            }

            // 2. Получение информации о документе
            Console.WriteLine("\n2. Запрос информации о документе N1:");
            string info = service.GetDocumentInfo(1);
            Console.WriteLine($"   {info}");

            // 3. Чтение документа
            Console.WriteLine("\n3. Попытка прочитать документ N1:");
            string content = service.ReadDocument(1);

            if (content.StartsWith("Гости не могут") || content.StartsWith("ОШИБКА"))
            {
                Console.WriteLine($"    {content}");
            }

            else
            {
                Console.WriteLine($"   Содержимое: {content}");
            }

            // 4. Создание документа
            Console.WriteLine("\n4. Попытка создать новый документ:");
            service.CreateDocument("Новый приказ", "Содержимое нового приказа");

            // 5. Изменение документа
            Console.WriteLine("\n5. Попытка изменить документ N2:");
            service.WriteDocument(2, "Измененное содержимое документа");

            // 6. Удаление документа
            Console.WriteLine("\n6. Попытка удалить документ N3:");
            service.DeleteDocument(3);
        }
    }
}