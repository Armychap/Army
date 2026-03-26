using System.Collections.Generic;

namespace BookReaderWithAdapter
{
    // Адаптер, преобразующий OldEpubReader к интерфейсу IBookReader
    // Решает проблему несовместимости интерфейсов
    class EpubReaderAdapter : IBookReader
    {
        // ссылка на адаптируемый объект
        private OldEpubReader _epubReader;
        
        // конструктор адаптера
        public EpubReaderAdapter(OldEpubReader epubReader)
        {
            _epubReader = epubReader;
        }
        
        // Open() → OpenEpub()
        public void Open(string filePath) => _epubReader.OpenEpub(filePath);
        
        // Close() → CloseEpub()
        public void Close() => _epubReader.CloseEpub();
        
        public void NextPage() => _epubReader.NextPage();
        public void PreviousPage() => _epubReader.PreviousPage();
        
        // GoToPage() → DisplayPage()
        public void GoToPage(int pageNumber) => _epubReader.DisplayPage(pageNumber);
        
        public void AddBookmark() => _epubReader.AddBookmark();
        public List<int> GetBookmarks() => _epubReader.GetBookmarks();
        public void GoToBookmark(int index) => _epubReader.GoToBookmark(index);
        
        // GetInfo() → GetBookInfo()
        public string GetInfo() => _epubReader.GetBookInfo();
        
        public int CurrentPage => _epubReader.CurrentPage;
        public int TotalPages => _epubReader.TotalPages;
        public bool IsOpened => _epubReader.IsBookOpened;
    }
}