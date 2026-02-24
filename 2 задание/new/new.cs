using System;
using System.Collections.Generic;
using System.Linq;

//SRP (Single Responsibility): Принцип единственной ответственности — один класс должен решать одну задачу.
//LSP (Liskov Substitution): Принцип подстановки Барбары Лисков — подклассы должны заменять базовые классы без нарушения работы.
//DIP (Dependency Inversion): Принцип инверсии зависимостей — зависимости строятся на абстракциях, а не на реализациях.

namespace LibraryManagement.SOLID
{
    // SRP
    public class Book
    {
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public uint Year { get; set; }
        public string Publisher { get; set; }
        public uint Pages { get; set; }

        //lsp

        public Book(string title, List<string> authors, uint year, string publisher, uint pages)
        {
            Title = title;
            Authors = authors;
            Year = year;
            Publisher = publisher;
            Pages = pages;
        }
    }

    // SRP
    //lsp
    public class CatalogBook : Book
    {
        public uint Id { get; set; }
        public uint Quantity { get; set; }
        public uint Instances { get; set; }

        public CatalogBook(uint id, string title, List<string> authors, uint year,
                          string publisher, uint pages, uint quantity)
            : base(title, authors, year, publisher, pages)
        {
            Id = id;
            Quantity = quantity;
            Instances = quantity;
        }
    }

    // SRP
    public class PickedRecord
    {
        public uint BookId { get; set; }
        public string ReaderName { get; set; }
        public DateTime IssueDate { get; set; }
    }

    public interface IBookRepository
    {
        void Add(CatalogBook book);
        bool Delete(uint id);
        CatalogBook GetById(uint id);
        CatalogBook GetByTitle(string title);
        List<CatalogBook> GetByAuthor(string author);
        List<CatalogBook> GetAll();
        void Update(CatalogBook book);
    }

    public interface IPickedRepository
    {
        void Add(PickedRecord record);
        bool Remove(uint bookId, string readerName);
        List<PickedRecord> GetByBookId(uint bookId);
        List<PickedRecord> GetAll();
    }

    // DIP, LSP
    public interface IOutputService
    {
        void PrintBookInfo(CatalogBook book, List<PickedRecord> pickedRecords);
        void PrintMessage(string message);
        void PrintError(string message);
        void PrintSuccess(string message);
        void PrintOverdueReaders(List<(string ReaderName, string BookTitle, int DaysOverdue)> overdueList);
        void PrintMenu();
    }

    public interface IInputService
    {
        int GetIntInput(string prompt);
        uint GetUIntInput(string prompt);
        string GetStringInput(string prompt);
        int GetMenuChoice();
    }

    public interface ISearchService
    {
        CatalogBook SearchByTitle(string title);
        List<CatalogBook> SearchByAuthor(string author);
    }

    public interface IOverdueService
    {
        List<(string ReaderName, string BookTitle, int DaysOverdue)> GetOverdueReaders();
    }


    // SRP, DIP

    public class BookRepository : IBookRepository
    {
        private List<CatalogBook> _books = new List<CatalogBook>();

        public void Add(CatalogBook book) => _books.Add(book);

        public bool Delete(uint id)
        {
            var book = GetById(id);
            return book != null && _books.Remove(book);
        }

        public CatalogBook GetById(uint id)
            => _books.FirstOrDefault(b => b.Id == id);

        public CatalogBook GetByTitle(string title)
            => _books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

        public List<CatalogBook> GetByAuthor(string author)
            => _books.Where(b => b.Authors.Any(a => a.Equals(author, StringComparison.OrdinalIgnoreCase))).ToList();

        public List<CatalogBook> GetAll() => _books.ToList();

        public void Update(CatalogBook book)
        {
            var index = _books.FindIndex(b => b.Id == book.Id);
            if (index >= 0)
                _books[index] = book;
        }
    }

    public class PickedRepository : IPickedRepository
    {
        private List<PickedRecord> _records = new List<PickedRecord>();

        public void Add(PickedRecord record) => _records.Add(record);

        public bool Remove(uint bookId, string readerName)
        {
            var record = _records.FirstOrDefault(r => r.BookId == bookId && r.ReaderName == readerName);
            return record != null && _records.Remove(record);
        }

        public List<PickedRecord> GetByBookId(uint bookId)
            => _records.Where(r => r.BookId == bookId).ToList();

        public List<PickedRecord> GetAll() => _records.ToList();
    }

