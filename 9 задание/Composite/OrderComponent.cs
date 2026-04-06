using System.Collections.Generic;

namespace CompositeOrderSystem
{
    // Базовый компонент (Component) паттерна Composite.
    abstract class OrderComponent
    {
        public string Name { get; protected set; }

        protected OrderComponent(string name)
        {
            Name = name;
        }

        // Возвращает общую стоимость компонента
        public abstract decimal GetTotalPrice();
        // Печатает компонент с указанным уровнем вложенности
        public abstract void PrintOrder(int depth = 0);
        // Возвращает общее количество товаров в компоненте
        public abstract int GetTotalItems();
        // Возвращает среднюю цену для компонента
        public abstract decimal GetAveragePrice();
        // Собирает все наименования товаров из компонента
        public abstract List<string> GetAllNames();

        // Композиция: только Composite переопределяет эти методы
        public virtual void Add(OrderComponent component) { }
        public virtual void Remove(OrderComponent component) { }
    }
}
