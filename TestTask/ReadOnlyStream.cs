using System;
using System.IO;

namespace TestTask
{
    public class ReadOnlyStream : IReadOnlyStream
    {
        private FileStream _fileStream;
        private StreamReader _streamReader;
        private bool _disposed = false;

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
            _streamReader = new StreamReader(_fileStream, System.Text.Encoding.UTF8);
            IsEof = false;
        }

        /// <summary>
        /// Проверяет путь к файлу и его существование
        /// </summary>
        /// <param name="fileFullPath"></param>
        private void VerifyFile(string fileFullPath)
        {
            if (string.IsNullOrEmpty(fileFullPath))
            {
                throw new ArgumentNullException(nameof(fileFullPath), "Путь к файлу не может быть null или пустым.");
            }
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

            int charCode = _streamReader.Read();
            if (charCode == -1)
            {
                IsEof = true;
                throw new EndOfStreamException("Достигнут конец файла.");
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
                _streamReader?.Dispose();
                _streamReader = new StreamReader(_fileStream, System.Text.Encoding.UTF8);
                IsEof = false;
            }
            catch (Exception ex)
            {
                IsEof = true;
                throw new InvalidOperationException("Ошибка при сбросе позиции.", ex);
            }
        }

        private void IfDisposedThrow()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }


        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _streamReader?.Dispose();
                _fileStream?.Dispose();
            }
            catch { }

            _disposed = true;
            IsEof = true;
        }
    }
}
