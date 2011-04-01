Public Module Disassembler
    Public ReadOnly RegNames As String() =
        {"r0", "r1", "r2", "r3", "r4", "r5", "sp", "pc"}

    Public ReadOnly SectionNames As String() = {Nothing, ".text", ".data", ".bss"}

    Public Function Disassemble(data As Byte(), oct As Boolean) As OpCode
        If data.Length < 6 Then Array.Resize(data, 6)
        Dim bd = New BinData(data) With {.UseOct = oct}
        Return Disassemble(bd, 0)
    End Function

    Public Function Disassemble(bd As BinData, pos%) As OpCode
        Select Case bd(pos + 1) >> 4
            Case 0 : Return Read0(bd, pos)
            Case 1 : Return ReadSrcDst("mov", bd, pos)
            Case 2 : Return ReadSrcDst("cmp", bd, pos)
            Case 3 : Return ReadSrcDst("bit", bd, pos)
            Case 4 : Return ReadSrcDst("bic", bd, pos)
            Case 5 : Return ReadSrcDst("bis", bd, pos)
            Case 6 : Return ReadSrcDst("add", bd, pos)
            Case 7 : Return Read7(bd, pos)
            Case &O10 : Return Read10(bd, pos)
            Case &O11 : Return ReadSrcDst("movb", bd, pos)
            Case &O12 : Return ReadSrcDst("cmpb", bd, pos)
            Case &O13 : Return ReadSrcDst("bitb", bd, pos)
            Case &O14 : Return ReadSrcDst("bicb", bd, pos)
            Case &O15 : Return ReadSrcDst("bisb", bd, pos)
            Case &O16 : Return ReadSrcDst("sub", bd, pos)
            Case &O17 : Return Read17(bd, pos)
        End Select
        Return Nothing
    End Function

    Private Function Read0(bd As BinData, pos%) As OpCode
        Select Case bd(pos + 1)
            Case 1 : Return ReadOffset("br", bd, pos)
            Case 2 : Return ReadOffset("bne", bd, pos)
            Case 3 : Return ReadOffset("beq", bd, pos)
            Case 4 : Return ReadOffset("bge", bd, pos)
            Case 5 : Return ReadOffset("blt", bd, pos)
            Case 6 : Return ReadOffset("bgt", bd, pos)
            Case 7 : Return ReadOffset("ble", bd, pos)
        End Select
        Dim v = bd.ReadUInt16(pos)
        If v = &HA0 Then Return New OpCode("nop", 2)
        Dim v1 = (v >> 9) And 7, v2 = (v >> 6) And 7
        Select Case v1
            Case 0
                Select Case v2
                    Case 0
                        Select Case v And &O77
                            Case 0 : Return New OpCode("halt", 2)
                            Case 1 : Return New OpCode("wait", 2)
                            Case 2 : Return New OpCode("rti", 2)
                            Case 3 : Return New OpCode("bpt", 2)
                            Case 4 : Return New OpCode("iot", 2)
                            Case 5 : Return New OpCode("reset", 2)
                            Case 6 : Return New OpCode("rtt", 2)
                        End Select
                    Case 1 : Return ReadDst("jmp", bd, pos)
                    Case 2
                        Select Case (v >> 3) And 7
                            Case 0 : Return ReadReg("rts", bd, pos)
                            Case 3 : Return New OpCode("spl " + (v & 7), 2)
                        End Select
                    Case 3 : Return ReadDst("swab", bd, pos)
                End Select
            Case 4 : Return ReadRegDst("jsr", bd, pos)
            Case 5
                Select Case v2
                    Case 0 : Return ReadDst("clr", bd, pos)
                    Case 1 : Return ReadDst("com", bd, pos)
                    Case 2 : Return ReadDst("inc", bd, pos)
                    Case 3 : Return ReadDst("dec", bd, pos)
                    Case 4 : Return ReadDst("neg", bd, pos)
                    Case 5 : Return ReadDst("adc", bd, pos)
                    Case 6 : Return ReadDst("sbc", bd, pos)
                    Case 7 : Return ReadDst("tst", bd, pos)
                End Select
            Case 6
                Select Case v2
                    Case 0 : Return ReadDst("ror", bd, pos)
                    Case 1 : Return ReadDst("rol", bd, pos)
                    Case 2 : Return ReadDst("asr", bd, pos)
                    Case 3 : Return ReadDst("asl", bd, pos)
                    Case 4 : Return ReadNum("mark", bd, pos)
                    Case 5 : Return ReadDst("mfpi", bd, pos)
                    Case 6 : Return ReadDst("mtpi", bd, pos)
                    Case 7 : Return ReadDst("sxt", bd, pos)
                End Select
        End Select
        Return Nothing
    End Function

    Private Function Read7(bd As BinData, pos%) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Select Case (v >> 9) And 7
            Case 0 : Return ReadSrcReg("mul", bd, pos)
            Case 1 : Return ReadSrcReg("div", bd, pos)
            Case 2 : Return ReadSrcReg("ash", bd, pos)
            Case 3 : Return ReadSrcReg("ashc", bd, pos)
            Case 4 : Return ReadRegDst("xor", bd, pos)
            Case 5
                Select Case (v >> 3) And &O77
                    Case 0 : Return ReadReg("fadd", bd, pos)
                    Case 1 : Return ReadReg("fsub", bd, pos)
                    Case 2 : Return ReadReg("fmul", bd, pos)
                    Case 3 : Return ReadReg("fdiv", bd, pos)
                End Select
            Case 7 : Return ReadRegOffset("sob", bd, pos)
        End Select
        Return Nothing
    End Function

    Private Function Read10(bd As BinData, pos%) As OpCode
        Select Case bd(pos + 1)
            Case &H80 : Return ReadOffset("bpl", bd, pos)
            Case &H81 : Return ReadOffset("bmi", bd, pos)
            Case &H82 : Return ReadOffset("bhi", bd, pos)
            Case &H83 : Return ReadOffset("blos", bd, pos)
            Case &H84 : Return ReadOffset("bvc", bd, pos)
            Case &H85 : Return ReadOffset("bvs", bd, pos)
            Case &H86 : Return ReadOffset("bcc", bd, pos)
            Case &H87 : Return ReadOffset("bcs", bd, pos)
            Case &H88 : Return New OpCode("emt " + bd.Enc(bd(pos)), 2)
            Case &H89 : Return New OpCode("sys " + bd.Enc(bd(pos)), 2)
        End Select
        Dim v = bd.ReadUInt16(pos)
        Select Case (v >> 6) And &O77
            Case &O50 : Return ReadDst("clrb", bd, pos)
            Case &O51 : Return ReadDst("comb", bd, pos)
            Case &O52 : Return ReadDst("incb", bd, pos)
            Case &O53 : Return ReadDst("decb", bd, pos)
            Case &O54 : Return ReadDst("negb", bd, pos)
            Case &O55 : Return ReadDst("adcb", bd, pos)
            Case &O56 : Return ReadDst("sbcb", bd, pos)
            Case &O57 : Return ReadDst("tstb", bd, pos)
            Case &O60 : Return ReadDst("rorb", bd, pos)
            Case &O61 : Return ReadDst("rolb", bd, pos)
            Case &O62 : Return ReadDst("asrb", bd, pos)
            Case &O63 : Return ReadDst("aslb", bd, pos)
            Case &O64 : Return ReadDst("mfpd", bd, pos)
            Case &O65 : Return ReadDst("mtpd", bd, pos)
        End Select
        Return Nothing
    End Function

    Private Function Read17(bd As BinData, pos%) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Select Case v And &HFFF
            Case 1 : Return New OpCode("setf", 2)
            Case 2 : Return New OpCode("seti", 2)
            Case &O11 : Return New OpCode("setd", 2)
            Case &O12 : Return New OpCode("setl", 2)
        End Select
        Return Nothing
    End Function

    Private Function ReadSrcDst(op$, bd As BinData, pos%) As OpCode
        Dim len = 2
        Dim v = bd.ReadUInt16(pos)
        Dim src = New Operand((v >> 9) And 7, (v >> 6) And 7, bd, pos + len)
        Return ReadDst(op + " " + src.ToString(bd) + ",", bd, pos, len + src.Length)
    End Function

    Private Function ReadDst(op$, bd As BinData, pos%, Optional len% = 2) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Dim dst = New Operand((v >> 3) And 7, v And 7, bd, pos + len)
        Return New OpCode(op + " " + dst.ToString(bd), len + dst.Length)
    End Function

    Private Function ReadRegDst(op$, bd As BinData, pos%) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Dim r = RegNames((v >> 6) And 7)
        Return ReadDst(op + " " + r + ",", bd, pos)
    End Function

    Private Function ReadSrcReg(op$, bd As BinData, pos%) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Dim r = RegNames((v >> 6) And 7)
        Dim src = New Operand((v >> 3) And 7, v And 7, bd, pos + 2)
        Return New OpCode(op + " " + src.ToString(bd) + ", " + r, 2 + src.Length)
    End Function

    Private Function ReadNum(op$, bd As BinData, pos%) As OpCode
        Return New OpCode(op + " " + bd.Enc(CByte(bd(pos) And &O77)), 2)
    End Function

    Private Function ReadRegOffset(op$, bd As BinData, pos%) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Dim r = RegNames((v >> 6) And 7)
        Return New OpCode(op + " " + r + ", " + bd.Enc(CUShort(pos + 2 - (v And &O77) * 2)), 2)
    End Function

    Private Function ReadOffset(op$, bd As BinData, pos%) As OpCode
        Return New OpCode(op + " " + bd.Enc(bd.GetOffset(pos)), 2)
    End Function

    Private Function ReadReg(op$, bd As BinData, pos%) As OpCode
        Dim r = RegNames(bd(pos) And 7)
        Return New OpCode(op + " " + r, 2)
    End Function

    Public Function ConvShort(v As UShort) As Short
        Return CShort(If(v < &H8000, v, v - &H10000))
    End Function

    Public Function ConvSByte(v As Byte) As SByte
        Return CSByte(If(v < &H80, v, v - &H100))
    End Function
End Module
