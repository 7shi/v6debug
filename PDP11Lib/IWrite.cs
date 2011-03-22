using System;
using System.Collections.Generic;
using System.Text;

namespace PDP11Lib
{
    public interface IWrite
    {
        void Write(string format, params object[] args);
        void WriteLine();
        void WriteLine(string format, params object[] args);
    }
}
