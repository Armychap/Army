using System;
using System.Collections.Generic;
using System.IO;

namespace BookReaderWithAdapter
{
    // Новый формат книг, реализующий тот же интерфейс
    class PdfReader : IBookReader
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
        
        // открывает PDF файл
        public void Open(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            
            _currentBookPath = filePath;
            _currentPage = 1;
            _totalPages = (int)(new FileInfo(filePath).Length / 1024) * 2;
            _bookTitle = Path.GetFileNameWithoutExtension(filePath);
        }
        
        // закрывает PDF файл
        public void Close()
        {
            _currentBookPath = null;
            _currentPage = 0;
            _totalPages = 0;
            _bookTitle = null;
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
        
        // переходит на указанную страницу
        public void GoToPage(int pageNumber)
        {
            if (pageNumber >= 1 && pageNumber <= _totalPages)
                _currentPage = pageNumber;
        }
        
        // добавляет закладку
        public void AddBookmark()
        {
            if (!_bookmarks.Contains(_currentPage))
                _bookmarks.Add(_currentPage);
        }
        
        // получает список закладок
        public List<int> GetBookmarks() => _bookmarks;
        
        // переходит к закладке
        public void GoToBookmark(int index)
        {
            if (index >= 0 && index < _bookmarks.Count)
                _currentPage = _bookmarks[index];
        }
        
        // получает информацию о книге
        public string GetInfo() => $"{_bookTitle} - {_currentPage}/{_totalPages}";
        
        // текущая страница
        public int CurrentPage => _currentPage;
        // общее число страниц
        public int TotalPages => _totalPages;
        // открыта ли книга
        public bool IsOpened => !string.IsNullOrEmpty(_currentBookPath);
    }
}