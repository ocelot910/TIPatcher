using System;
using System.Collections.Generic;
using System.Text;
using TIPatcher.Interfaces;

namespace TIPatcher
{
    public class ConsoleLogger : ILogger
    {
        public string Ask()
        {
            return Console.ReadLine() ?? "";
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void WaitForInput()
        {
            Console.ReadKey();
        }
    }
}
