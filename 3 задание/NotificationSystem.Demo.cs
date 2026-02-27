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
        // Запуск демонстрации
        public void RunDemo()
        {            
            // Проверка Singleton
            NotificationSystem sameNotifications = NotificationSystem.Instance;
            Print($"Один экземпляр? {ReferenceEquals(this, sameNotifications)}");
            Print("");
            
            // Включаем звук
            IsSoundEnabled = true;
            
            // Предзагружаем звуки
            PreloadSounds();
            Print("");
            
            // Главный поток
            Print("Запуск программы");
            SendInformation("Программа запущена");
            SendSuccess("Загрузка завершена");
            
            Print("Нажмите Enter для запуска задач");
            Console.ReadLine();
            
            // Два потока
            Print("");
            Print("Все потоки");
            
            // Поток 1
            Task.Run(() => {
                Print("[Поток 1] Начинаю загрузку");
                
                NotificationSystem.Instance.SendInformation("(1)Файл 1 - загрузка");
                Thread.Sleep(400);
                NotificationSystem.Instance.SendSuccess("(1)Файл 1  - загружен");
                
                NotificationSystem.Instance.SendInformation("(1)Файл 2 - загрузка");
                Thread.Sleep(300);
                NotificationSystem.Instance.SendSuccess("(1)Файл 2 - загружен");
                
                NotificationSystem.Instance.SendWarning("(1)Файл 3 - повреждён, пропускаю");
                
                Print("[Поток 1] Загрузка завершена");
            });
            
            // Поток 2
            Task.Run(() => {
                Thread.Sleep(200);
                Print("[Поток 2] Начинаю обработку");
                
                NotificationSystem.Instance.SendInformation("(2)Обработка - распаковка");
                Thread.Sleep(500);
                NotificationSystem.Instance.SendSuccess("(2)Обработка - распаковано");
                
                NotificationSystem.Instance.SendError("(2)Обработка - ошибка в данных");
                Thread.Sleep(300);
                NotificationSystem.Instance.SendSuccess("(2)Обработка - ошибка исправлена");
                
                NotificationSystem.Instance.SendCritical("(2)Обработка - сбой системы");
                Thread.Sleep(400);
                NotificationSystem.Instance.SendSuccess("(2)Обработка - восстановлено");
                
                Print("[Поток 2] Обработка завершена");
            });
            
            // Главный поток
            Thread.Sleep(300);
            NotificationSystem.Instance.SendInformation("Главный - проверка статуса");
            Thread.Sleep(400);
            NotificationSystem.Instance.SendInformation("Главный - всё работает");
            Thread.Sleep(400);
            NotificationSystem.Instance.SendSuccess("Главный - задача выполнена");
            
            Print("");
            Print("Нажмите Enter для просмотра истории");
            Console.ReadLine();
            
            // Показываем историю
            ShowHistory();
            
            Print("");
            Print("Нажмите Enter для выхода");
            Console.ReadLine();
            
            Shutdown();
        }
    }
}