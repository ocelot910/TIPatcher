using System;
using System.Collections.Generic;
using System.Text;

namespace TIPatcher.Interfaces
{
    public interface ILogger
    {
        void Log(string message);
        void WaitForInput();
        string Ask();
    }
}
