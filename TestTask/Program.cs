using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestTask
{
    public class Program
    {

        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Ошибка: необходимо передать 2 пути до файлов.");
                    return;
                }

                using (IReadOnlyStream inputStream1 = GetInputStream(args[0]))
                using (IReadOnlyStream inputStream2 = GetInputStream(args[1]))
                {
                    IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
                    IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);

                    RemoveCharStatsByType(singleLetterStats, CharType.Vowel);
                    RemoveCharStatsByType(doubleLetterStats, CharType.Consonants);

                    PrintStatistic(singleLetterStats);
                    PrintStatistic(doubleLetterStats);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
            return new ReadOnlyStream(fileFullPath);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {
            var letterStats = new Dictionary<string, LetterStats>();

            stream.ResetPositionToStart();
            while (!stream.IsEof)
            {
                char c = stream.ReadNextChar();
                if (char.IsLetter(c))
                {
                    string letter = c.ToString();
                    if (letterStats.ContainsKey(letter))
                    {
                        LetterStats singleStat = letterStats[letter];
                        IncStatistic(ref singleStat);
                        letterStats[letter] = singleStat;
                    }
                    else
                    {
                        letterStats[letter] = new LetterStats { Letter = letter, Count = 1};
                    }
                }
            }

            return letterStats.Values.ToList();
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            var letterStats = new Dictionary<string, LetterStats>();
            char? prevChar = null;

            stream.ResetPositionToStart();
            while (!stream.IsEof)
            {
                char c = stream.ReadNextChar();
                if (char.IsLetter(c))
                {
                    if (prevChar.HasValue && char.ToUpper(prevChar.Value) == char.ToUpper(c))
                    {
                        string pair = char.ToUpper(prevChar.Value).ToString() + char.ToUpper(c).ToString();
                        if (letterStats.ContainsKey(pair))
                        {
                            LetterStats singleStat = letterStats[pair];
                            IncStatistic(ref singleStat);
                            letterStats[pair] = singleStat;
                        }
                        else
                        {
                            letterStats[pair] = new LetterStats { Letter = pair, Count = 1 };
                        }
                    }
                    prevChar = c;
                }
                else
                {
                    prevChar = null;
                }
            }

            return letterStats.Values.ToList();
        }

        private static readonly HashSet<char> Vowels = new HashSet<char>
        {
            'А', 'Е', 'Ё', 'И', 'О', 'У', 'Ы', 'Э', 'Ю', 'Я',
            'A', 'E', 'I', 'O', 'U'
        };

        /// <summary>
        /// Проверяет, является ли буква гласной.
        /// </summary>
        /// <param name="letter">Буква или пара букв для проверки</param>
        /// <returns>true, если все символы - гласные; false в противном случае</returns>
        private static bool IsVowel(string letter)
        {
            if (string.IsNullOrEmpty(letter))
                return false;

            return letter.All(c => Vowels.Contains(char.ToUpper(c)));
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static void RemoveCharStatsByType(IList<LetterStats> letters, CharType charType)
        {
            if (letters is List<LetterStats> list)
            {
                list.RemoveAll(stat =>
                    charType == CharType.Consonants
                        ? !IsVowel(stat.Letter)
                        : IsVowel(stat.Letter)
                );
            }
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        private static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            var sortedLetters = letters.OrderBy(x => x.Letter).ToList();
            int totalCount = 0;

            foreach (var stat in sortedLetters)
            {
                Console.WriteLine($"{stat.Letter} : {stat.Count}");
                totalCount += stat.Count;
            }

            Console.WriteLine($"ИТОГО: {totalCount}");
        }

        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats"></param>
        private static void IncStatistic(ref LetterStats letterStats)
        {
            letterStats.Count++;
        }
    }
}
