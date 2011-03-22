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

        public static OpCode Read(AOut aout, int pos)
        {
            switch (aout[pos + 1] >> 4)
            {
                case 0: return Read0(aout, pos);
                case 1: return ReadSrcDst("mov", aout, pos);
                case 2: return ReadSrcDst("cmp", aout, pos);
                case 3: return ReadSrcDst("bit", aout, pos);
                case 4: return ReadSrcDst("bic", aout, pos);
                case 5: return ReadSrcDst("bis", aout, pos);
                case 6: return ReadSrcDst("add", aout, pos);
                case 8: return Read10(aout, pos);
                case 9: return ReadSrcDst("movb", aout, pos);
                case 10: return ReadSrcDst("cmpb", aout, pos);
                case 11: return ReadSrcDst("bitb", aout, pos);
                case 12: return ReadSrcDst("bicb", aout, pos);
                case 13: return ReadSrcDst("bisb", aout, pos);
                case 14: return ReadSrcDst("sub", aout, pos);
                case 15: return Read17(aout, pos);
            }
            return null;
        }

        private static OpCode Read0(AOut aout, int pos)
        {
            switch (aout[pos + 1])
            {
                case 1: return ReadOffset("br", aout, pos);
                case 2: return ReadOffset("bne", aout, pos);
                case 3: return ReadOffset("beq", aout, pos);
                case 4: return ReadOffset("bge", aout, pos);
                case 5: return ReadOffset("blt", aout, pos);
                case 6: return ReadOffset("bgt", aout, pos);
                case 7: return ReadOffset("ble", aout, pos);
            }
            var v = aout.ReadUInt16(pos);
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
                        case 1: return ReadSrcOrDst("jmp", aout, pos);
                        case 2:
                            switch ((v >> 3) & 7)
                            {
                                case 0: return ReadReg("rts", aout, pos);
                                case 3: return new OpCode("spl " + (v & 7), 2);
                            }
                            break;
                        case 3: return ReadSrcOrDst("swab", aout, pos);
                    }
                    break;
                case 4: return ReadRegSrcOrDst("jsr", aout, pos);
                case 5:
                    switch (v2)
                    {
                        case 0: return ReadSrcOrDst("clr", aout, pos);
                        case 1: return ReadSrcOrDst("com", aout, pos);
                        case 2: return ReadSrcOrDst("inc", aout, pos);
                        case 3: return ReadSrcOrDst("dec", aout, pos);
                        case 4: return ReadSrcOrDst("neg", aout, pos);
                        case 5: return ReadSrcOrDst("adc", aout, pos);
                        case 6: return ReadSrcOrDst("sbc", aout, pos);
                        case 7: return ReadSrcOrDst("tst", aout, pos);
                    }
                    break;
                case 6:
                    switch (v2)
                    {
                        case 0: return ReadSrcOrDst("ror", aout, pos);
                        case 1: return ReadSrcOrDst("rol", aout, pos);
                        case 2: return ReadSrcOrDst("asr", aout, pos);
                        case 3: return ReadSrcOrDst("asl", aout, pos);
                        case 4: return ReadNum("mark", aout, pos);
                        case 5: return ReadSrcOrDst("mfpi", aout, pos);
                        case 6: return ReadSrcOrDst("mtpi", aout, pos);
                        case 7: return ReadSrcOrDst("sxt", aout, pos);
                    }
                    break;
            }
            return null;
        }

        private static OpCode Read7(AOut aout, int pos)
        {
            var v = aout.ReadUInt16(pos);
            switch ((v >> 9) & 7)
            {
                case 0: return ReadRegSrcOrDst("mul", aout, pos);
                case 1: return ReadRegSrcOrDst("div", aout, pos);
                case 2: return ReadRegSrcOrDst("ash", aout, pos);
                case 3: return ReadRegSrcOrDst("ashc", aout, pos);
                case 4: return ReadRegSrcOrDst("xor", aout, pos);
                case 5:
                    switch ((v >> 3) & 63)
                    {
                        case 0: return ReadReg("fadd", aout, pos);
                        case 1: return ReadReg("fsub", aout, pos);
                        case 2: return ReadReg("fmul", aout, pos);
                        case 3: return ReadReg("fdiv", aout, pos);
                    }
                    break;
                case 7: return ReadRegNum("sob", aout, pos);
            }
            return null;
        }

        private static OpCode Read10(AOut aout, int pos)
        {
            switch (aout[pos + 1])
            {
                case 0x80: return ReadOffset("bpl", aout, pos);
                case 0x81: return ReadOffset("bmi", aout, pos);
                case 0x82: return ReadOffset("bhi", aout, pos);
                case 0x83: return ReadOffset("blos", aout, pos);
                case 0x84: return ReadOffset("bvc", aout, pos);
                case 0x85: return ReadOffset("bvs", aout, pos);
                case 0x86: return ReadOffset("bcc", aout, pos);
                case 0x87: return ReadOffset("bcs", aout, pos);
                case 0x88: return new OpCode(string.Format("emt 0x{0:x2}", aout[pos]), 2);
                case 0x89: return new OpCode(string.Format("sys 0x{0:x2}", aout[pos]), 2);
            }
            var v = aout.ReadUInt16(pos);
            switch ((v >> 6) & 63)
            {
                case 0x28: return ReadSrcOrDst("clrb", aout, pos);
                case 0x29: return ReadSrcOrDst("comb", aout, pos);
                case 0x2a: return ReadSrcOrDst("incb", aout, pos);
                case 0x2b: return ReadSrcOrDst("decb", aout, pos);
                case 0x2c: return ReadSrcOrDst("negb", aout, pos);
                case 0x2d: return ReadSrcOrDst("adcb", aout, pos);
                case 0x2e: return ReadSrcOrDst("sbcb", aout, pos);
                case 0x2f: return ReadSrcOrDst("tstb", aout, pos);
                case 0x30: return ReadSrcOrDst("rorb", aout, pos);
                case 0x31: return ReadSrcOrDst("rolb", aout, pos);
                case 0x32: return ReadSrcOrDst("asrb", aout, pos);
                case 0x33: return ReadSrcOrDst("aslb", aout, pos);
                case 0x35: return ReadSrcOrDst("mfpd", aout, pos);
                case 0x36: return ReadSrcOrDst("mtpd", aout, pos);
            }
            return null;
        }

        private static OpCode Read17(AOut aout, int pos)
        {
            var v = aout.ReadUInt16(pos);
            switch (v & 0xfff)
            {
                case 1: return new OpCode("setf", 2);
                case 2: return new OpCode("seti", 2);
                case 9: return new OpCode("setd", 2);
                case 10: return new OpCode("setl", 2);
            }
            return null;
        }

        private static OpCode ReadSrcDst(string op, AOut aout, int pos)
        {
            int len = 2;
            var v = aout.ReadUInt16(pos);
            var v1 = (v >> 9) & 7;
            var v2 = (v >> 6) & 7;
            var v3 = (v >> 3) & 7;
            var v4 = v & 7;
            short v5 = 0, v6 = 0;
            if (hasOperand(v1, v2)) { v5 = (short)aout.ReadUInt16(pos + len); len += 2; }
            if (hasOperand(v3, v4)) { v6 = (short)aout.ReadUInt16(pos + len); len += 2; }
            var opr1 = GetOperand(pos + len, v1, v2, v5);
            var opr2 = GetOperand(pos + len, v3, v4, v6);
            return new OpCode(op + " " + opr1 + ", " + opr2, len);
        }

        private static OpCode ReadSrcOrDst(string op, AOut aout, int pos)
        {
            int len = 2;
            var v = aout.ReadUInt16(pos);
            var v1 = (v >> 3) & 7;
            var v2 = v & 7;
            short v3 = 0;
            if (hasOperand(v1, v2)) { v3 = (short)aout.ReadUInt16(pos + len); len += 2; }
            var opr = GetOperand(pos + len, v1, v2, v3);
            return new OpCode(op + " " + opr, len);
        }

        private static OpCode ReadRegSrcOrDst(string op, AOut aout, int pos)
        {
            var v = aout.ReadUInt16(pos);
            var r = Regs[(v >> 6) & 7];
            return ReadSrcOrDst(op + " " + r + ",", aout, pos);
        }

        private static OpCode ReadNum(string op, AOut aout, int pos)
        {
            return new OpCode(string.Format("{0} 0x{1:x2}", op, aout[pos] & 63), 2);
        }

        private static OpCode ReadRegNum(string op, AOut aout, int pos)
        {
            var v = aout.ReadUInt16(pos);
            var r = Regs[(v >> 6) & 7];
            return ReadNum(op + " " + r + ",", aout, pos);
        }

        private static OpCode ReadOffset(string op, AOut aout, int pos)
        {
            var ad = pos + 2 + ((sbyte)aout[pos]) * 2;
            return new OpCode(string.Format("{0} 0x{1:x4}", op, ad), 2);
        }

        private static OpCode ReadReg(string op, AOut aout, int pos)
        {
            var r = Regs[aout[pos] & 7];
            return new OpCode(op + " " + r, 2);
        }

        private static bool hasOperand(int v1, int v2)
        {
            return v1 >= 6 || (v2 == 7 && (v1 == 2 || v1 == 3));
        }

        public static string GetOperand(int pc, int v1, int v2, short v3)
        {
            if (v2 == 7)
            {
                switch (v1)
                {
                    case 2: return string.Format("$0x{0:x4}", (ushort)v3);
                    case 3: return string.Format("*$0x{0:x4}", (ushort)v3);
                    case 6: return string.Format("0x{0:x4}", pc + v3);
                    case 7: return string.Format("*0x{0:x4}", pc + v3);
                }
            }
            var r = Regs[v2];
            var sign = v3 < 0 ? "-" : "";
            var v3a = Math.Abs(v3);
            var dd = v3a < 10 ? v3.ToString() : sign + "0x" + v3a.ToString("x");
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
