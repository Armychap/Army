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
            Console.WriteLine($"  [User] Создан пользователь: {name} (роль: {role})");
        }
    }
}