    public class ConsoleOutputService : IOutputService
    {
        public void PrintBookInfo(CatalogBook book, List<PickedRecord> pickedRecords)
        {
            Console.WriteLine($"КНИГА ID: {book.Id}");
            Console.WriteLine($"Название: {book.Title}");
            Console.Write("Автор(ы): ");
            foreach (var author in book.Authors)
                Console.Write(author + " ");
            Console.WriteLine();
            Console.WriteLine($"Год издания: {book.Year}");
            Console.WriteLine($"Издательство: {book.Publisher}");
            Console.WriteLine($"Страниц: {book.Pages}");
            Console.WriteLine($"Экземпляров: {book.Quantity}, В наличии: {book.Instances}");

            if (pickedRecords.Any())
            {
                Console.WriteLine("Читатели, взявшие книгу:");
                foreach (var record in pickedRecords)
                {
                    Console.WriteLine($"  {record.ReaderName} (выдана: {record.IssueDate:dd.MM.yyyy})");
                }
            }
            else
            {
                Console.WriteLine("Никто не брал эту книгу.");
            }
        }

        public void PrintMessage(string message) => Console.WriteLine(message);

        public void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ОШИБКА: {message}");
            Console.ResetColor();
        }

        public void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void PrintOverdueReaders(List<(string ReaderName, string BookTitle, int DaysOverdue)> overdueList)
        {
            if (!overdueList.Any())
            {
                PrintMessage("Нет читателей с просроченными книгами.");
                return;
            }
            Console.WriteLine("ЧИТАТЕЛИ С ПРОСРОЧЕННЫМИ КНИГАМИ");
            foreach (var item in overdueList)
            {
                PrintError($"{item.ReaderName} не вернул книгу \"{item.BookTitle}\" ({item.DaysOverdue} дней)");
            }
        }

        public void PrintMenu()
        {
            Console.WriteLine("БИБЛИОТЕЧНЫЙ КАТАЛОГ");
            Console.WriteLine("1. Создать книгу");
            Console.WriteLine("2. Удалить книгу");
            Console.WriteLine("3. Информация о книге (по ID)");
            Console.WriteLine("4. Поиск книги");
            Console.WriteLine("5. Выдать книгу читателю");
            Console.WriteLine("6. Вернуть книгу");
            Console.WriteLine("7. Просроченные книги");
            Console.WriteLine("8. Выход");
            Console.Write("Выберите действие: ");
        }
    }

    public class ConsoleInputService : IInputService
    {
        public int GetIntInput(string prompt)
        {
            Console.Write(prompt);
            int result; 
            while (!int.TryParse(Console.ReadLine(), out result))
            {
                Console.Write("Неверный формат. Введите целое число: ");
            }
            return result;  
        }

        public uint GetUIntInput(string prompt)
        {
            Console.Write(prompt);
            uint result;
            while (!uint.TryParse(Console.ReadLine(), out result))
            {
                Console.Write("Неверный формат. Введите положительное число: ");
            }
            return result;
        }

        public string GetStringInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        public int GetMenuChoice()
        {
            return int.TryParse(Console.ReadLine(), out int choice) ? choice : -1;
        }
    }

//lsp
    public class SearchService : ISearchService
    {
        private readonly IBookRepository _bookRepository;

        public SearchService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public CatalogBook SearchByTitle(string title)
            => _bookRepository.GetByTitle(title);

        public List<CatalogBook> SearchByAuthor(string author)
            => _bookRepository.GetByAuthor(author);
    }

