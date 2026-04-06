using System;

namespace Proxy
{
    public class User
    {
        public string Name { get; }
        public string Role { get; }

        public User(string name, string role)
        {
            Name = name;
            Role = role;
            Console.WriteLine($"[USER] Создан пользователь: {name} (роль: {role})");
        }
    }
}