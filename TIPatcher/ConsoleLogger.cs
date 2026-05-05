using System;
using System.Collections.Generic;
using System.Text;
using TIPatcher.Interfaces;

namespace TIPatcher
{
    public class ConsoleLogger : ILogger
    {
        private readonly string _prefix = "";
        private readonly ConsoleColor _prefixColour = ConsoleColor.Cyan;
        public ConsoleLogger(string prefix, ConsoleColor prefixColour)
            => (_prefix, _prefixColour) = (prefix, prefixColour);
        public ConsoleLogger() { }

        public string Ask()
        {
            WriteColour("> ", ConsoleColor.Green);
            return Console.ReadLine() ?? "";
        }

        public void Log(string message)
        {
            AddPrefix();
            Console.WriteLine(message);
        }

        public void Error(string message)
        {
            AddPrefix();
            WriteColour("[ERROR] ", ConsoleColor.DarkRed);
            Console.WriteLine(message);
        }

        public void Warning(string message)
        {
            AddPrefix();
            WriteColour("[WARNING] ", ConsoleColor.Red);
            Console.WriteLine(message);
        }
        public void Info(string message)
        {
            AddPrefix();
            WriteColour("[INFO] ", ConsoleColor.Yellow);
            Console.WriteLine(message);
        }

        public void WaitForInput()
        {
            Console.ReadKey();
        }

        private void AddPrefix()
        {
            if (_prefix.Length > 0)
            {
                WriteColour(_prefix.PadRight(_prefix.Length + 1), _prefixColour);
            }
        }

        private static void WriteColour(string message, ConsoleColor colour)
        {
            Console.ForegroundColor = colour;
            Console.Write(message);
            Console.ResetColor();
        }
    }
}
