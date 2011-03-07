using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace v6
{
    class AOut
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

        public void Write(IWrite iw)
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
            iw.WriteLine(".text");
            for (int i = 0; i < tsize; i += 2)
            {
                var s = BitConverter.ToUInt16(image, i);
                iw.WriteLine("{0:x4}: {1:x4}(o{2})", i, s, Oct(s, 6));
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
