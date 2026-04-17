using System;
using System.Collections.Generic;

namespace ShoppingCartCommandPattern
{
    // Invoker (принимает команды, выполняет их и управляет отменой/повтором)
    public class CartInvoker
    {
        private Stack<ICommand> undoStack = new Stack<ICommand>(); // для хранения истории выполненных команд
        private Stack<ICommand> redoStack = new Stack<ICommand>(); // для хранения истории отмененных команд
        private List<string> analyticsLog = new List<string>(); // для хранения аналитики

        // Выполняет команду и сохраняет ее в истории Undo
        public void Execute(ICommand command)
        {
            command.Execute(); 
            undoStack.Push(command); // сохраняем команду для возможности отмены
            redoStack.Clear(); // очищаем историю Redo, так как после новой команды нельзя повторить старые отмены
            
            analyticsLog.Add($"{DateTime.Now:HH:mm:ss} - {command.Description}");
            Console.WriteLine($"Лог: {command.Description}\n");
        }

        // Отменяет последнюю выполненную команду
        public void Undo()
        {
            if (undoStack.Count == 0)
            {
                Console.WriteLine("Нечего отменять\n");
                return;
            }
            var cmd = undoStack.Pop(); // получаем последнюю выполненную команду
            cmd.Undo(); // выполняем отмену команды
            redoStack.Push(cmd); // сохраняем отмененную команду для возможности повторения
            analyticsLog.Add($"{DateTime.Now:HH:mm:ss} - Отмена: {cmd.Description}");
            Console.WriteLine($"Отменено: {cmd.Description}\n");
        }

        // Повторяет последнюю отмененную команду, если она доступна в стеке Redo
        public void Redo()
        {
            if (redoStack.Count == 0)
            {
                Console.WriteLine("Нечего повторять\n");
                return;
            }
            var cmd = redoStack.Pop(); // получаем последнюю отмененную команду
            cmd.Execute(); // выполняем повтор команды
            undoStack.Push(cmd); // сохраняем повторенную команду обратно в стек Undo
            analyticsLog.Add($"{DateTime.Now:HH:mm:ss} - Повтор: {cmd.Description}");
            Console.WriteLine($"Повторено: {cmd.Description}\n");
        }

        public void ShowAnalytics()
        {
            Console.WriteLine("\nАналитика:");
            foreach (var log in analyticsLog)
                Console.WriteLine($"  {log}");
            Console.WriteLine();
        }
    }
}