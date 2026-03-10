using System;
using System.Threading.Tasks;

namespace ObjectPoolBulletExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var simulation = new BattleSimulation();
            await simulation.RunAsync();
        }
    }
}
