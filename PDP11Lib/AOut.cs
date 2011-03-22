using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PDP11Lib
{
    public class AOut
    {
        public ushort fmagic, tsize, dsize, bsize, ssize, entry, pad, relflg;
        public byte[] image;

        public void Read(BinaryReader br)
        {
            fmagic = br.ReadUInt16();
            tsize = br.ReadUInt16();
            dsize = br.ReadUInt16();
            bsize = br.ReadUInt16();
            ssize = br.ReadUInt16();
            entry = br.ReadUInt16();
            pad = br.ReadUInt16();
            relflg = br.ReadUInt16();
            image = br.ReadBytes(tsize + dsize);
        }

        public void Write(TextWriter iw)
        {
            iw.WriteLine("fmagic = {0:x4}", fmagic);
            iw.WriteLine("tsize  = {0:x4}", tsize);
            iw.WriteLine("dsize  = {0:x4}", dsize);
            iw.WriteLine("bsize  = {0:x4}", bsize);
            iw.WriteLine("ssize  = {0:x4}", ssize);
            iw.WriteLine("entry  = {0:x4}", entry);
            iw.WriteLine("pad    = {0:x4}", pad);
            iw.WriteLine("relflg = {0:x4}", relflg);
            iw.WriteLine();
            iw.WriteLine("[.text]");
            for (int i = 0; i < tsize; )
            {
                var op = Pdp11.Read(image, i);
                int len = op != null ? op.Length : 2;
                var s = BitConverter.ToUInt16(image, i);
                iw.Write("{0:x4}:  {1:x4}(o{2})", i, s, Oct(s, 6));
                for (int j = 2; j < 6; j += 2)
                {
                    if (j < len)
                        iw.Write(" {0:x4}", BitConverter.ToUInt16(image, i + j));
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
    }
}
