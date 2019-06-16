using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redage.SDK
{
    public class nLog
    {
        /// <summary>
        /// Инициализация системы логирования
        /// </summary>
        /// <param name="reference">Зависимость - Пространство вызова лога, своя пометка в консоли</param>
        /// <param name="canDebug">Включить или отключить вывод отладочных сообщений для всего пространства</param>
        public nLog(string _reference = null, bool _canDebug = false)
        {
            if (_reference == null) _reference = "Logger";
            Reference = _reference;
            CanDebug = _canDebug;
        }
        public string Reference { get; set; }
        public bool CanDebug { get; set; }

        /// <summary>
        /// Флаги (пометки) строк при выводе в консоль
        /// </summary>
        public enum Type
        {
            Info,
            Warn,
            Error,
            Success
        };

        /// <summary>
        /// Вывести в консоль обычный текст с нужным флагом
        /// </summary>
        /// <param name="text">Выводимый текст</param>
        /// <param name="logType">Флаг. Указывает, как нужно пометить строку</param>
        public void Write(string text, Type logType = Type.Info)
        {
            try
            {
                Console.ResetColor();
                Console.Write($"{DateTime.Now.ToString("HH':'mm':'ss.fff")} | ");
                switch (logType)
                {
                    case Type.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Error");
                        break;
                    case Type.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" Warn");
                        break;
                    case Type.Info:
                        Console.Write(" Info");
                        break;
                    case Type.Success:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" Succ");
                        break;
                    default:
                        return;
                }
                Console.ResetColor();
                Console.Write($" | {Reference} | {text}\n");
            }
            catch (Exception e)
            {
                Console.ResetColor();
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Logger Error:\n" + e.ToString());
                Console.ResetColor();
            }
        }
        /// <summary>
        /// Вывести в консоль обычный текст с нужным флагом асинхронно
        /// </summary>
        /// <param name="text"></param>
        /// <param name="logType"></param>
        public async Task WriteAsync(string text, Type logType = Type.Info)
        {
            try
            {
                Console.ResetColor();
                Console.Write($"{DateTime.Now.ToString("HH':'mm':'ss.fff")} | ");
                switch (logType)
                {
                    case Type.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Error");
                        break;
                    case Type.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" Warn");
                        break;
                    case Type.Info:
                        Console.Write(" Info");
                        break;
                    case Type.Success:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" Succ");
                        break;
                    default:
                        return;
                }
                Console.ResetColor();
                Console.Write($" | {Reference} | {text}\n");
            }
            catch (Exception e)
            {
                Console.ResetColor();
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Logger Error:\n" + e.ToString());
                Console.ResetColor();
            }
        }
        /// <summary>
        /// Вывести в консоль отладочный текст с нужным флагом
        /// </summary>
        /// <param name="text">Выводимый текст</param>
        /// <param name="logType">Флаг. Указывает, как нужно пометить строку</param>
        public void Debug(string text, Type logType = Type.Info)
        {
            try
            {
                if (!CanDebug) return;
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{DateTime.Now.ToString("HH':'mm':'ss.fff")}");
                Console.ResetColor();
                Console.Write($" | ");
                switch (logType)
                {
                    case Type.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Error");
                        break;
                    case Type.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" Warn");
                        break;
                    case Type.Info:
                        Console.Write(" Info");
                        break;
                    case Type.Success:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" Succ");
                        break;
                    default:
                        return;
                }
                Console.ResetColor();
                Console.Write($" | {Reference} | {text}\n");
            }
            catch (Exception e)
            {
                Console.ResetColor();
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Logger Error:\n" + e.ToString());
                Console.ResetColor();
            }
        }
        /// <summary>
        /// Вывести в консоль отладочный текст с нужным флагом асинхронно
        /// </summary>
        /// <param name="text">Выводимый текст</param>
        /// <param name="logType">Флаг. Указывает, как нужно пометить строку</param>
        public async Task DebugAsync(string text, Type logType = Type.Info)
        {
            try
            {
                if (!CanDebug) return;
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{DateTime.Now.ToString("HH':'mm':'ss.fff")}");
                Console.ResetColor();
                Console.Write($" | ");
                switch (logType)
                {
                    case Type.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Error");
                        break;
                    case Type.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" Warn");
                        break;
                    case Type.Info:
                        Console.Write(" Info");
                        break;
                    case Type.Success:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" Succ");
                        break;
                    default:
                        return;
                }
                Console.ResetColor();
                Console.Write($" | {Reference} | {text}\n");
            }
            catch (Exception e)
            {
                Console.ResetColor();
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Logger Error:\n" + e.ToString());
                Console.ResetColor();
            }
        }

    }
}