//lsp
    public class OverdueService : IOverdueService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IPickedRepository _pickedRepository;

        public OverdueService(IBookRepository bookRepository, IPickedRepository pickedRepository)
        {
            _bookRepository = bookRepository;
            _pickedRepository = pickedRepository;
        }

        public List<(string ReaderName, string BookTitle, int DaysOverdue)> GetOverdueReaders()
        {
            var result = new List<(string, string, int)>();
            var currentDate = DateTime.Now;

            foreach (var record in _pickedRepository.GetAll())
            {
                var daysOverdue = (currentDate - record.IssueDate).Days;
                if (daysOverdue > 365)
                {
                    var book = _bookRepository.GetById(record.BookId);
                    if (book != null)
                    {
                        result.Add((record.ReaderName, book.Title, daysOverdue));
                    }
                }
            }
            return result;
        }
    }


    // LSP
    public class FileOutputService : IOutputService
    {
        private readonly string _filePath;

        public FileOutputService(string filePath = "library_log.txt")
        {
            _filePath = filePath;
        }

        private void WriteToFile(string content)
        {
            using (var writer = new System.IO.StreamWriter(_filePath, true, System.Text.Encoding.UTF8))
            {
                writer.WriteLine(content);
            }
        }

        public void PrintBookInfo(CatalogBook book, List<PickedRecord> pickedRecords)
        {
            WriteToFile($"КНИГА ID: {book.Id}");
            WriteToFile($"Название: {book.Title}");
            WriteToFile($"Автор(ы): {string.Join(", ", book.Authors)}");
            WriteToFile($"Год издания: {book.Year}");
            WriteToFile($"Издательство: {book.Publisher}");
            WriteToFile($"Страниц: {book.Pages}");
            WriteToFile($"Экземпляров: {book.Quantity}, В наличии: {book.Instances}");

            if (pickedRecords.Any())
            {
                WriteToFile("Читатели, взявшие книгу:");
                foreach (var record in pickedRecords)
                {
                    WriteToFile($"  {record.ReaderName} (выдана: {record.IssueDate:dd.MM.yyyy})");
                }
            }
            else
            {
                WriteToFile("Никто не брал эту книгу.");
            }
        }

        public void PrintMessage(string message) => WriteToFile(message);

        public void PrintError(string message) => WriteToFile($"ERROR: {message}");

        public void PrintSuccess(string message) => WriteToFile($"SUCCESS: {message}");

        // LSP
        public void PrintOverdueReaders(List<(string ReaderName, string BookTitle, int DaysOverdue)> overdueList)
        {
            if (!overdueList.Any())
            {
                WriteToFile("Нет читателей с просроченными книгами.");
                return;
            }
            WriteToFile("ЧИТАТЕЛИ С ПРОСРОЧЕННЫМИ КНИГАМИ");
            foreach (var item in overdueList)
            {
                WriteToFile($"ERROR: {item.ReaderName} не вернул книгу \"{item.BookTitle}\" ({item.DaysOverdue} дней)");
            }
        }

        // LSP
        public void PrintMenu()
        {
            WriteToFile("БИБЛИОТЕЧНЫЙ КАТАЛОГ");
            WriteToFile("1. Создать книгу");
            WriteToFile("2. Удалить книгу");
            WriteToFile("3. Информация о книге (по ID)");
            WriteToFile("4. Поиск книги");
            WriteToFile("5. Выдать книгу читателю");
            WriteToFile("6. Вернуть книгу");
            WriteToFile("7. Просроченные книги");
            WriteToFile("8. Выход");
        }
    }


    //SRP, DIP
    //lsp
    public class LibraryService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IPickedRepository _pickedRepository;
        private readonly IOutputService _output;
        private readonly IInputService _input;
        private readonly ISearchService _search;
        private readonly IOverdueService _overdue;
        private uint _nextId = 1;

        public LibraryService(
            IBookRepository bookRepository,
            IPickedRepository pickedRepository,
            IOutputService output,
            IInputService input,
            ISearchService search,
            IOverdueService overdue)
        {
            _bookRepository = bookRepository;
            _pickedRepository = pickedRepository;
            _output = output;
            _input = input;
            _search = search;
            _overdue = overdue;
        }

        public void CreateBook()
        {
            _output.PrintMessage("СОЗДАНИЕ НОВОЙ КНИГИ");

            string title = _input.GetStringInput("Введите название книги: ");

            int authorCount = _input.GetIntInput("Введите количество авторов: ");
            var authors = new List<string>();
            for (int i = 0; i < authorCount; i++)
            {
                authors.Add(_input.GetStringInput($"Введите имя автора {i + 1}: "));
            }

            uint year = _input.GetUIntInput("Введите год издания: ");
            string publisher = _input.GetStringInput("Введите издательство: ");
            uint pages = _input.GetUIntInput("Введите количество страниц: ");
            uint quantity = _input.GetUIntInput("Введите количество экземпляров: ");

            var book = new CatalogBook(_nextId++, title, authors, year, publisher, pages, quantity);
            _bookRepository.Add(book);

            _output.PrintSuccess($"Книга добавлена в каталог с ID: {book.Id}");
        }

        public void DeleteBook()
        {
            uint id = _input.GetUIntInput("Введите ID книги для удаления: ");

            if (_bookRepository.Delete(id))
            {
                _output.PrintSuccess("Книга удалена из каталога.");
            }
            else
            {
                _output.PrintError("Книга с таким ID не найдена.");
            }
        }

        public void ShowBookInfo()
        {
            uint id = _input.GetUIntInput("Введите ID книги: ");

            var book = _bookRepository.GetById(id);
            if (book == null)
            {
                _output.PrintError("Книга с таким ID не найдена.");
                return;
            }

            var pickedRecords = _pickedRepository.GetByBookId(id);
            _output.PrintBookInfo(book, pickedRecords);
        }

        public void SearchBook()
        {
            _output.PrintMessage("ПОИСК КНИГИ");
            _output.PrintMessage("1 - по названию");
            _output.PrintMessage("2 - по автору");

            int type = _input.GetIntInput("Выберите тип поиска: ");
            string query = _input.GetStringInput("Введите запрос: ");

            if (type == 1)
            {
                var book = _search.SearchByTitle(query);
                if (book != null)
                {
                    var pickedRecords = _pickedRepository.GetByBookId(book.Id);
                    _output.PrintBookInfo(book, pickedRecords);
                }
                else
                {
                    _output.PrintError("Книга не найдена.");
                }
            }
            else if (type == 2)
            {
                var books = _search.SearchByAuthor(query);
                if (books.Any())
                {
                    _output.PrintMessage($"Найдено книг: {books.Count}");
                    foreach (var book in books)
                    {
                        var pickedRecords = _pickedRepository.GetByBookId(book.Id);
                        _output.PrintBookInfo(book, pickedRecords);
                    }
                }
                else
                {
                    _output.PrintError("Книги не найдены.");
                }
            }
            else
            {
                _output.PrintError("Неверный тип поиска.");
            }
        }

        public void IssueBook()
        {
            uint id = _input.GetUIntInput("Введите ID книги: ");
            string readerName = _input.GetStringInput("Введите имя читателя: ");

            var book = _bookRepository.GetById(id);
            if (book == null)
            {
                _output.PrintError("Книга с таким ID не найдена.");
                return;
            }

            if (book.Instances == 0)
            {
                _output.PrintError("Нет доступных экземпляров!");
                return;
            }

            book.Instances--;
            _bookRepository.Update(book);

            _pickedRepository.Add(new PickedRecord
            {
                BookId = id,
                ReaderName = readerName,
                IssueDate = DateTime.Now
            });

            _output.PrintSuccess($"Книга выдана читателю: {readerName}");
        }

        public void ReturnBook()
        {
            uint id = _input.GetUIntInput("Введите ID книги: ");
            string readerName = _input.GetStringInput("Введите имя читателя: ");

            var book = _bookRepository.GetById(id);
            if (book == null)
            {
                _output.PrintError("Книга с таким ID не найдена.");
                return;
            }

            if (_pickedRepository.Remove(id, readerName))
            {
                book.Instances++;
                _bookRepository.Update(book);
                _output.PrintSuccess($"Книга возвращена читателем: {readerName}");
            }
            else
            {
                _output.PrintError($"Читатель {readerName} не числится среди взявших эту книгу.");
            }
        }

        public void ShowOverdueReaders()
        {
            var overdueList = _overdue.GetOverdueReaders();
            _output.PrintOverdueReaders(overdueList);
        }

        public void Run()
        {
            while (true)
            {
                _output.PrintMenu();
                int choice = _input.GetMenuChoice();

                switch (choice)
                {
                    case 1: CreateBook(); break;
                    case 2: DeleteBook(); break;
                    case 3: ShowBookInfo(); break;
                    case 4: SearchBook(); break;
                    case 5: IssueBook(); break;
                    case 6: ReturnBook(); break;
                    case 7: ShowOverdueReaders(); break;
                    case 8:
                        _output.PrintSuccess("До свидания!");
                        return;
                    default:
                        _output.PrintError("Неверный выбор.");
                        break;
                }
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            // DIP: создаём конкретные реализации
            var bookRepository = new BookRepository();
            var pickedRepository = new PickedRepository();

            // LSP
            var outputService = new ConsoleOutputService();

            var inputService = new ConsoleInputService();
            var searchService = new SearchService(bookRepository);
            var overdueService = new OverdueService(bookRepository, pickedRepository);

            var library = new LibraryService(
                bookRepository,
                pickedRepository,
                outputService,
                inputService,
                searchService,
                overdueService
            );

            library.Run();
        }
    }
}