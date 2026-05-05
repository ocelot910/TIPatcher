using System;
using System.Collections.Generic;
using System.Text;

namespace TIPatcher.Interfaces
{
    public interface ILogger
    {
        void Log(string message);
        void Error(string message);
        void Warning(string message);
        void Info(string message);
        void WaitForInput();
        string Ask();
    }
}
