using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PDP11Lib
{
    public class BinData
    {
        public int Offset { get; protected set; }
        public byte[] Data { get; protected set; }
        public bool UseOct { get; set; }

        public BinData(byte[] data)
        {
            Data = data;
        }

        public byte this[int pos]
        {
            get { return Data[Offset + pos]; }
        }

        public ushort ReadUInt16(int pos)
        {
            return BitConverter.ToUInt16(Data, Offset + pos);
        }

        public void Dump(TextWriter sw)
        {
            for (int i = 0; i < Data.Length; i += 16)
            {
                sw.Write("[{0:X4}]", i);
                var sb = new StringBuilder();
                for (int j = 0; j < 16; j++)
                {
                    if (i + j < Data.Length)
                    {
                        if (j == 8) sw.Write(" -");
                        var b = Data[i + j];
                        sw.Write(" {0:X2}", b);
                        sb.Append(32 <= b && b < 128 ? (char)b : '.');
                    }
                    else
                    {
                        if (j == 8) sw.Write("  ");
                        sw.Write("   ");
                    }
                }
                sw.WriteLine(" {0}", sb.ToString());
            }
        }

        public string GetDump()
        {
            using (var sw = new StringWriter())
            {
                Dump(sw);
                return sw.ToString();
            }
        }

        public static string Oct(uint o, int c)
        {
            var oct = Convert.ToString(o, 8);
            if (oct.Length < c)
                oct = new string('0', c - oct.Length) + oct;
            return oct;
        }

        public string Enc0(ushort v)
        {
            return UseOct ? Oct(v, 6) : v.ToString("x4");
        }

        public string Enc(ushort v)
        {
            return UseOct ? Oct(v, 7) : "0x" + v.ToString("x4");
        }

        public string Enc(byte v)
        {
            return UseOct ? Oct(v, 4) : "0x" + v.ToString("x2");
        }
    }
}
