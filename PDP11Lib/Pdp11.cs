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

    public static class Pdp11
    {
        public static readonly string[] Regs =
        {
            "r0", "r1", "r2", "r3", "r4", "r5", "sp", "pc"
        };

        public static OpCode Read(byte[] data, bool oct)
        {
            if (data.Length < 6) Array.Resize(ref data, 6);
            var bd = new BinData(data) { UseOct = oct };
            return Read(bd, 0);
        }

        public static OpCode Read(BinData bd, int pos)
        {
            switch (bd[pos + 1] >> 4)
            {
                case 0: return Read0(bd, pos);
                case 1: return ReadSrcDst("mov", bd, pos);
                case 2: return ReadSrcDst("cmp", bd, pos);
                case 3: return ReadSrcDst("bit", bd, pos);
                case 4: return ReadSrcDst("bic", bd, pos);
                case 5: return ReadSrcDst("bis", bd, pos);
                case 6: return ReadSrcDst("add", bd, pos);
                case 8: return Read10(bd, pos);
                case 9: return ReadSrcDst("movb", bd, pos);
                case 10: return ReadSrcDst("cmpb", bd, pos);
                case 11: return ReadSrcDst("bitb", bd, pos);
                case 12: return ReadSrcDst("bicb", bd, pos);
                case 13: return ReadSrcDst("bisb", bd, pos);
                case 14: return ReadSrcDst("sub", bd, pos);
                case 15: return Read17(bd, pos);
            }
            return null;
        }

        private static OpCode Read0(BinData bd, int pos)
        {
            switch (bd[pos + 1])
            {
                case 1: return ReadOffset("br", bd, pos);
                case 2: return ReadOffset("bne", bd, pos);
                case 3: return ReadOffset("beq", bd, pos);
                case 4: return ReadOffset("bge", bd, pos);
                case 5: return ReadOffset("blt", bd, pos);
                case 6: return ReadOffset("bgt", bd, pos);
                case 7: return ReadOffset("ble", bd, pos);
            }
            var v = bd.ReadUInt16(pos);
            if (v == 0xa0) return new OpCode("nop", 2);
            int v1 = (v >> 9) & 7, v2 = (v >> 6) & 7;
            switch (v1)
            {
                case 0:
                    switch (v2)
                    {
                        case 0:
                            switch (v & 63)
                            {
                                case 0: return new OpCode("halt", 2);
                                case 1: return new OpCode("wait", 2);
                                case 2: return new OpCode("rti", 2);
                                case 3: return new OpCode("bpt", 2);
                                case 4: return new OpCode("iot", 2);
                                case 5: return new OpCode("reset", 2);
                                case 6: return new OpCode("rtt", 2);
                            }
                            break;
                        case 1: return ReadSrcOrDst("jmp", bd, pos);
                        case 2:
                            switch ((v >> 3) & 7)
                            {
                                case 0: return ReadReg("rts", bd, pos);
                                case 3: return new OpCode("spl " + (v & 7), 2);
                            }
                            break;
                        case 3: return ReadSrcOrDst("swab", bd, pos);
                    }
                    break;
                case 4: return ReadRegSrcOrDst("jsr", bd, pos);
                case 5:
                    switch (v2)
                    {
                        case 0: return ReadSrcOrDst("clr", bd, pos);
                        case 1: return ReadSrcOrDst("com", bd, pos);
                        case 2: return ReadSrcOrDst("inc", bd, pos);
                        case 3: return ReadSrcOrDst("dec", bd, pos);
                        case 4: return ReadSrcOrDst("neg", bd, pos);
                        case 5: return ReadSrcOrDst("adc", bd, pos);
                        case 6: return ReadSrcOrDst("sbc", bd, pos);
                        case 7: return ReadSrcOrDst("tst", bd, pos);
                    }
                    break;
                case 6:
                    switch (v2)
                    {
                        case 0: return ReadSrcOrDst("ror", bd, pos);
                        case 1: return ReadSrcOrDst("rol", bd, pos);
                        case 2: return ReadSrcOrDst("asr", bd, pos);
                        case 3: return ReadSrcOrDst("asl", bd, pos);
                        case 4: return ReadNum("mark", bd, pos);
                        case 5: return ReadSrcOrDst("mfpi", bd, pos);
                        case 6: return ReadSrcOrDst("mtpi", bd, pos);
                        case 7: return ReadSrcOrDst("sxt", bd, pos);
                    }
                    break;
            }
            return null;
        }

        private static OpCode Read7(BinData bd, int pos)
        {
            var v = bd.ReadUInt16(pos);
            switch ((v >> 9) & 7)
            {
                case 0: return ReadRegSrcOrDst("mul", bd, pos);
                case 1: return ReadRegSrcOrDst("div", bd, pos);
                case 2: return ReadRegSrcOrDst("ash", bd, pos);
                case 3: return ReadRegSrcOrDst("ashc", bd, pos);
                case 4: return ReadRegSrcOrDst("xor", bd, pos);
                case 5:
                    switch ((v >> 3) & 63)
                    {
                        case 0: return ReadReg("fadd", bd, pos);
                        case 1: return ReadReg("fsub", bd, pos);
                        case 2: return ReadReg("fmul", bd, pos);
                        case 3: return ReadReg("fdiv", bd, pos);
                    }
                    break;
                case 7: return ReadRegNum("sob", bd, pos);
            }
            return null;
        }

        private static OpCode Read10(BinData bd, int pos)
        {
            switch (bd[pos + 1])
            {
                case 0x80: return ReadOffset("bpl", bd, pos);
                case 0x81: return ReadOffset("bmi", bd, pos);
                case 0x82: return ReadOffset("bhi", bd, pos);
                case 0x83: return ReadOffset("blos", bd, pos);
                case 0x84: return ReadOffset("bvc", bd, pos);
                case 0x85: return ReadOffset("bvs", bd, pos);
                case 0x86: return ReadOffset("bcc", bd, pos);
                case 0x87: return ReadOffset("bcs", bd, pos);
                case 0x88: return new OpCode("emt " + bd.Enc(bd[pos]), 2);
                case 0x89: return new OpCode("trap " + bd.Enc(bd[pos]), 2);
            }
            var v = bd.ReadUInt16(pos);
            switch ((v >> 6) & 63)
            {
                case 0x28: return ReadSrcOrDst("clrb", bd, pos);
                case 0x29: return ReadSrcOrDst("comb", bd, pos);
                case 0x2a: return ReadSrcOrDst("incb", bd, pos);
                case 0x2b: return ReadSrcOrDst("decb", bd, pos);
                case 0x2c: return ReadSrcOrDst("negb", bd, pos);
                case 0x2d: return ReadSrcOrDst("adcb", bd, pos);
                case 0x2e: return ReadSrcOrDst("sbcb", bd, pos);
                case 0x2f: return ReadSrcOrDst("tstb", bd, pos);
                case 0x30: return ReadSrcOrDst("rorb", bd, pos);
                case 0x31: return ReadSrcOrDst("rolb", bd, pos);
                case 0x32: return ReadSrcOrDst("asrb", bd, pos);
                case 0x33: return ReadSrcOrDst("aslb", bd, pos);
                case 0x35: return ReadSrcOrDst("mfpd", bd, pos);
                case 0x36: return ReadSrcOrDst("mtpd", bd, pos);
            }
            return null;
        }

        private static OpCode Read17(BinData bd, int pos)
        {
            var v = bd.ReadUInt16(pos);
            switch (v & 0xfff)
            {
                case 1: return new OpCode("setf", 2);
                case 2: return new OpCode("seti", 2);
                case 9: return new OpCode("setd", 2);
                case 10: return new OpCode("setl", 2);
            }
            return null;
        }

        private static OpCode ReadSrcDst(string op, BinData bd, int pos)
        {
            int len = 2;
            var v = bd.ReadUInt16(pos);
            var v1 = (v >> 9) & 7;
            var v2 = (v >> 6) & 7;
            var v3 = (v >> 3) & 7;
            var v4 = v & 7;
            short v5 = 0, v6 = 0;
            if (hasOperand(v1, v2)) { v5 = (short)bd.ReadUInt16(pos + len); len += 2; }
            if (hasOperand(v3, v4)) { v6 = (short)bd.ReadUInt16(pos + len); len += 2; }
            var opr1 = GetOperand(bd, pos + len, v1, v2, v5);
            var opr2 = GetOperand(bd, pos + len, v3, v4, v6);
            return new OpCode(op + " " + opr1 + ", " + opr2, len);
        }

        private static OpCode ReadSrcOrDst(string op, BinData bd, int pos)
        {
            int len = 2;
            var v = bd.ReadUInt16(pos);
            var v1 = (v >> 3) & 7;
            var v2 = v & 7;
            short v3 = 0;
            if (hasOperand(v1, v2)) { v3 = (short)bd.ReadUInt16(pos + len); len += 2; }
            var opr = GetOperand(bd, pos + len, v1, v2, v3);
            return new OpCode(op + " " + opr, len);
        }

        private static OpCode ReadRegSrcOrDst(string op, BinData bd, int pos)
        {
            var v = bd.ReadUInt16(pos);
            var r = Regs[(v >> 6) & 7];
            return ReadSrcOrDst(op + " " + r + ",", bd, pos);
        }

        private static OpCode ReadNum(string op, BinData bd, int pos)
        {
            return new OpCode(op + " " + bd.Enc((byte)(bd[pos] & 63)), 2);
        }

        private static OpCode ReadRegNum(string op, BinData bd, int pos)
        {
            var v = bd.ReadUInt16(pos);
            var r = Regs[(v >> 6) & 7];
            return ReadNum(op + " " + r + ",", bd, pos);
        }

        private static OpCode ReadOffset(string op, BinData bd, int pos)
        {
            var ad = pos + 2 + ((sbyte)bd[pos]) * 2;
            return new OpCode(op + " " + bd.Enc((ushort)ad), 2);
        }

        private static OpCode ReadReg(string op, BinData bd, int pos)
        {
            var r = Regs[bd[pos] & 7];
            return new OpCode(op + " " + r, 2);
        }

        private static bool hasOperand(int v1, int v2)
        {
            return v1 >= 6 || (v2 == 7 && (v1 == 2 || v1 == 3));
        }

        public static string GetOperand(BinData bd, int pc, int v1, int v2, short v3)
        {
            if (v2 == 7)
            {
                switch (v1)
                {
                    case 2: return "$" + bd.Enc((ushort)v3);
                    case 3: return "*$" + bd.Enc((ushort)v3);
                    case 6: return bd.Enc((ushort)(pc + v3));
                    case 7: return "*" + bd.Enc((ushort)(pc + v3));
                }
            }
            var r = Regs[v2];
            var sign = v3 < 0 ? "-" : "";
            var v3a = Math.Abs(v3);
            var dd = v3a < 10 ? v3.ToString() : sign + bd.Enc((ushort)v3a);
            switch (v1)
            {
                case 0: return r;
                case 1: return "(" + r + ")";
                case 2: return "(" + r + ")+";
                case 3: return "*(" + r + ")+";
                case 4: return "-(" + r + ")";
                case 5: return "*-(" + r + ")";
                case 6: return dd + "(" + r + ")";
                case 7: return "*" + dd + "(" + r + ")";
            }
            throw new Exception("invalid argument");
        }
    }
}
