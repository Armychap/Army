using System;
using System.Collections.Generic;
using System.IO;

namespace BookReaderWithoutAdapter
{
    // класс для чтения EPUB книг
    class OldEpubReader
    {
        private string _currentBookPath;
        private int _currentPage;
        private int _totalPages;
        private string _bookTitle;
        private List<int> _bookmarks = new List<int>();
        
        // метод открытия EPUB
        public void OpenEpub(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            
            _currentBookPath = filePath;
            _currentPage = 1;
            _totalPages = (int)(new FileInfo(filePath).Length / 1024);
            _bookTitle = Path.GetFileNameWithoutExtension(filePath);
        }
        
        // метод отображения страницы
        public void DisplayPage(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > _totalPages)
                throw new ArgumentOutOfRangeException($"Страница {pageNumber} не существует");
            
            _currentPage = pageNumber;
        }
        
        // переходит на следующую страницу
        public void NextPage()
        {
            if (_currentPage < _totalPages)
                _currentPage++;
        }
        
        // переходит на предыдущую страницу
        public void PreviousPage()
        {
            if (_currentPage > 1)
                _currentPage--;
        }
        
        // добавляет закладку на текущей странице
        public void AddBookmark()
        {
            if (!_bookmarks.Contains(_currentPage))
                _bookmarks.Add(_currentPage);
        }
        
        // получает список закладок
        public List<int> GetBookmarks() => _bookmarks;
        
        // переходит к закладке по номеру
        public void GoToBookmark(int index)
        {
            if (index >= 0 && index < _bookmarks.Count)
                _currentPage = _bookmarks[index];
        }
        
        // метод закрытия EPUB
        public void CloseEpub()
        {
            _currentBookPath = null;
            _currentPage = 0;
            _totalPages = 0;
            _bookTitle = null;
        }
        
        // получает информацию о книге
        public string GetBookInfo() => $"{_bookTitle} - {_currentPage}/{_totalPages}";
        
        // проверяет, открыта ли книга
        public bool IsBookOpened => !string.IsNullOrEmpty(_currentBookPath);
        // текущая страница
        public int CurrentPage => _currentPage;
        // общее число страниц
        public int TotalPages => _totalPages;
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            
            // Создаем тестовый файл книги
            string testFile = "book.epub";
            if (!File.Exists(testFile))
            {
                File.WriteAllText(testFile, new string('A', 102400));
                Console.WriteLine("Создана тестовая книга\n");
            }
            
            // создаем объект читателя
            OldEpubReader reader = new OldEpubReader();
            
            Console.WriteLine("Открываем EPUB книгу");
            reader.OpenEpub(testFile);
            Console.WriteLine($"   {reader.GetBookInfo()}\n");
            
            Console.WriteLine("Переходим на страницу 5");
            reader.DisplayPage(5);
            reader.AddBookmark();
            Console.WriteLine($"Закладка добавлена на странице 5\n");
            
            // Навигация по страницам
            Console.WriteLine("Листаем вперед");
            reader.NextPage();
            Console.WriteLine($"Текущая страница: {reader.CurrentPage}");
            reader.NextPage();
            Console.WriteLine($"Текущая страница: {reader.CurrentPage}\n");
            
            Console.WriteLine($"Информация: {reader.GetBookInfo()}");
            Console.WriteLine($"Закладки: {string.Join(", ", reader.GetBookmarks())}\n");
            
            Console.WriteLine("Закрываем книгу");
            reader.CloseEpub();
            
        }
    }
}