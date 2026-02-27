using System;

namespace NotificationSingletonDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            NotificationSystem.Instance.RunDemo();
        }
    }
}