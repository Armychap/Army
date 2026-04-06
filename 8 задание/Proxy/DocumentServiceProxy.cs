using Proxy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Proxy
{
    public class DocumentServiceProxy : IDocumentService
    {
        private readonly DocumentService _realService;
        private readonly User _currentUser;

        private readonly Dictionary<int, string> _documentCache = new();  // Кэш для документов (Id, содержимое)

        private readonly Dictionary<int, string> _documentInfoCache = new();  // Кэш для инфы документов (Id, содержимое)

        private List<string> _allDocumentsCache;  // Кэш для списка всех документов
        private bool _allDocumentsCacheValid = false;

        public DocumentServiceProxy(User currentUser)
        {
            _currentUser = currentUser;
            _realService = new DocumentService();

            LogInfo($"Прокси создан для пользователя: {currentUser.Name} (роль: {currentUser.Role})");
            LogCache($"Включено кэширование результатов");
        }

        // Чтение документа (права: все, кроме гостей)
        public string ReadDocument(int docId)
        {
            Console.WriteLine();
            LogAccess($"Перехвачен вызов ReadDocument({docId})");
            LogSecurity($"Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role == "Guest")
            {
                LogSecurity("ДОСТУП ЗАПРЕЩЕН:");
                return "Гости не могут читать документы";
            }

            LogSecurity($"ДОСТУП РАЗРЕШЕН для роли '{_currentUser.Role}'");

            // Проверка кэша
            if (_documentCache.ContainsKey(docId))
            {
                LogCache($"Возвращаем закэшированное содержимое документа {docId}");
                return _documentCache[docId];
            }

            LogCache($"Данных нет, обращаемся к реальному сервису");
            string content = _realService.ReadDocument(docId);

            // Сохраняем в кэш только если документ найден
            if (!content.StartsWith("ОШИБКА"))
            {
                _documentCache[docId] = content;
                LogCache($"Сохранено содержимое документа {docId}");
            }

            return content;
        }

        // Запись документа (права: все, кроме гостей)
        public void WriteDocument(int docId, string content)
        {
            Console.WriteLine();
            LogAccess($"Перехвачен вызов WriteDocument({docId})");
            LogSecurity($"Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role != "Manager" && _currentUser.Role != "Admin")
            {
                LogSecurity($"ДОСТУП ЗАПРЕЩЕН: Роль '{_currentUser.Role}' не может изменять документы");
                return;
            }

            LogSecurity($"ДОСТУП РАЗРЕШЕН для роли '{_currentUser.Role}'");
            _realService.WriteDocument(docId, content);

            InvalidateCache(docId);  // инвалидация кэша после изменения
        }

        // Удаление документа (права: только админ)
        public void DeleteDocument(int docId)
        {
            Console.WriteLine();
            LogAccess($"Перехвачен вызов DeleteDocument({docId})");
            LogSecurity($"Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role != "Admin")
            {
                LogSecurity($"ДОСТУП ЗАПРЕЩЕН: Только администраторы могут удалять документы");
                return;
            }

            LogSecurity($"ДОСТУП РАЗРЕШЕН для роли 'Admin'");
            _realService.DeleteDocument(docId);

            InvalidateCache(docId);
        }

        // Создание документа (права: менеджер и админ)
        public void CreateDocument(string title, string content)
        {
            Console.WriteLine();
            LogAccess($"Перехвачен вызов CreateDocument('{title}')");
            LogSecurity($"Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role != "Manager" && _currentUser.Role != "Admin")
            {
                LogSecurity($"ДОСТУП ЗАПРЕЩЕН: Роль '{_currentUser.Role}' не может создавать документы");
                return;
            }

            LogSecurity($"ДОСТУП РАЗРЕШЕН для роли '{_currentUser.Role}'");
            _realService.CreateDocument(title, content);

            _allDocumentsCacheValid = false;  // Инвалидация кэша списка документов (появился новый документ)
            LogCache($"Инвалидирован кэш списка документов");
        }

        // Получение списка документов (права: все, кроме гостей)
        public List<string> GetAllDocuments()
        {
            Console.WriteLine();
            LogAccess($"Перехвачен вызов GetAllDocuments()");
            LogSecurity($"Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role == "Guest")
            {
                LogSecurity("ДОСТУП ЗАПРЕЩЕН:");
                return new List<string> { "Гости не могут просматривать список документов" };
            }

            LogSecurity($"ДОСТУП РАЗРЕШЕН для роли '{_currentUser.Role}'");

            // Проверка кэша
            if (_allDocumentsCacheValid && _allDocumentsCache != null)
            {
                LogCache($"Возвращаем закэшированный список документов");
                return _allDocumentsCache;
            }

            LogCache($"Данных нет, обращаемся к реальному сервису");
            _allDocumentsCache = _realService.GetAllDocuments();
            _allDocumentsCacheValid = true;
            LogCache($"Сохранен список документов ({_allDocumentsCache.Count} записей)");

            return _allDocumentsCache;
        }

        // Получение информации о документе (права: все, кроме гостей)
        public string GetDocumentInfo(int docId)
        {
            Console.WriteLine();
            LogAccess($"Перехвачен вызов GetDocumentInfo({docId})");
            LogSecurity($"Проверка прав для роли '{_currentUser.Role}'...");

            if (_currentUser.Role == "Guest")
            {
                LogSecurity("ДОСТУП ЗАПРЕЩЕН:");
                return "Гости не могут получать информацию о документах";
            }

            LogSecurity($"ДОСТУП РАЗРЕШЕН для роли '{_currentUser.Role}'");

            // Проверка кэша
            if (_documentInfoCache.ContainsKey(docId))
            {
                LogCache($"Возвращаем закэшированную информацию о документе {docId}");
                return _documentInfoCache[docId];
            }

            LogCache($"Данных нет, обращаемся к реальному сервису");
            string info = _realService.GetDocumentInfo(docId);

            if (!info.Contains("не найден"))
            {
                _documentInfoCache[docId] = info;
                LogCache($"Сохранена информация о документе {docId}");
            }

            return info;
        }

        // Метод для инвалидации кэша при изменении документа
        private void InvalidateCache(int docId)
        {
            if (_documentCache.ContainsKey(docId))
            {
                _documentCache.Remove(docId);
                LogCache($"Удалено содержимое документа {docId} из кэша");
            }

            if (_documentInfoCache.ContainsKey(docId))
            {
                _documentInfoCache.Remove(docId);
                LogCache($"Удалена информация о документе {docId} из кэша");
            }

            // Список документов тоже мог измениться
            _allDocumentsCacheValid = false;
            LogCache($"Инвалидирован кэш списка документов");
        }

        // Методы логирования
        private void LogAccess(string message)
        {
            Console.WriteLine($"[ACCESS] {message}");
        }

        private void LogSecurity(string message)
        {
            Console.WriteLine($"[SECURITY] {message}");
        }

        private void LogCache(string message)
        {
            Console.WriteLine($"[CACHE] {message}");
        }

        private void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }
    }
}