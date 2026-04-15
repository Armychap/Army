using System;
using System.Collections.Generic;

namespace ShoppingCartCommandPattern
{
    // Invoker паттерна Command: принимает команды, выполняет их и управляет отменой/повтором.
    // Здесь также сохраняется история действий для простого журнала аналитики.
    public class CartInvoker
    {
        private Stack<ICommand> undoStack = new Stack<ICommand>();
        private Stack<ICommand> redoStack = new Stack<ICommand>();
        private List<string> analyticsLog = new List<string>();

        // Выполняет команду и сохраняет ее в истории Undo.
        // После нового действия стек Redo очищается, поскольку прежние повторы становятся неактуальными.
        public void Execute(ICommand command)
        {
            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();
            
            analyticsLog.Add($"{DateTime.Now:HH:mm:ss} - {command.Description}");
            Console.WriteLine($"Лог: {command.Description}\n");
        }

        // Отменяет последнюю выполненную команду.
        // Команда перемещается в стек Redo, чтобы ее можно было повторить позже.
        public void Undo()
        {
            if (undoStack.Count == 0)
            {
                Console.WriteLine("Нечего отменять\n");
                return;
            }
            var cmd = undoStack.Pop();
            cmd.Undo();
            redoStack.Push(cmd);
            analyticsLog.Add($"{DateTime.Now:HH:mm:ss} - Отмена: {cmd.Description}");
            Console.WriteLine($"Отменено: {cmd.Description}\n");
        }

        // Повторяет последнюю отмененную команду, если она доступна в стеке Redo.
        public void Redo()
        {
            if (redoStack.Count == 0)
            {
                Console.WriteLine("Нечего повторять\n");
                return;
            }
            var cmd = redoStack.Pop();
            cmd.Execute();
            undoStack.Push(cmd);
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