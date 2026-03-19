using System;

namespace LazyInitializationReports
{
    // Значение создаётся только при первом обращении к Value
    public sealed class Lazy<T>
    {
        private readonly Func<T> _valueFactory;
        private readonly object _lock = new object();
        private bool _isValueCreated;
        private T? _value;

        // Конструктор принимающий фабрику для создания объекта
        public Lazy(Func<T> valueFactory)
        {
            _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
        }

        //Возвращает true, если значение уже было создано
        public bool IsValueCreated => _isValueCreated;

        public T Value
        {
            get
            {
                // Быстрый путь без блокировки, когда значение уже есть
                if (_isValueCreated)
                {
                    return _value!;
                }

                // Только первый поток дойдёт до создания
                lock (_lock)
                {
                    if (!_isValueCreated)
                    {
                        // Фабрика может быть дорогостоящей, выполняем один раз
                        _value = _valueFactory();
                        _isValueCreated = true;
                    }

                    return _value!;
                }
            }
        }

        // Сбрасывает состояние, чтобы значение создалось заново при следующем обращении
        public void Reset()
        {
            lock (_lock)
            {
                _isValueCreated = false;
                _value = default;
            }
        }
    }
}
