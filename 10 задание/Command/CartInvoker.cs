using System;
using System.Collections.Generic;

namespace ShoppingCartCommandPattern
{
    // Invoker (принимает команды, выполняет их и управляет отменой/повтором)
    public class CartInvoker
    {
        private Stack<ICommand> undoStack = new Stack<ICommand>(); // стек для хранения истории выполненных команд (для Undo)
        private Stack<ICommand> redoStack = new Stack<ICommand>(); // стек для хранения истории отмененных команд (для Redo)
        private List<string> analyticsLog = new List<string>(); // список для хранения лога всех действий с временными метками

        // Выполняет команду и сохраняет ее в истории Undo
        public void Execute(ICommand command)
        {
            command.Execute(); // выполняем саму команду
            undoStack.Push(command); // сохраняем команду в стек Undo для возможности отмены
            redoStack.Clear(); // очищаем историю Redo, так как после новой команды нельзя повторить старые отмены
            
            // Добавляем запись в аналитический лог
            analyticsLog.Add($"{DateTime.Now:HH:mm:ss} - {command.Description}");
            Console.WriteLine($"Лог: {command.Description}\n");
        }

        // Отменяет последнюю выполненную команду
        public void Undo()
        {
            // Проверяем, есть ли команды для отмены
            if (undoStack.Count == 0)
            {
                Console.WriteLine("Нечего отменять\n");
                return;
            }
            
            var cmd = undoStack.Pop(); // получаем последнюю выполненную команду (извлекаем из стека)
            cmd.Undo(); // вызываем отмену команды
            redoStack.Push(cmd); // сохраняем отмененную команду в стек Redo для возможности повтора
            
            // Добавляем запись об отмене в аналитический лог
            analyticsLog.Add($"{DateTime.Now:HH:mm:ss} - Отмена: {cmd.Description}");
            Console.WriteLine($"Отменено: {cmd.Description}\n");
        }

        // Повторяет последнюю отмененную команду, если она доступна в стеке Redo
        public void Redo()
        {
            // Проверяем, есть ли отмененные команды для повтора
            if (redoStack.Count == 0)
            {
                Console.WriteLine("Нечего повторять\n");
                return;
            }
            
            var cmd = redoStack.Pop(); // получаем последнюю отмененную команду (извлекаем из стека Redo)
            cmd.Execute(); // выполняем команду заново
            undoStack.Push(cmd); // сохраняем повторенную команду обратно в стек Undo (чтобы можно было отменить повтор)
            
            // Добавляем запись о повторе в аналитический лог
            analyticsLog.Add($"{DateTime.Now:HH:mm:ss} - Повтор: {cmd.Description}");
            Console.WriteLine($"Повторено: {cmd.Description}\n");
        }

        // Выводит всю аналитику - историю всех выполненных, отмененных и повторенных действий
        public void ShowAnalytics()
        {
            Console.WriteLine("\nАналитика:");
            foreach (var log in analyticsLog)
                Console.WriteLine($"  {log}");
            Console.WriteLine();
        }
    }
}