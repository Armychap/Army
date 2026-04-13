using System;
using System.Text;

namespace ShoppingCartWithUndo
{
    // Класс программы - отвечает за запуск приложения и демонстрацию
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Корзина интернет-магазина с Undo/Redo";

            var logger = new ConsoleLogger();
            var cart = new ShoppingCart();
            var commandManager = new CommandManager(logger);

            // Создаем товары
            var apple = new Product("1", "Яблоко", 1.5m);
            var banana = new Product("2", "Банан", 2.0m);
            var orange = new Product("3", "Апельсин", 3.0m);

            Console.WriteLine("=== ДОБРО ПОЖАЛОВАТЬ В МАГАЗИН ===\n");

            DemonstrateBasicOperations(commandManager, cart, apple, banana, orange);
            DemonstrateUndoRedo(commandManager, cart);
            DemonstrateMacroCommand(commandManager, cart, logger, apple, orange);
            ShowAnalytics(logger);
            DemonstrateChainUndoRedo(commandManager, cart, banana);

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        // Метод для демонстрации основных операций - добавление, изменение, скидки
        private static void DemonstrateBasicOperations(CommandManager commandManager, ShoppingCart cart, Product apple, Product banana, Product orange)
        {
            // 1. Добавляем товары
            commandManager.ExecuteCommand(new AddToCartCommand(cart, apple, 3));
            commandManager.ExecuteCommand(new AddToCartCommand(cart, banana, 2));
            commandManager.ExecuteCommand(new AddToCartCommand(cart, orange, 1));
            cart.Display();

            // 2. Изменяем количество
            commandManager.ExecuteCommand(new ChangeQuantityCommand(cart, "1", 5));
            cart.Display();

            // 3. Применяем скидку
            commandManager.ExecuteCommand(new ApplyDiscountCommand(cart, "1", 0.5m));
            cart.Display();
        }

        // Метод для демонстрации Undo/Redo
        private static void DemonstrateUndoRedo(CommandManager commandManager, ShoppingCart cart)
        {
            // 4. Отменяем скидку
            Console.WriteLine("--- Отмена последнего действия (скидка) ---");
            commandManager.Undo();
            cart.Display();

            // 5. Удаляем товар
            commandManager.ExecuteCommand(new RemoveFromCartCommand(cart, "2"));
            cart.Display();

            // 6. Повторяем удаление (Redo не сработает, т.к. после Execute Redo очистился)
            Console.WriteLine("--- Повтор (но Redo пуст) ---");
            commandManager.Redo();

            // 7. Отменяем удаление
            Console.WriteLine("--- Отмена удаления ---");
            commandManager.Undo();
            cart.Display();
        }

        // Метод для демонстрации макрокоманды
        private static void DemonstrateMacroCommand(CommandManager commandManager, ShoppingCart cart, ILogger logger, Product apple, Product orange)
        {
            // 8. МАКРОКОМАНДА: применяем несколько скидок (промокоды)
            Console.WriteLine("--- Применяем промокоды (макрокоманда) ---");
            var promoCommand = new ApplyPromoCodesCommand(logger);
            promoCommand.AddDiscountCommand(new ApplyDiscountCommand(cart, "1", 0.3m));
            promoCommand.AddDiscountCommand(new ApplyDiscountCommand(cart, "3", 0.5m));
            commandManager.ExecuteCommand(promoCommand);
            cart.Display();

            // 9. Отменяем макрокоманду
            Console.WriteLine("--- Отмена промокодов ---");
            commandManager.Undo();
            cart.Display();
        }

        // Метод для показа аналитики
        private static void ShowAnalytics(ILogger logger)
        {
            // 10. Показываем аналитику
            logger.ShowAnalytics();
        }

        // Метод для демонстрации цепочки Undo/Redo
        private static void DemonstrateChainUndoRedo(CommandManager commandManager, ShoppingCart cart, Product banana)
        {
            // Демонстрация всех возможностей Undo/Redo
            Console.WriteLine("\n=== ДЕМОНСТРАЦИЯ ЦЕПОЧКИ UNDO/REDO ===");
            Console.WriteLine("Добавляем еще один товар и меняем количество...");
            commandManager.ExecuteCommand(new AddToCartCommand(cart, banana, 3));
            commandManager.ExecuteCommand(new ChangeQuantityCommand(cart, "1", 10));
            cart.Display();

            Console.WriteLine("Отмена x2...");
            commandManager.Undo();
            commandManager.Undo();
            cart.Display();

            Console.WriteLine("Повтор x1...");
            commandManager.Redo();
            cart.Display();
        }
    }
}