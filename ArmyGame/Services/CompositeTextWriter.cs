using System;
using System.IO;

namespace ArmyBattle.Services
{
    /// <summary>
    /// TextWriter, который одновременно пишет в несколько потоков вывода.
    /// Практическое применение:
    /// Позволяет одновременно писать в консоль И в файл (StringWriter).
    /// Это необходимо для логирования битв с одновременным отображением на экране.
    /// </summary>
    public class CompositeTextWriter : TextWriter
    {
        // Массив всех писателей, в которые нужно писать одновременно
        private readonly TextWriter[] _writers;
        
        // Поставщик формата (запятые, точки и т.д.) из первого писателя
        private readonly IFormatProvider _formatProvider;

        /// <summary>
        /// Конструктор инициализирует составной писатель с несколькими целевыми писателями.
        /// </summary>
        public CompositeTextWriter(params TextWriter[] writers)
        {
            // Сохраняем массив писателей с проверкой на null
            _writers = writers ?? throw new ArgumentNullException(nameof(writers));
            
            // Получаем форматирование из первого писателя (если он есть)
            _formatProvider = writers.Length > 0 ? writers[0].FormatProvider : null;
        }

        /// <summary>
        /// Получает поставщик формата для текущего писателя.
        /// Используется для правильного форматирования чисел, дат и т.д.
        /// </summary>
        public override IFormatProvider FormatProvider => _formatProvider;

        /// <summary>
        /// Возвращает кодировку текста
        /// Требуется для реализации абстрактного свойства TextWriter.
        /// </summary>
        public override System.Text.Encoding Encoding
        {
            // Возвращаем UTF-8 кодировку для поддержки русских символов и других текстов
            get { return System.Text.Encoding.UTF8; }
        }

        /// <summary>
        /// Пишет одиночный символ во все целевые писатели одновременно.
        /// </summary>
        public override void Write(char value)
        {
            // Итерируемся по каждому писателю
            foreach (var writer in _writers)
                // Пишем символ в текущий писатель
                writer.Write(value);
        }

        /// <summary>
        /// Пишет строку текста во все целевые писатели одновременно.
        /// </summary>
        public override void Write(string value)
        {
            // Итерируемся по каждому писателю
            foreach (var writer in _writers)
                // Пишем строку в текущий писатель
                writer.Write(value);
        }

        /// <summary>
        /// Пишет пустую строку (перевод строки) во все целевые писатели
        /// </summary>
        public override void WriteLine()
        {
            // Итерируемся по каждому писателю
            foreach (var writer in _writers)
                // Пишем новую строку в текущий писатель
                writer.WriteLine();
        }

        /// <summary>
        /// Пишет строку с переводом строки во все целевые писатели одновременно.
        /// </summary>
        public override void WriteLine(string value)
        {
            // Итерируемся по каждому писателю
            foreach (var writer in _writers)
                // Пишем строку с новой строкой в текущий писатель
                writer.WriteLine(value);
        }

        /// <summary>
        /// Пишет форматированную строку во все целевые писатели одновременно.
        /// Позволяет использовать форматирование как в Console.WriteLine.
        /// </summary>
        public override void WriteLine(string format, params object[] arg)
        {
            // Итерируемся по каждому писателю
            foreach (var writer in _writers)
                // Пишем форматированную строку в текущий писатель
                writer.WriteLine(format, arg);
        }

        /// <summary>
        /// Пишет массив символов во все целевые писатели одновременно.
        /// </summary>
        public override void Write(char[] buffer, int index, int count)
        {
            // Итерируемся по каждому писателю
            foreach (var writer in _writers)
                // Пишем часть буфера в текущий писатель
                writer.Write(buffer, index, count);
        }

        /// <summary>
        /// Очищает внутренние буферы всех целевых писателей.
        /// Гарантирует, что все данные записаны приложением операционной системе.
        /// </summary>
        public override void Flush()
        {
            // Итерируемся по каждому писателю
            foreach (var writer in _writers)
                // Очищаем буфер текущего писателя
                writer.Flush();
        }

        /// <summary>
        /// Освобождает все ресурсы, захваченные всеми целевыми писателями.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            // Проверяем что это явное освобождение (не из финализатора)
            if (disposing)
            {
                // Итерируемся по каждому писателю
                foreach (var writer in _writers)
                    // Освобождаем ресурсы текущего писателя
                    writer.Dispose();
            }
            
            // Вызываем базовый метод для правильной очистки
            base.Dispose(disposing);
        }
    }
}
