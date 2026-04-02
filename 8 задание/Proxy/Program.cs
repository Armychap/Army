using System;

namespace Proxy
{
    class Program
    {
        static void Main(string[] args)
        {
            Demo demo = new Demo();
            demo.Run();

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey(true);
        }
    }
}