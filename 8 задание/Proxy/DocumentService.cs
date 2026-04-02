using Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Proxy
{
    public class DocumentService : IDocumentService
    {
        private readonly Dictionary<int, Document> _documents = new();
        private int _nextId = 1;

        public DocumentService()
        {
            Console.WriteLine("  [RealService] Инициализация реального сервиса документов...");

            _documents[_nextId++] = new Document(1, "Приказ N1", "О выплате премии", "HR");
            _documents[_nextId++] = new Document(2, "Финансовый отчет", "Бюджет на Q2 2026", "Finance");
            _documents[_nextId++] = new Document(3, "Техническая документация", "Архитектура системы", "IT");
            _documents[_nextId++] = new Document(4, "Кадровый приказ", "О назначении на должность", "HR");

            Console.WriteLine("  [RealService] Загружено 4 тестовых документа");
        }

        public string ReadDocument(int docId)
        {
            Console.WriteLine($"  [RealService] Чтение документа {docId} из БД...");
            Thread.Sleep(200);

            if (_documents.ContainsKey(docId))
                return _documents[docId].Content;
            return "ОШИБКА: Документ не найден";
        }

        public void WriteDocument(int docId, string content)
        {
            Console.WriteLine($"  [RealService] Сохранение документа {docId} в БД...");
            Thread.Sleep(200);

            if (_documents.ContainsKey(docId))
                _documents[docId].Content = content;
            else
                Console.WriteLine($"  [RealService] ОШИБКА: Документ {docId} не существует");
        }

        public void DeleteDocument(int docId)
        {
            Console.WriteLine($"  [RealService] Удаление документа {docId} из БД...");
            Thread.Sleep(200);

            if (_documents.ContainsKey(docId))
                _documents.Remove(docId);
            else
                Console.WriteLine($"  [RealService] ОШИБКА: Документ {docId} не существует");
        }

        public void CreateDocument(string title, string content)
        {
            Console.WriteLine($"  [RealService] Создание нового документа '{title}' в БД...");
            Thread.Sleep(200);
            _documents[_nextId] = new Document(_nextId, title, content, "General");
            _nextId++;
        }

        public List<string> GetAllDocuments()
        {
            Console.WriteLine($"  [RealService] Получение списка всех документов из БД...");
            Thread.Sleep(200);
            return _documents.Values.Select(d => $"ID:{d.Id} | {d.Title} | Отдел: {d.Department}").ToList();
        }

        public string GetDocumentInfo(int docId)
        {
            Console.WriteLine($"  [RealService] Получение информации о документе {docId}...");
            if (_documents.ContainsKey(docId))
            {
                var d = _documents[docId];
                return $"ID:{d.Id} | Название:{d.Title} | Отдел:{d.Department}";
            }
            return "Документ не найден";
        }
    }
}