using System;
using System.Collections.Generic;
using System.Text;

namespace v6
{
    interface IWrite
    {
        void Write(string format, params object[] args);
        void WriteLine();
        void WriteLine(string format, params object[] args);
    }
}
