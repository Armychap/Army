using System.Collections.Generic;

namespace BookReaderWithAdapter
{
    // Единый интерфейс для всех книг (Target)
    // Клиент работает только с этим интерфейсом
    interface IBookReader
    {
        void Open(string filePath);
        void Close();
        void NextPage();
        void PreviousPage();
        void GoToPage(int pageNumber);
        void AddBookmark();
        List<int> GetBookmarks();
        void GoToBookmark(int index);
        string GetInfo();
        int CurrentPage { get; }
        int TotalPages { get; }
        bool IsOpened { get; }
    }
}