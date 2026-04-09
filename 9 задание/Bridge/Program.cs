using System.Text;
using SmartHomeBridge;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        BridgeDemo.Run(Console.Out);
    }
}
