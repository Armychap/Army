using System;

namespace NotificationSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("СИСТЕМА ОПОВЕЩЕНИЙ\n");

            Menu menu = new Menu();
            menu.Run();
        }
    }
}