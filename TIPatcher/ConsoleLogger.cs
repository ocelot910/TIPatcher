using System;
using System.Collections.Generic;
using System.Text;
using TIPatcher.Interfaces;

namespace TIPatcher
{
    public class ConsoleLogger : ILogger
    {
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
