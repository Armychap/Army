using System;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationSingletonDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            NotificationSystem notifications = NotificationSystem.Instance;
            
            notifications.Print("СИСТЕМА ОПОВЕЩЕНИЙ");
            notifications.Print("");
            
            // Проверка Singleton
            NotificationSystem sameNotifications = NotificationSystem.Instance;
            notifications.Print($"Один экземпляр? {ReferenceEquals(notifications, sameNotifications)}");
            notifications.Print("");
            
            // Включаем звук
            notifications.IsSoundEnabled = true;
            
            // Предзагружаем звуки
            notifications.PreloadSounds();
            notifications.Print("");
            
            // ===== ОТПРАВКА ИЗ ГЛАВНОГО ПОТОКА =====
            notifications.Print("Запуск программы");
            notifications.SendInformation("Программа запущена");
            notifications.SendSuccess("Загрузка завершена");
            
            notifications.Print("Нажмите Enter для запуска задач");
            Console.ReadLine();
            
            // ===== РАБОТА В ДВУХ ПОТОКАХ =====
            notifications.Print("");
            notifications.Print("Все потоки");
            
            // ПОТОК 1: Загрузка данных
            Task.Run(() => {
                notifications.Print("[Поток 1] Начинаю загрузку");
                
                NotificationSystem.Instance.SendInformation("Файл 1 - загрузка");
                Thread.Sleep(400);
                NotificationSystem.Instance.SendSuccess("Файл 1  - загружен");
                
                NotificationSystem.Instance.SendInformation("Файл 2 - загрузка");
                Thread.Sleep(300);
                NotificationSystem.Instance.SendSuccess("Файл 2 - загружен");
                
                // Предупреждение
                NotificationSystem.Instance.SendWarning("Файл 3 - повреждён, пропускаю");
                
                notifications.Print("[Поток 1] Загрузка завершена");
            });
            
            // ПОТОК 2: Обработка данных
            Task.Run(() => {
                Thread.Sleep(200);
                notifications.Print("[Поток 2] Начинаю обработку");
                
                NotificationSystem.Instance.SendInformation("Обработка - распаковка");
                Thread.Sleep(500);
                NotificationSystem.Instance.SendSuccess("Обработка - распаковано");
                
                // Ошибка
                NotificationSystem.Instance.SendError("Обработка - ошибка в данных");
                Thread.Sleep(300);
                NotificationSystem.Instance.SendSuccess("Обработка - ошибка исправлена");
                
                // Критическая ошибка
                NotificationSystem.Instance.SendCritical("Обработка - сбой системы");
                Thread.Sleep(400);
                NotificationSystem.Instance.SendSuccess("Обработка - восстановлено");
                
                notifications.Print("[Поток 2] Обработка завершена");
            });
            
            // ГЛАВНЫЙ ПОТОК: Мониторинг
            Thread.Sleep(300);
            NotificationSystem.Instance.SendInformation("Главный - проверка статуса");
            Thread.Sleep(400);
            NotificationSystem.Instance.SendInformation("Главный - всё работает");
            Thread.Sleep(400);
            NotificationSystem.Instance.SendSuccess("Главный - задача выполнена");
            
            notifications.Print("");
            notifications.Print("Нажмите Enter для просмотра истории");
            Console.ReadLine();
            
            // Показываем историю
            notifications.ShowHistory();
            
            notifications.Print("");
            notifications.Print("Нажмите Enter для выхода");
            Console.ReadLine();
            
            notifications.Shutdown();
        }
    }
}