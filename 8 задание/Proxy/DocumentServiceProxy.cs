using Proxy;
using System;
using System.Collections.Generic;

namespace Proxy
{
    public class DocumentServiceProxy : IDocumentService
    {
        private readonly DocumentService _realService;
        private readonly User _currentUser;

        public DocumentServiceProxy(User currentUser)
        {
            _currentUser = currentUser;
            _realService = new DocumentService();
            Console.WriteLine($"  [Proxy] Прокси создан для пользователя: {currentUser.Name} (роль: {currentUser.Role})");
            Console.WriteLine($"  [Proxy] Прокси перехватывает все вызовы и проверяет права доступа");
        }

        // Чтение документа. Права: все, кроме гостей.
        public string ReadDocument(int docId)
        {
            Console.WriteLine($"\n  [Proxy] Перехвачен вызов ReadDocument({docId})");
            Console.WriteLine($"  [Proxy] Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role == "Guest")
            {
                Console.WriteLine($"  [Proxy] ДОСТУП ЗАПРЕЩЕН: Гости не могут читать документы");
                return "Доступ запрещен: Гости не могут читать документы";
            }

            Console.WriteLine($"  [Proxy] ДОСТУП РАЗРЕШЕН для роли '{_currentUser.Role}'");
            Console.WriteLine($"  [Proxy] Передача вызова реальному объекту...");
            return _realService.ReadDocument(docId);
        }

        // Запись документа. Права: только Manager и Admin.
        public void WriteDocument(int docId, string content)
        {
            Console.WriteLine($"\n  [Proxy] Перехвачен вызов WriteDocument({docId})");
            Console.WriteLine($"  [Proxy] Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role != "Manager" && _currentUser.Role != "Admin")
            {
                Console.WriteLine($"  [Proxy] ДОСТУП ЗАПРЕЩЕН: Роль '{_currentUser.Role}' не может изменять документы");
                return;
            }

            Console.WriteLine($"  [Proxy] ДОСТУП РАЗРЕШЕН для роли '{_currentUser.Role}'");
            Console.WriteLine($"  [Proxy] Передача вызова реальному объекту...");
            _realService.WriteDocument(docId, content);
        }

        // Удаление документа. Права: только Admin.
        public void DeleteDocument(int docId)
        {
            Console.WriteLine($"\n  [Proxy] Перехвачен вызов DeleteDocument({docId})");
            Console.WriteLine($"  [Proxy] Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role != "Admin")
            {
                Console.WriteLine($"  [Proxy] ДОСТУП ЗАПРЕЩЕН: Только администраторы могут удалять документы");
                return;
            }

            Console.WriteLine($"  [Proxy] ДОСТУП РАЗРЕШЕН для роли 'Admin'");
            Console.WriteLine($"  [Proxy] Передача вызова реальному объекту...");
            _realService.DeleteDocument(docId);
        }

        // Создание документа. Права: Manager и Admin.
        public void CreateDocument(string title, string content)
        {
            Console.WriteLine($"\n  [Proxy] Перехвачен вызов CreateDocument('{title}')");
            Console.WriteLine($"  [Proxy] Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role != "Manager" && _currentUser.Role != "Admin")
            {
                Console.WriteLine($"  [Proxy] ДОСТУП ЗАПРЕЩЕН: Роль '{_currentUser.Role}' не может создавать документы");
                return;
            }

            Console.WriteLine($"  [Proxy] ДОСТУП РАЗРЕШЕН для роли '{_currentUser.Role}'");
            Console.WriteLine($"  [Proxy] Передача вызова реальному объекту...");
            _realService.CreateDocument(title, content);
        }

        // Получение списка документов. Права: все, кроме гостей.
        public List<string> GetAllDocuments()
        {
            Console.WriteLine($"\n  [Proxy] Перехвачен вызов GetAllDocuments()");
            Console.WriteLine($"  [Proxy] Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role == "Guest")
            {
                Console.WriteLine($"  [Proxy] ДОСТУП ЗАПРЕЩЕН: Гости не могут просматривать список документов");
                return new List<string> { "Доступ запрещен: Гости не могут просматривать список документов" };
            }

            Console.WriteLine($"  [Proxy] ДОСТУП РАЗРЕШЕН для роли '{_currentUser.Role}'");
            Console.WriteLine($"  [Proxy] Передача вызова реальному объекту...");
            return _realService.GetAllDocuments();
        }

        // Получение информации о документе. Права: все, кроме гостей.
        public string GetDocumentInfo(int docId)
        {
            Console.WriteLine($"\n  [Proxy] Перехвачен вызов GetDocumentInfo({docId})");
            Console.WriteLine($"  [Proxy] Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role == "Guest")
            {
                Console.WriteLine($"  [Proxy] ДОСТУП ЗАПРЕЩЕН: Гости не могут получать информацию о документах");
                return "Доступ запрещен: Гости не могут получать информацию о документах";
            }

            Console.WriteLine($"  [Proxy] ДОСТУП РАЗРЕШЕН для роли '{_currentUser.Role}'");
            Console.WriteLine($"  [Proxy] Передача вызова реальному объекту...");
            return _realService.GetDocumentInfo(docId);
        }
    }
}