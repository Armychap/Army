using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagement.Simple
{
    // Класс книги
    public class Book
    {
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public uint Year { get; set; }
        public string Publisher { get; set; }
        public uint Pages { get; set; }

        public Book(string title, List<string> authors, uint year, string publisher, uint pages)
        {
            Title = title;
            Authors = authors;
            Year = year;
            Publisher = publisher;
            Pages = pages;
        }

        public void PrintInfo()
        {
            Console.WriteLine($"Название: {Title}");
            Console.Write("Автор(ы): ");
            foreach (var author in Authors)
            {
                Console.Write(author + " ");
            }
            Console.WriteLine();
            Console.WriteLine($"Год издания: {Year}");
            Console.WriteLine($"Издательство: {Publisher}");
            Console.WriteLine($"Количество страниц: {Pages}");
        }
    }

    // Запись о выдаче
    public class Picked
    {
        public string ReaderName { get; set; }
        public DateTime IssueDate { get; set; }
    }

    // Книга в каталоге
    public class CatalogBook : Book
    {
        public uint Id { get; set; }
        public uint Quantity { get; set; }
        public uint Instances { get; set; }
        public List<Picked> PickedRecords { get; set; }

        public CatalogBook(uint id, string title, List<string> authors, uint year, 
                          string publisher, uint pages, uint quantity)
            : base(title, authors, year, publisher, pages)
        {
            Id = id;
            Quantity = quantity;
            Instances = quantity;
            PickedRecords = new List<Picked>();
        }

        public void PrintCatalogInfo()
        {
            PrintInfo();
            Console.WriteLine($"ID: {Id}");
            Console.WriteLine($"Количество экземпляров: {Quantity}. В наличии: {Instances}");

            if (PickedRecords.Any())
            {
                Console.WriteLine("Читатели, взявшие книгу:");
                foreach (var record in PickedRecords)
                {
                    Console.WriteLine($"{record.ReaderName}. Дата выдачи: {record.IssueDate:dd.MM.yyyy}");
                }
            }
            else
            {
                Console.WriteLine("Никто не взял эту книгу.");
            }
        }

        public void AddPicked(string readerName)
        {
            if (Instances > 0)
            {
                PickedRecords.Add(new Picked { ReaderName = readerName, IssueDate = DateTime.Now });
                Instances--;
                Console.WriteLine($"Книга выдана читателю: {readerName}");
            }
            else
            {
                Console.WriteLine("Нет доступных экземпляров!");
            }
        }

        public void ReturnBook(string readerName)
        {
            var record = PickedRecords.FirstOrDefault(p => p.ReaderName == readerName);
            if (record != null)
            {
                PickedRecords.Remove(record);
                Instances++;
                Console.WriteLine($"Книга возвращена читателем: {readerName}");
            }
            else
            {
                Console.WriteLine($"Читатель {readerName} не числится среди взявших эту книгу.");
            }
        }
    }

    class Program
    {
        static CatalogBook SearchBook(List<CatalogBook> catalog, string query, bool searchByTitle)
        {
            foreach (var book in catalog)
            {
                if (searchByTitle && book.Title.Equals(query, StringComparison.OrdinalIgnoreCase))
                {
                    return book;
                }
                if (!searchByTitle)
                {
                    foreach (var author in book.Authors)
                    {
                        if (author.Equals(query, StringComparison.OrdinalIgnoreCase))
                        {
                            return book;
                        }
                    }
                }
            }
            return null;
        }

        static void PrintOverdueReaders(List<CatalogBook> catalog)
        {
            var currentTime = DateTime.Now;
            bool found = false;
            
            foreach (var book in catalog)
            {
                foreach (var record in book.PickedRecords)
                {
                    var days = (currentTime - record.IssueDate).Days;
                    if (days > 365)
                    {
                        Console.WriteLine($"Читатель {record.ReaderName} не вернул книгу \"{book.Title}\" за {days} дней.");
                        found = true;
                    }
                }
            }
            
            if (!found)
            {
                Console.WriteLine("Нет читателей с просроченными книгами.");
            }
        }

        static void Menu()
        {
            Console.WriteLine("\n=== БИБЛИОТЕЧНЫЙ КАТАЛОГ ===");
            Console.WriteLine("1. Создать книгу");
            Console.WriteLine("2. Удалить книгу");
            Console.WriteLine("3. Вывод информации о книге (по ID)");
            Console.WriteLine("4. Поиск книги (по названию или автору)");
            Console.WriteLine("5. Выдача книги читателю");
            Console.WriteLine("6. Возврат книги");
            Console.WriteLine("7. Список читателей, не вернувших книги в течение года");
            Console.WriteLine("8. Выход");
            Console.Write("Выберите действие: ");
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            var catalog = new List<CatalogBook>();
            uint nextId = 1;

            while (true)
            {
                Menu();
                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Неверный выбор.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        {
                            Console.Write("Введите название книги: ");
                            string title = Console.ReadLine();

                            Console.Write("Введите количество авторов: ");
                            if (!int.TryParse(Console.ReadLine(), out int authorCount))
                            {
                                Console.WriteLine("Неверный формат.");
                                break;
                            }
                            
                            var authors = new List<string>();
                            for (int i = 0; i < authorCount; i++)
                            {
                                Console.Write($"Введите имя автора {i + 1}: ");
                                authors.Add(Console.ReadLine());
                            }

                            Console.Write("Введите год издания: ");
                            if (!uint.TryParse(Console.ReadLine(), out uint year))
                            {
                                Console.WriteLine("Неверный формат.");
                                break;
                            }

                            Console.Write("Введите издательство: ");
                            string publisher = Console.ReadLine();

                            Console.Write("Введите количество страниц: ");
                            if (!uint.TryParse(Console.ReadLine(), out uint pages))
                            {
                                Console.WriteLine("Неверный формат.");
                                break;
                            }

                            Console.Write("Введите количество экземпляров: ");
                            if (!uint.TryParse(Console.ReadLine(), out uint quantity))
                            {
                                Console.WriteLine("Неверный формат.");
                                break;
                            }

                            catalog.Add(new CatalogBook(nextId++, title, authors, year, publisher, pages, quantity));
                            Console.WriteLine("Книга добавлена в каталог.");
                            break;
                        }

                    case 2:
                        {
                            Console.Write("Введите ID книги для удаления: ");
                            if (!uint.TryParse(Console.ReadLine(), out uint id))
                            {
                                Console.WriteLine("Неверный формат.");
                                break;
                            }

                            var book = catalog.FirstOrDefault(b => b.Id == id);
                            if (book != null)
                            {
                                catalog.Remove(book);
                                Console.WriteLine("Книга удалена из каталога.");
                            }
                            else
                            {
                                Console.WriteLine("Книга с таким ID не найдена.");
                            }
                            break;
                        }

                    case 3:
                        {
                            Console.Write("Введите ID книги: ");
                            if (!uint.TryParse(Console.ReadLine(), out uint id))
                            {
                                Console.WriteLine("Неверный формат.");
                                break;
                            }

                            var book = catalog.FirstOrDefault(b => b.Id == id);
                            if (book != null)
                            {
                                book.PrintCatalogInfo();
                            }
                            else
                            {
                                Console.WriteLine("Книга с таким ID не найдена.");
                            }
                            break;
                        }

                    case 4:
                        {
                            Console.Write("Поиск по названию (1) или автору (2): ");
                            if (!int.TryParse(Console.ReadLine(), out int searchType))
                            {
                                Console.WriteLine("Неверный формат.");
                                break;
                            }
                            
                            Console.Write("Введите запрос: ");
                            string query = Console.ReadLine();

                            var book = SearchBook(catalog, query, searchType == 1);
                            if (book != null)
                            {
                                Console.WriteLine($"Найдена книга: \"{book.Title}\", ID: {book.Id}");
                            }
                            else
                            {
                                Console.WriteLine("Книга не найдена.");
                            }
                            break;
                        }

                    case 5:
                        {
                            Console.Write("Введите ID книги: ");
                            if (!uint.TryParse(Console.ReadLine(), out uint id))
                            {
                                Console.WriteLine("Неверный формат.");
                                break;
                            }
                            
                            Console.Write("Введите имя читателя: ");
                            string readerName = Console.ReadLine();

                            var book = catalog.FirstOrDefault(b => b.Id == id);
                            if (book != null)
                            {
                                book.AddPicked(readerName);
                            }
                            else
                            {
                                Console.WriteLine("Книга с таким ID не найдена.");
                            }
                            break;
                        }

                    case 6:
                        {
                            Console.Write("Введите ID книги: ");
                            if (!uint.TryParse(Console.ReadLine(), out uint id))
                            {
                                Console.WriteLine("Неверный формат.");
                                break;
                            }
                            
                            Console.Write("Введите имя читателя: ");
                            string readerName = Console.ReadLine();

                            var book = catalog.FirstOrDefault(b => b.Id == id);
                            if (book != null)
                            {
                                book.ReturnBook(readerName);
                            }
                            else
                            {
                                Console.WriteLine("Книга с таким ID не найдена.");
                            }
                            break;
                        }

                    case 7:
                        {
                            PrintOverdueReaders(catalog);
                            break;
                        }

                    case 8:
                        {
                            Console.WriteLine("До свидания!");
                            return;
                        }

                    default:
                        Console.WriteLine("Неверный выбор.");
                        break;
                }
            }
        }
    }
}