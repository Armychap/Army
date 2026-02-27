using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationSingletonDemo
{
    public sealed partial class NotificationSystem
    {
        // Показать историю
        public void ShowHistory()
        {
            Console.WriteLine("\nИстория оповещений");

            string[] history;
            lock (_syncRoot)
            {
                history = _notificationHistory.ToArray();
            }

            foreach (string entry in history)
                Console.WriteLine(entry);
        }

        // Вывести текст
        public void Print(string text) => Console.WriteLine(text);
    }
}