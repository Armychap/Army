using System;

namespace LazyInitializationReports
{
    /// <summary>
    /// Собственная реализация ленивой инициализации (без System.Lazy).
    /// Значение создаётся только при первом обращении к Value.
    /// Потокобезопасно через double-check + lock.
    /// </summary>
    public sealed class Lazy<T>
    {
        private readonly Func<T> _valueFactory;
        private readonly object _lock = new object();
        private bool _isValueCreated;
        private T? _value;

        public Lazy(Func<T> valueFactory)
        {
            _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
        }

        public bool IsValueCreated => _isValueCreated;

        public T Value
        {
            get
            {
                if (_isValueCreated)
                {
                    return _value!;
                }

                lock (_lock)
                {
                    if (!_isValueCreated)
                    {
                        _value = _valueFactory();
                        _isValueCreated = true;
                    }

                    return _value!;
                }
            }
        }
    }
}
