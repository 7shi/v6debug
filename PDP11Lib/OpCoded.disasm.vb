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

    Public Function Disassemble$(bd As BinData, pos%, op As OpCode)
        Dim vm = TryCast(bd, VM)
        Dim st = If(vm IsNot Nothing, New VMState(vm), Nothing)
        Dim ret = op.Disassemble(bd, pos)
        If st IsNot Nothing Then st.Restore()
        Return ret
    End Function

    Public Function ConvShort(v As UShort) As Short
        Return CShort(If(v < &H8000, v, v - &H10000))
    End Function

    Public Function ConvSByte(v As Byte) As SByte
        Return CSByte(If(v < &H80, v, v - &H100))
    End Function

    Public Function GetRegString$(bd As BinData, r%, pc%)
        Return RegNames(r) + bd.GetReg(r, CUShort(pc And &HFFFF))
    End Function
End Module

Partial Public Class OpCode
    Private Sub Disassemble()
        Select Case val >> 12
            Case 0 : Read0()
            Case 1 : SetSrcDst("mov", 2)
            Case 2 : SetSrcDst("cmp", 2)
            Case 3 : SetSrcDst("bit", 2)
            Case 4 : SetSrcDst("bic", 2)
            Case 5 : SetSrcDst("bis", 2)
            Case 6 : SetSrcDst("add", 2)
            Case 7 : Read7()
            Case &O10 : Read10()
            Case &O11 : SetSrcDst("movb", 1)
            Case &O12 : SetSrcDst("cmpb", 1)
            Case &O13 : SetSrcDst("bitb", 1)
            Case &O14 : SetSrcDst("bicb", 1)
            Case &O15 : SetSrcDst("bisb", 1)
            Case &O16 : SetSrcDst("sub", 2)
            Case &O17 : Read17()
        End Select
    End Sub

    Private Sub Read0()
        Select Case val >> 8
            Case 1 : SetOffset("br")
            Case 2 : SetOffset("bne")
            Case 3 : SetOffset("beq")
            Case 4 : SetOffset("bge")
            Case 5 : SetOffset("blt")
            Case 6 : SetOffset("bgt")
            Case 7 : SetOffset("ble")
        End Select
        If Length <> 0 Then Return

        Dim v1 = (val >> 9) And 7, v2 = (val >> 6) And 7
        Select Case v1
            Case 0 ' 00 0x xx
                Select Case v2
                    Case 0 ' 00 00 xx
                        Select Case val And &O77
                            Case 0 : SetMne("halt")
                            Case 1 : SetMne("wait")
                            Case 2 : SetMne("rti")
                            Case 3 : SetMne("bpt")
                            Case 4 : SetMne("iot")
                            Case 5 : SetMne("reset")
                            Case 6 : SetMne("rtt")
                        End Select
                    Case 1 : SetDst("jmp", 2)
                    Case 2 ' 00 02 xx
                        Select Case (val >> 3) And 7
                            Case 0 : SetReg("rts")
                            Case 3 : SetMne("spl " + (val & 7))
                            Case 4 To 7 ' 00 02 4x - 00 02 7x
                                Dim mne As String
                                Select Case val
                                    'Case &O260 : mne = "nop"
                                    Case &O240 : mne = "nop"
                                    Case &O257 : mne = "ccc"
                                    Case &O277 : mne = "scc"
                                    Case Else
                                        mne = If((val And 16) <> 0, "se", "cl") +
                                            If((val And 8) <> 0, "n", "") +
                                            If((val And 4) <> 0, "z", "") +
                                            If((val And 2) <> 0, "v", "") +
                                            If((val And 1) <> 0, "c", "")
                                End Select
                                SetMne(mne)
                        End Select
                    Case 3 : SetDst("swab", 2)
                End Select
            Case 4 : SetRegDst("jsr", 2)
            Case 5
                Select Case v2
                    Case 0 : SetDst("clr", 2)
                    Case 1 : SetDst("com", 2)
                    Case 2 : SetDst("inc", 2)
                    Case 3 : SetDst("dec", 2)
                    Case 4 : SetDst("neg", 2)
                    Case 5 : SetDst("adc", 2)
                    Case 6 : SetDst("sbc", 2)
                    Case 7 : SetDst("tst", 2)
                End Select
            Case 6
                Select Case v2
                    Case 0 : SetDst("ror", 2)
                    Case 1 : SetDst("rol", 2)
                    Case 2 : SetDst("asr", 2)
                    Case 3 : SetDst("asl", 2)
                    Case 4 : SetNum("mark")
                    Case 5 : SetDst("mfpi", 2)
                    Case 6 : SetDst("mtpi", 2)
                    Case 7 : SetDst("sxt", 2)
                End Select
        End Select
    End Sub

    Private Sub Read7()
        Select Case (val >> 9) And 7
            Case 0 : SetSrcReg("mul", 2)
            Case 1 : SetSrcReg("div", 2)
            Case 2 : SetSrcReg("ash", 2)
            Case 3 : SetSrcReg("ashc", 2)
            Case 4 : SetRegDst("xor", 2)
            Case 5
                Select Case (val >> 3) And &O77
                    Case 0 : SetReg("fadd")
                    Case 1 : SetReg("fsub")
                    Case 2 : SetReg("fmul")
                    Case 3 : SetReg("fdiv")
                End Select
            Case 7 : SetRegOffset("sob")
        End Select
    End Sub

    Private Sub Read10()
        Select Case val >> 8
            Case &H80 : SetOffset("bpl")
            Case &H81 : SetOffset("bmi")
            Case &H82 : SetOffset("bhi")
            Case &H83 : SetOffset("blos")
            Case &H84 : SetOffset("bvc")
            Case &H85 : SetOffset("bvs")
            Case &H86 : SetOffset("bcc")
            Case &H87 : SetOffset("bcs")
            Case &H88 : disasm = Function(bd, pos) "emt " + bd.Enc(bd(pos)) : Length = 2
            Case &H89
                Dim arg = val And 255
                If arg < SysNames.Length AndAlso SysNames(arg) IsNot Nothing Then
                    disasm =
                        Function(bd, pos)
                            Dim sb = New StringBuilder("sys " + SysNames(arg))
                            If arg = 0 Then ' indir
                                Dim ad = bd.ReadUInt16(pos + 2)
                                Dim argad = bd.EncAddr(ad)
                                Dim p = argad.IndexOf("{")
                                If p > 0 Then argad = argad.Substring(0, p)
                                Dim op = New OpCode(bd.ReadUInt16(ad))
                                sb.Append("; " + argad + "{" + Disassembler.Disassemble(bd, ad, op) + "}")
                            ElseIf arg = 8 Or arg = 15 Then ' creat/chmod
                                sb.Append("; " + bd.EncAddr(bd.ReadUInt16(pos + 4)))
                                sb.Append("; " + "0" + Convert.ToString(bd.ReadUInt16(pos + 4), 8))
                            ElseIf arg = 48 Then ' signal
                                Dim sig = bd.ReadUInt16(pos + 2)
                                If sig < SigNames.Length AndAlso SigNames(sig) IsNot Nothing Then
                                    sb.Append("; " + SigNames(sig))
                                Else
                                    sb.Append("; " & sig)
                                End If
                                sb.Append("; " + bd.EncAddr(bd.ReadUInt16(pos + 4)))
                            Else
                                Dim argc = SysArgs(arg)
                                For i = 1 To argc
                                    sb.Append("; " + bd.Enc(bd.ReadUInt16(pos + i * 2)))
                                Next
                            End If
                            Return sb.ToString
                        End Function
                    Length = CUShort(2 + SysArgs(arg) * 2)
                Else
                    disasm = Function(bd, pos) "sys " & arg
                    Length = 2
                End If
        End Select
        If Length <> 0 Then Return

        Select Case (val >> 6) And &O77
            Case &O50 : SetDst("clrb", 1)
            Case &O51 : SetDst("comb", 1)
            Case &O52 : SetDst("incb", 1)
            Case &O53 : SetDst("decb", 1)
            Case &O54 : SetDst("negb", 1)
            Case &O55 : SetDst("adcb", 1)
            Case &O56 : SetDst("sbcb", 1)
            Case &O57 : SetDst("tstb", 1)
            Case &O60 : SetDst("rorb", 1)
            Case &O61 : SetDst("rolb", 1)
            Case &O62 : SetDst("asrb", 1)
            Case &O63 : SetDst("aslb", 1)
            Case &O64 : SetDst("mfpd", 1)
            Case &O65 : SetDst("mtpd", 1)
        End Select
    End Sub

    Private Sub Read17()
        Select Case val And &HFFF
            Case 1 : SetMne("setf")
            Case 2 : SetMne("seti")
            Case &O11 : SetMne("setd")
            Case &O12 : SetMne("setl")
        End Select
    End Sub

    Private Sub SetMne(op$)
        disasm = Function(bd, pos) op
        Length = 2
    End Sub

    Private Sub SetDst(op$, size As UShort)
        Dim dst = Operands(val And 63)
        If dst.IsValid Then
            disasm = Function(bd, pos) op + " " + dst.ToString(bd, pos + 2, size)
            Length = 2US + dst.Length
        End If
    End Sub

    Private Sub SetSrcDst(op$, size As UShort)
        Dim src = Operands((val >> 6) And 63)
        Dim dst = Operands(val And 63)
        If src.IsValid AndAlso dst.IsValid Then
            disasm = Function(bd, pos)
                         Return op + " " + src.ToString(bd, pos + 2, size) +
                             ", " + dst.ToString(bd, pos + 2 + src.Length, size)
                     End Function
            Length = 2US + src.Length + dst.Length
        End If
    End Sub

    Private Sub SetRegDst(op$, size As UShort)
        Dim reg = (val >> 6) And 7
        Dim dst = Operands(val And 63)
        If dst.IsValid Then
            disasm = Function(bd, pos)
                         Return op + " " + GetRegString(bd, reg, pos + 2) +
                             ", " + dst.ToString(bd, pos + 2, size)
                     End Function
            Length = 2US + dst.Length
        End If
    End Sub

    Private Sub SetSrcReg(op$, size As UShort)
        Dim reg = (val >> 6) And 7
        Dim src = Operands(val And 63)
        If src.IsValid Then
            disasm = Function(bd, pos)
                         Return op + " " + src.ToString(bd, pos + 2, size) +
                             "," + GetRegString(bd, reg, pos + 2)
                     End Function
            Length = 2US + src.Length
        End If
    End Sub

    Private Sub SetReg(op$)
        Dim reg = val And 7
        disasm = Function(bd, pos) op + " " + GetRegString(bd, reg, pos + 2)
        Length = 2
    End Sub

    Private Sub SetNum(op$)
        disasm = Function(bd, pos) op + " " + bd.Enc(CByte(bd(pos) And &O77))
        Length = 2
    End Sub

    Private Sub SetRegOffset(op$)
        Dim reg = (val >> 6) And 7
        disasm = Function(bd, pos)
                     Return op + " " + GetRegString(bd, reg, pos + 2) +
                         ", " + bd.Enc(CUShort(pos + 2 - (val And &O77) * 2))
                 End Function
        Length = 2
    End Sub

    Private Sub SetOffset(op$)
        disasm = Function(bd, pos) op + " " + bd.Enc(bd.GetOffset(pos))
        Length = 2
    End Sub
End Class
