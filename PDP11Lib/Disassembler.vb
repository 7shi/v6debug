Public Module Disassembler
    Public ReadOnly RegNames As String() =
        {"r0", "r1", "r2", "r3", "r4", "r5", "sp", "pc"}

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
                    Case 1 : Return ReadSrcOrDst("jmp", bd, pos)
                    Case 2
                        Select Case (v >> 3) And 7
                            Case 0 : Return ReadReg("rts", bd, pos)
                            Case 3 : Return New OpCode("spl " + (v & 7), 2)
                        End Select
                    Case 3 : Return ReadSrcOrDst("swab", bd, pos)
                End Select
            Case 4 : Return ReadRegSrcOrDst("jsr", bd, pos)
            Case 5
                Select Case v2
                    Case 0 : Return ReadSrcOrDst("clr", bd, pos)
                    Case 1 : Return ReadSrcOrDst("com", bd, pos)
                    Case 2 : Return ReadSrcOrDst("inc", bd, pos)
                    Case 3 : Return ReadSrcOrDst("dec", bd, pos)
                    Case 4 : Return ReadSrcOrDst("neg", bd, pos)
                    Case 5 : Return ReadSrcOrDst("adc", bd, pos)
                    Case 6 : Return ReadSrcOrDst("sbc", bd, pos)
                    Case 7 : Return ReadSrcOrDst("tst", bd, pos)
                End Select
            Case 6
                Select Case v2
                    Case 0 : Return ReadSrcOrDst("ror", bd, pos)
                    Case 1 : Return ReadSrcOrDst("rol", bd, pos)
                    Case 2 : Return ReadSrcOrDst("asr", bd, pos)
                    Case 3 : Return ReadSrcOrDst("asl", bd, pos)
                    Case 4 : Return ReadNum("mark", bd, pos)
                    Case 5 : Return ReadSrcOrDst("mfpi", bd, pos)
                    Case 6 : Return ReadSrcOrDst("mtpi", bd, pos)
                    Case 7 : Return ReadSrcOrDst("sxt", bd, pos)
                End Select
        End Select
        Return Nothing
    End Function

    Private Function Read7(bd As BinData, pos%) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Select Case (v >> 9) And 7
            Case 0 : Return ReadRegSrcOrDst("mul", bd, pos)
            Case 1 : Return ReadRegSrcOrDst("div", bd, pos)
            Case 2 : Return ReadRegSrcOrDst("ash", bd, pos)
            Case 3 : Return ReadRegSrcOrDst("ashc", bd, pos)
            Case 4 : Return ReadRegSrcOrDst("xor", bd, pos)
            Case 5
                Select Case (v >> 3) And &O77
                    Case 0 : Return ReadReg("fadd", bd, pos)
                    Case 1 : Return ReadReg("fsub", bd, pos)
                    Case 2 : Return ReadReg("fmul", bd, pos)
                    Case 3 : Return ReadReg("fdiv", bd, pos)
                End Select
            Case 7 : Return ReadRegNum("sob", bd, pos)
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
            Case &H89 : Return New OpCode("trap " + bd.Enc(bd(pos)), 2)
        End Select
        Dim v = bd.ReadUInt16(pos)
        Select Case (v >> 6) And &O77
            Case &O50 : Return ReadSrcOrDst("clrb", bd, pos)
            Case &O51 : Return ReadSrcOrDst("comb", bd, pos)
            Case &O52 : Return ReadSrcOrDst("incb", bd, pos)
            Case &O53 : Return ReadSrcOrDst("decb", bd, pos)
            Case &O54 : Return ReadSrcOrDst("negb", bd, pos)
            Case &O55 : Return ReadSrcOrDst("adcb", bd, pos)
            Case &O56 : Return ReadSrcOrDst("sbcb", bd, pos)
            Case &O57 : Return ReadSrcOrDst("tstb", bd, pos)
            Case &O60 : Return ReadSrcOrDst("rorb", bd, pos)
            Case &O61 : Return ReadSrcOrDst("rolb", bd, pos)
            Case &O62 : Return ReadSrcOrDst("asrb", bd, pos)
            Case &O63 : Return ReadSrcOrDst("aslb", bd, pos)
            Case &O64 : Return ReadSrcOrDst("mfpd", bd, pos)
            Case &O65 : Return ReadSrcOrDst("mtpd", bd, pos)
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
        Dim v1 = (v >> 9) And 7
        Dim v2 = (v >> 6) And 7
        Dim v3 = (v >> 3) And 7
        Dim v4 = v And 7
        Dim v5 = 0S, v6 = 0S
        If HasOperand(v1, v2) Then v5 = bd.ReadInt16(pos + len) : len += 2
        If HasOperand(v3, v4) Then v6 = bd.ReadInt16(pos + len) : len += 2
        Dim opr1 = GetOperand(bd, pos + len, v1, v2, v5)
        Dim opr2 = GetOperand(bd, pos + len, v3, v4, v6)
        Return New OpCode(op + " " + opr1 + ", " + opr2, len)
    End Function

    Private Function ReadSrcOrDst(op$, bd As BinData, pos%) As OpCode
        Dim len = 2
        Dim v = bd.ReadUInt16(pos)
        Dim v1 = (v >> 3) And 7
        Dim v2 = v And 7
        Dim v3 = 0
        If HasOperand(v1, v2) Then v3 = bd.ReadUInt16(pos + len) : len += 2
        Dim opr = GetOperand(bd, pos + len, v1, v2, CShort(v3))
        Return New OpCode(op + " " + opr, len)
    End Function

    Private Function ReadRegSrcOrDst(op$, bd As BinData, pos%) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Dim r = RegNames((v >> 6) And 7)
        Return ReadSrcOrDst(op + " " + r + ",", bd, pos)
    End Function

    Private Function ReadNum(op$, bd As BinData, pos%) As OpCode
        Return New OpCode(op + " " + bd.Enc(CByte(bd(pos) And &O77)), 2)
    End Function

    Private Function ReadRegNum(op$, bd As BinData, pos%) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Dim r = RegNames((v >> 6) And 7)
        Return ReadNum(op + " " + r + ",", bd, pos)
    End Function

    Private Function ReadOffset(op$, bd As BinData, pos%) As OpCode
        Dim d = CInt(bd(pos))
        If d >= 128 Then d -= 256
        Dim ad = pos + 2 + d * 2
        Return New OpCode(op + " " + bd.Enc(CUShort(ad)), 2)
    End Function

    Private Function ReadReg(op$, bd As BinData, pos%) As OpCode
        Dim r = RegNames(bd(pos) And 7)
        Return New OpCode(op + " " + r, 2)
    End Function

    Public Function HasOperand(v1%, v2%) As Boolean
        Return v1 >= 6 OrElse (v2 = 7 AndAlso (v1 = 2 OrElse v1 = 3))
    End Function

    Public Function GetOperand$(bd As BinData, pc%, v1%, v2%, v3 As Short)
        If v2 = 7 Then
            Select Case v1
                Case 2 : Return "$" + bd.Enc(CUShort(v3 And &HFFFF))
                Case 3 : Return "*$" + bd.Enc(CUShort(v3 And &HFFFF))
                Case 6 : Return bd.Enc(CUShort(pc + v3))
                Case 7 : Return "*" + bd.Enc(CUShort(pc + v3))
            End Select
        End If
        Dim r = RegNames(v2)
        Dim sign = If(v3 < 0, "-", "")
        Dim v3a = Math.Abs(v3)
        Dim dd = v3.ToString
        If v3a >= 10 Then dd = sign + bd.Enc(CUShort(v3a))
        Select Case v1
            Case 0 : Return r
            Case 1 : Return "(" + r + ")"
            Case 2 : Return "(" + r + ")+"
            Case 3 : Return "*(" + r + ")+"
            Case 4 : Return "-(" + r + ")"
            Case 5 : Return "*-(" + r + ")"
            Case 6 : Return dd + "(" + r + ")"
            Case 7 : Return "*" + dd + "(" + r + ")"
        End Select
        Throw New Exception("invalid argument")
    End Function
End Module
