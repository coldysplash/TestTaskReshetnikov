using System;
using System.IO;

namespace TestTask
{
    public class ReadOnlyStream : IReadOnlyStream
    {
        private FileStream _fileStream;
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

            _fileStream = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read);
            IsEof = false;
        }

        /// <summary>
        /// Проверяет файл на существование
        /// </summary>
        /// <param name="fileFullPath"></param>
        private void VerifyFile(string fileFullPath)
        {
            if (!File.Exists(fileFullPath))
            {
                throw new FileNotFoundException($"Файл не найден: {fileFullPath}");
            }
        }

        /// <summary>
        /// Ф-ция чтения следующего символа из потока.
        /// Если произведена попытка прочитать символ после достижения конца файла, метод 
        /// должен бросать соответствующее исключение
        /// </summary>
        /// <returns>Считанный символ.</returns>
        public char ReadNextChar()
        {
            IfDisposedThrow();

            int charCode = _fileStream.ReadByte();
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

            if (_fileStream == null)
            {
                IsEof = true;
                return;
            }

            try
            {
                _fileStream.Position = 0;
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
                _fileStream?.Dispose();
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
