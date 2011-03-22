using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PDP11Lib
{
    public class AOut
    {
        public ushort fmagic, tsize, dsize, bsize, ssize, entry, pad, relflg;
        public byte[] data;

        public void Read(byte[] data)
        {
            this.data = data;
            fmagic = BitConverter.ToUInt16(data, 0);
            tsize = BitConverter.ToUInt16(data, 2);
            dsize = BitConverter.ToUInt16(data, 4);
            bsize = BitConverter.ToUInt16(data, 6);
            ssize = BitConverter.ToUInt16(data, 8);
            entry = BitConverter.ToUInt16(data, 10);
            pad = BitConverter.ToUInt16(data, 12);
            relflg = BitConverter.ToUInt16(data, 14);
        }

        public void Write(TextWriter iw)
        {
            iw.WriteLine("[{0:x4}] fmagic = {1:x4}", 0, fmagic);
            iw.WriteLine("[{0:x4}] tsize  = {1:x4}", 2, tsize);
            iw.WriteLine("[{0:x4}] dsize  = {1:x4}", 4, dsize);
            iw.WriteLine("[{0:x4}] bsize  = {1:x4}", 6, bsize);
            iw.WriteLine("[{0:x4}] ssize  = {1:x4}", 8, ssize);
            iw.WriteLine("[{0:x4}] entry  = {1:x4}", 10, entry);
            iw.WriteLine("[{0:x4}] pad    = {1:x4}", 12, pad);
            iw.WriteLine("[{0:x4}] relflg = {1:x4}", 14, relflg);
            iw.WriteLine();
            iw.WriteLine(".text");
            for (int i = 0; i < tsize; )
            {
                var op = Pdp11.Read(this, i);
                int len = op != null ? op.Length : 2;
                var s = ReadUInt16(i);
                iw.Write("[{0:x4}] {1:x4}: {2:x4}(o{3})", 16 + i, i, s, Oct(s, 6));
                for (int j = 2; j < 6; j += 2)
                {
                    if (j < len)
                        iw.Write(" {0:x4}", ReadUInt16(i + j));
                    else
                        iw.Write("     ");
                }
                iw.Write("  ");
                if (op != null)
                    iw.Write(op.Mnemonic);
                else
                    iw.Write(string.Format("0x{0:x4}", s));
                iw.WriteLine();
                i += len;
            }
        }

        public static string Oct(uint o, int c)
        {
            var oct = Convert.ToString(o, 8);
            if (oct.Length < c)
                oct = new string('0', c - oct.Length) + oct;
            return oct;
        }

        public byte this[int pos]
        {
            get { return data[16 + pos]; }
        }

        public ushort ReadUInt16(int pos)
        {
            return BitConverter.ToUInt16(data, 16 + pos);
        }

        public string Dump()
        {
            using (var sw = new StringWriter())
            {
                for (int i = 0; i < data.Length; i += 16)
                {
                    sw.Write("[{0:X4}]", i);
                    var sb = new StringBuilder();
                    for (int j = 0; j < 16; j++)
                    {
                        if (i + j < data.Length)
                        {
                            if (j == 8) sw.Write(" -");
                            var b = data[i + j];
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
                return sw.ToString();
            }
        }
    }
}
