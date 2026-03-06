using System;

namespace NotificationSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("СИСТЕМА ОПОВЕЩЕНИЙ\n");

            SingletonDemo.ShowDemonstration();

            Menu menu = new Menu();
            menu.Run();
        }
    }
}