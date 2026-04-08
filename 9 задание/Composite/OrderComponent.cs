using System.Collections.Generic;

namespace CompositeOrderSystem
{
    //Component
    abstract class OrderComponent
    {
        public string Name { get; protected set; }

        protected OrderComponent(string name)
        {
            Name = name;
        }
        // Базовые операции, которые должны реализовать все компоненты

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

        // Операции для работы с вложенными элементами
        // Композиция: только Composite переопределяет эти методы
        public virtual void Add(OrderComponent component) { }
        public virtual void Remove(OrderComponent component) { }
    }
}
