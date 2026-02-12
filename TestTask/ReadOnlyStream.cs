using System;
using System.IO;

namespace TestTask
{
    public class ReadOnlyStream : IReadOnlyStream, IDisposable
    {
        private StreamReader _reader;
        private bool _disposed;

        /// <summary>
        /// Флаг окончания файла.
        /// </summary>
        public bool IsEof { get; private set; }

        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        public ReadOnlyStream(string fileFullPath)
        {
            VerifyFile(fileFullPath);
            try
            {
                _reader = new StreamReader(fileFullPath); // По умолчанию UTF-8, можно указать Encoding если нужно
                IsEof = false;
            }
            catch (Exception ex)
            {
                throw new IOException($"Ошибка открытия файла: {fileFullPath}", ex);
            }
        }

        /// <summary>
        /// Проверяет файл на существование и доступ
        /// </summary>
        /// <param name="fileFullPath"></param>
        private void VerifyFile(string fileFullPath)
        {
            if (string.IsNullOrEmpty(fileFullPath))
            {
                throw new ArgumentNullException(nameof(fileFullPath));
            }
            if (!File.Exists(fileFullPath))
            {
                throw new FileNotFoundException($"Файл не найден: {fileFullPath}");
            }
            // Проверка на чтение
            try
            {
                using (var fs = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read)) { }
            }
            catch
            {
                throw new IOException($"Нет доступа к файлу: {fileFullPath}");
            }
        }

        /// <summary>
        /// Ф-ция чтения следующего символа из потока.
        /// Если произведена попытка прочитать символ после достижения конца файла,
        /// выставляется флаг IsEof = true
        /// </summary>
        /// <returns>Считанный символ.</returns>
        public char ReadNextChar()
        {
            IfDisposedThrow();

            int charCode = _reader.Read();
            if (charCode == -1)
            {
                IsEof = true;
            }
            return (char)charCode;
        }

        /// <summary>
        /// Сбрасывает текущую позицию потока на начало.
        /// </summary>
        public void ResetPositionToStart()
        {
            IfDisposedThrow();
            if (_reader == null)
            {
                IsEof = true;
                return;
            }
            try
            {
                _reader.BaseStream.Position = 0;
                _reader.DiscardBufferedData();
                IsEof = false;
            }
            catch (Exception ex)
            {
                IsEof = true;
                throw new InvalidOperationException("Ошибка при сбросе позиции стрима.", ex);
            }
        }

        private void IfDisposedThrow()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>
        /// Освобождает ресурсы, используемые потоком.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            try
            {
                _reader?.Dispose();
            }
            catch
            {
                // Игнорируем ошибки при закрытии
            }
            _disposed = true;
            IsEof = true;
        }
    }
}