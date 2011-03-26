using System;
using System.Collections.Generic;
using System.Text;

namespace PDP11Lib
{
    public class OpCode
    {
        public string Mnemonic { get; private set; }
        public int Length { get; private set; }

        public OpCode(string mnemonic, int length)
        {
            Mnemonic = mnemonic;
            Length = length;
        }
    }
}
