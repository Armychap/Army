using System;

namespace BookReaderWithAdapter
{
    class Program
    {
        static void Main(string[] args)
        {
            // Создаем тестовый файл
            string testFile = "book.epub"; // имя тестового файла
            if (!File.Exists(testFile))
            {
                File.WriteAllText(testFile, new string('A', 102400));
                Console.WriteLine("Создана тестовая книга\n");
            }
            
            Console.WriteLine("Работа с EPUB книгой через адаптер");
            
            // Адаптер позволяет использовать старый класс через новый интерфейс
            OldEpubReader legacyReader = new OldEpubReader(); // старый читатель EPUB
            IBookReader epubReader = new EpubReaderAdapter(legacyReader); // адаптер для EPUB
            
            // Клиент использует универсальные методы Open(), Close()
            Console.WriteLine("Открываем EPUB книгу");
            epubReader.Open(testFile); // открываем книгу
            Console.WriteLine($"   {epubReader.GetInfo()}\n");
            
            // Единый интерфейс для всех операций
            Console.WriteLine("Переходим на страницу 5");
            epubReader.GoToPage(5); // переходим на страницу
            epubReader.AddBookmark(); // добавляем закладку
            Console.WriteLine($"Закладка добавлена на странице 5\n");
            
            Console.WriteLine("Листаем вперед");
            epubReader.NextPage(); // следующая страница
            Console.WriteLine($"Текущая страница: {epubReader.CurrentPage}");
            epubReader.NextPage(); // следующая страница
            Console.WriteLine($"Текущая страница: {epubReader.CurrentPage}\n");
            
            Console.WriteLine($"Информация: {epubReader.GetInfo()}");
            Console.WriteLine($"Закладки: {string.Join(", ", epubReader.GetBookmarks())}\n");
            
            epubReader.Close(); // закрываем книгу
            
            // Работа с PDF
            Console.WriteLine("Работа с PDF книгой");
            
            // Новый формат легко добавляется через реализацию того же интерфейса
            IBookReader pdfReader = new PdfReader(); // новый читатель PDF
            
            pdfReader.Open(testFile); // открываем PDF
            Console.WriteLine($"   {pdfReader.GetInfo()}\n");
            
            Console.WriteLine("Переходим на страницу 10");
            pdfReader.GoToPage(10); // переходим на страницу
            pdfReader.AddBookmark(); // добавляем закладку
            
            Console.WriteLine("Листаем вперед");
            pdfReader.NextPage(); // следующая страница
            Console.WriteLine($"   Текущая страница: {pdfReader.CurrentPage}\n");
            
            Console.WriteLine($"Информация: {pdfReader.GetInfo()}");
            Console.WriteLine($"Закладки: {string.Join(", ", pdfReader.GetBookmarks())}\n");
            
            pdfReader.Close(); // закрываем PDF
        }
    }
}