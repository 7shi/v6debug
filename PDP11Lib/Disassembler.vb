Imports System.Text

Public Module Disassembler
    Public ReadOnly RegNames As String() =
        {"r0", "r1", "r2", "r3", "r4", "r5", "sp", "pc"}

    Public ReadOnly SectionNames As String() = {Nothing, ".text", ".data", ".bss"}

    Public ReadOnly SysNames As String() =
        {"indir", "exit", "fork", "read", "write", "open", "close", "wait",
         "creat", "link", "unlink", "exec", "chdir", "time", "mknod", "chmod",
         "chown", "break", "stat", "seek", "getpid", "mount", "umount", "setuid",
         "getuid", "stime", "ptrace", Nothing, "fstat", Nothing, "smdate", "stty",
         "gtty", Nothing, "nice", "sleep", "sync", "kill", "switch", Nothing,
         Nothing, "dup", "pipe", "times", "prof", "tiu", "setgid", "getgid", "signal"}

    Public ReadOnly SigNames As String() =
        {Nothing, "SIGHUP", "SIGINT", "SIGQIT", "SIGINS", "SIGTRC", "SIGIOT", "SIGEMT",
         "SIGFPT", "SIGKIL", "SIGBUS", "SIGSEG", "SIGSYS", "SIGPIPE"}

    Public ReadOnly SysArgs As Integer() =
        {1, 0, 0, 2, 2, 2, 0, 0, 2, 2, 1, 2, 1, 0, 3, 2,
         2, 1, 2, 2, 0, 3, 1, 0, 0, 0, 3, 0, 1, 0, 1, 1,
         1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 4, 0, 0, 0, 2}

    Public Function Disassemble(data As Byte(), oct As Boolean) As OpCode
        If data.Length < 6 Then Array.Resize(data, 6)
        Dim bd = New BinData(data) With {.UseOct = oct}
        Return Disassemble(bd, 0)
    End Function

    Public Function Disassemble(bd As BinData, pos%) As OpCode
        Dim vm = TryCast(bd, VM)
        Dim st = If(vm IsNot Nothing, New VMState(vm), Nothing)
        Dim ret = _Disassemble(bd, pos)
        If st IsNot Nothing Then st.Restore()
        Return ret
    End Function

    Private Function _Disassemble(bd As BinData, pos%) As OpCode
        Select Case bd(pos + 1) >> 4
            Case 0 : Return Read0(bd, pos)
            Case 1 : Return ReadSrcDst("mov", bd, pos, 2)
            Case 2 : Return ReadSrcDst("cmp", bd, pos, 2)
            Case 3 : Return ReadSrcDst("bit", bd, pos, 2)
            Case 4 : Return ReadSrcDst("bic", bd, pos, 2)
            Case 5 : Return ReadSrcDst("bis", bd, pos, 2)
            Case 6 : Return ReadSrcDst("add", bd, pos, 2)
            Case 7 : Return Read7(bd, pos)
            Case &O10 : Return Read10(bd, pos)
            Case &O11 : Return ReadSrcDst("movb", bd, pos, 1)
            Case &O12 : Return ReadSrcDst("cmpb", bd, pos, 1)
            Case &O13 : Return ReadSrcDst("bitb", bd, pos, 1)
            Case &O14 : Return ReadSrcDst("bicb", bd, pos, 1)
            Case &O15 : Return ReadSrcDst("bisb", bd, pos, 1)
            Case &O16 : Return ReadSrcDst("sub", bd, pos, 2)
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
        Dim v1 = (v >> 9) And 7, v2 = (v >> 6) And 7
        Select Case v1
            Case 0 ' 00 0x xx
                Select Case v2
                    Case 0 ' 00 00 xx
                        Select Case v And &O77
                            Case 0 : Return New OpCode("halt", 2)
                            Case 1 : Return New OpCode("wait", 2)
                            Case 2 : Return New OpCode("rti", 2)
                            Case 3 : Return New OpCode("bpt", 2)
                            Case 4 : Return New OpCode("iot", 2)
                            Case 5 : Return New OpCode("reset", 2)
                            Case 6 : Return New OpCode("rtt", 2)
                        End Select
                    Case 1 : Return ReadDst("jmp", bd, pos, 2)
                    Case 2 ' 00 02 xx
                        Select Case (v >> 3) And 7
                            Case 0 : Return ReadReg("rts", bd, pos)
                            Case 3 : Return New OpCode("spl " + (v & 7), 2)
                            Case 4 - 7 ' 00 02 4x - 00 02 7x
                                Dim mne As String
                                Select Case v
                                    'Case &O260 : mne = "nop"
                                    Case &O240 : mne = "nop"
                                    Case &O257 : mne = "ccc"
                                    Case &O277 : mne = "scc"
                                    Case Else
                                        mne = If((v And 16) <> 0, "se", "cl") +
                                            If((v And 8) <> 0, "n", "") +
                                            If((v And 4) <> 0, "z", "") +
                                            If((v And 2) <> 0, "v", "") +
                                            If((v And 1) <> 0, "c", "")
                                End Select
                                Return New OpCode(mne, 2)
                        End Select
                    Case 3 : Return ReadDst("swab", bd, pos, 2)
                End Select
            Case 4 : Return ReadRegDst("jsr", bd, pos, 2)
            Case 5
                Select Case v2
                    Case 0 : Return ReadDst("clr", bd, pos, 2)
                    Case 1 : Return ReadDst("com", bd, pos, 2)
                    Case 2 : Return ReadDst("inc", bd, pos, 2)
                    Case 3 : Return ReadDst("dec", bd, pos, 2)
                    Case 4 : Return ReadDst("neg", bd, pos, 2)
                    Case 5 : Return ReadDst("adc", bd, pos, 2)
                    Case 6 : Return ReadDst("sbc", bd, pos, 2)
                    Case 7 : Return ReadDst("tst", bd, pos, 2)
                End Select
            Case 6
                Select Case v2
                    Case 0 : Return ReadDst("ror", bd, pos, 2)
                    Case 1 : Return ReadDst("rol", bd, pos, 2)
                    Case 2 : Return ReadDst("asr", bd, pos, 2)
                    Case 3 : Return ReadDst("asl", bd, pos, 2)
                    Case 4 : Return ReadNum("mark", bd, pos)
                    Case 5 : Return ReadDst("mfpi", bd, pos, 2)
                    Case 6 : Return ReadDst("mtpi", bd, pos, 2)
                    Case 7 : Return ReadDst("sxt", bd, pos, 2)
                End Select
        End Select
        Return Nothing
    End Function

    Private Function Read7(bd As BinData, pos%) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Select Case (v >> 9) And 7
            Case 0 : Return ReadSrcReg("mul", bd, pos, 2)
            Case 1 : Return ReadSrcReg("div", bd, pos, 2)
            Case 2 : Return ReadSrcReg("ash", bd, pos, 2)
            Case 3 : Return ReadSrcReg("ashc", bd, pos, 2)
            Case 4 : Return ReadRegDst("xor", bd, pos, 2)
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
            Case &H89
                Dim arg = bd(pos)
                If arg < SysNames.Length Then
                    Dim n = SysNames(arg)
                    If n IsNot Nothing Then
                        Dim sb = New StringBuilder("sys " + n)
                        If arg = 0 Then ' indir
                            Dim ad = bd.ReadUInt16(pos + 2)
                            Dim argad = bd.EncAddr(ad)
                            Dim p = argad.IndexOf("{")
                            If p > 0 Then argad = argad.Substring(0, p)
                            Dim op = Disassemble(bd, ad)
                            sb.Append("; " + argad + "{" + op.Mnemonic + "}")
                        Else
                            Dim first = 1
                            If arg = 48 Then ' signal
                                Dim sig = bd.ReadUInt16(pos + 2)
                                If sig < SigNames.Length AndAlso SigNames(sig) IsNot Nothing Then
                                    sb.Append("; " + SigNames(sig))
                                    first = 2
                                End If
                            End If
                            Dim argc = SysArgs(arg)
                            For i = first To argc
                                sb.Append("; " + bd.Enc(bd.ReadUInt16(pos + i * 2)))
                            Next
                        End If
                        Return New OpCode(sb.ToString, 2 + SysArgs(arg) * 2)
                    End If
                End If
                Return New OpCode("sys " & arg, 2)
        End Select
        Dim v = bd.ReadUInt16(pos)
        Select Case (v >> 6) And &O77
            Case &O50 : Return ReadDst("clrb", bd, pos, 1)
            Case &O51 : Return ReadDst("comb", bd, pos, 1)
            Case &O52 : Return ReadDst("incb", bd, pos, 1)
            Case &O53 : Return ReadDst("decb", bd, pos, 1)
            Case &O54 : Return ReadDst("negb", bd, pos, 1)
            Case &O55 : Return ReadDst("adcb", bd, pos, 1)
            Case &O56 : Return ReadDst("sbcb", bd, pos, 1)
            Case &O57 : Return ReadDst("tstb", bd, pos, 1)
            Case &O60 : Return ReadDst("rorb", bd, pos, 1)
            Case &O61 : Return ReadDst("rolb", bd, pos, 1)
            Case &O62 : Return ReadDst("asrb", bd, pos, 1)
            Case &O63 : Return ReadDst("aslb", bd, pos, 1)
            Case &O64 : Return ReadDst("mfpd", bd, pos, 1)
            Case &O65 : Return ReadDst("mtpd", bd, pos, 1)
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

    Private Function ReadSrcDst(op$, bd As BinData, pos%, size As UShort) As OpCode
        Dim len = 2
        Dim v = bd.ReadUInt16(pos)
        Dim src = New Operand((v >> 9) And 7, (v >> 6) And 7, bd, pos + len, size)
        Return ReadDst(op + " " + src.ToString(bd) + ",", bd, pos, size, len + src.Length)
    End Function

    Private Function ReadDst(op$, bd As BinData, pos%, size As UShort, Optional len% = 2) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Dim dst = New Operand((v >> 3) And 7, v And 7, bd, pos + len, size)
        Return New OpCode(op + " " + dst.ToString(bd), len + dst.Length)
    End Function

    Private Function ReadRegDst(op$, bd As BinData, pos%, size As UShort) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Dim r = GetRegString(bd, (v >> 6) And 7)
        Return ReadDst(op + " " + r + ",", bd, pos, size)
    End Function

    Private Function ReadSrcReg(op$, bd As BinData, pos%, size As UShort) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Dim r = GetRegString(bd, (v >> 6) And 7)
        Dim src = New Operand((v >> 3) And 7, v And 7, bd, pos + 2, size)
        Return New OpCode(op + " " + src.ToString(bd) + ", " + r, 2 + src.Length)
    End Function

    Private Function ReadNum(op$, bd As BinData, pos%) As OpCode
        Return New OpCode(op + " " + bd.Enc(CByte(bd(pos) And &O77)), 2)
    End Function

    Private Function ReadRegOffset(op$, bd As BinData, pos%) As OpCode
        Dim v = bd.ReadUInt16(pos)
        Dim r = GetRegString(bd, (v >> 6) And 7)
        Return New OpCode(op + " " + r + ", " + bd.Enc(CUShort(pos + 2 - (v And &O77) * 2)), 2)
    End Function

    Private Function ReadOffset(op$, bd As BinData, pos%) As OpCode
        Return New OpCode(op + " " + bd.Enc(bd.GetOffset(pos)), 2)
    End Function

    Private Function ReadReg(op$, bd As BinData, pos%) As OpCode
        Dim r = GetRegString(bd, bd(pos) And 7)
        Return New OpCode(op + " " + r, 2)
    End Function

    Public Function ConvShort(v As UShort) As Short
        Return CShort(If(v < &H8000, v, v - &H10000))
    End Function

    Public Function ConvSByte(v As Byte) As SByte
        Return CSByte(If(v < &H80, v, v - &H100))
    End Function

    Public Function GetRegString$(bd As BinData, r%)
        Return RegNames(r) + bd.GetReg(r)
    End Function
End Module
