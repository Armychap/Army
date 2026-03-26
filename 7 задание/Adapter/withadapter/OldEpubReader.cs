using System;
using System.Collections.Generic;
using System.IO;

namespace BookReaderWithAdapter
{
    // Старый класс для чтения EPUB книг (Adaptee)
    // Существующий класс, который нельзя изменить
    class OldEpubReader
    {
        // путь к текущей книге
        private string _currentBookPath;
        // текущая страница
        private int _currentPage;
        // общее количество страниц
        private int _totalPages;
        // название книги
        private string _bookTitle;
        // список закладок
        private List<int> _bookmarks = new List<int>();
        
        // открывает EPUB файл
        public void OpenEpub(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            
            _currentBookPath = filePath;
            _currentPage = 1;
            _totalPages = (int)(new FileInfo(filePath).Length / 1024);
            _bookTitle = Path.GetFileNameWithoutExtension(filePath);
        }
        
        // отображает страницу
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
        
        // добавляет закладку
        public void AddBookmark()
        {
            if (!_bookmarks.Contains(_currentPage))
                _bookmarks.Add(_currentPage);
        }
        
        // получает закладки
        public List<int> GetBookmarks() => _bookmarks;
        
        // переходит к закладке
        public void GoToBookmark(int index)
        {
            if (index >= 0 && index < _bookmarks.Count)
                _currentPage = _bookmarks[index];
        }
        
        // закрывает книгу
        public void CloseEpub()
        {
            _currentBookPath = null;
            _currentPage = 0;
            _totalPages = 0;
            _bookTitle = null;
        }
        
        // получает информацию
        public string GetBookInfo() => $"{_bookTitle} - {_currentPage}/{_totalPages}";
        
        // открыта ли книга
        public bool IsBookOpened => !string.IsNullOrEmpty(_currentBookPath);
        // текущая страница
        public int CurrentPage => _currentPage;
        // общее число страниц
        public int TotalPages => _totalPages;
    }
}