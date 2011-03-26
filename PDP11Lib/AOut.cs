using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PDP11Lib
{
    public class AOut : BinData
    {
        public ushort fmagic, tsize, dsize, bsize, ssize, entry, pad, relflg;

        public AOut(byte[] data)
            : base(data)
        {
            Offset = 16;
            fmagic = BitConverter.ToUInt16(data, 0);
            tsize = BitConverter.ToUInt16(data, 2);
            dsize = BitConverter.ToUInt16(data, 4);
            bsize = BitConverter.ToUInt16(data, 6);
            ssize = BitConverter.ToUInt16(data, 8);
            entry = BitConverter.ToUInt16(data, 10);
            pad = BitConverter.ToUInt16(data, 12);
            relflg = BitConverter.ToUInt16(data, 14);
        }

        public void Disassemble(TextWriter iw)
        {
            var opmagic = Disassembler.Read(new[] { Data[0], Data[1] }, UseOct);
            iw.WriteLine("[{0:x4}] fmagic = {1}  {2}", 0, Enc0(fmagic), opmagic.Mnemonic);
            iw.WriteLine("[{0:x4}] tsize  = {1}", 2, Enc0(tsize));
            iw.WriteLine("[{0:x4}] dsize  = {1}", 4, Enc0(dsize));
            iw.WriteLine("[{0:x4}] bsize  = {1}", 6, Enc0(bsize));
            iw.WriteLine("[{0:x4}] ssize  = {1}", 8, Enc0(ssize));
            iw.WriteLine("[{0:x4}] entry  = {1}", 10, Enc0(entry));
            iw.WriteLine("[{0:x4}] pad    = {1}", 12, Enc0(pad));
            iw.WriteLine("[{0:x4}] relflg = {1}", 14, Enc0(relflg));
            iw.WriteLine();
            iw.WriteLine(".text");
            for (int i = 0; i < tsize; )
            {
                var op = Disassembler.Read(this, i);
                int len = op != null ? op.Length : 2;
                var s = ReadUInt16(i);
                iw.Write("[{0:x4}] {1}: {2}", 16 + i, Enc0((ushort)i), Enc0(s));
                for (int j = 2; j < 6; j += 2)
                {
                    if (j < len)
                        iw.Write(" " + Enc0(ReadUInt16(i + j)));
                    else if (UseOct)
                        iw.Write("       ");
                    else
                        iw.Write("     ");
                }
                iw.Write("  ");
                if (op != null)
                    iw.Write(op.Mnemonic);
                else
                    iw.Write(Enc(s));
                iw.WriteLine();
                i += len;
            }
        }

        public string GetDisassemble()
        {
            using (var sw = new StringWriter())
            {
                Disassemble(sw);
                return sw.ToString();
            }
        }
    }
}
