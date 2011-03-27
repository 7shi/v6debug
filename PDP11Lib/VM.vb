Imports System.IO

Partial Public Class VM
    Inherits BinData

    Public Regs(7) As UShort

    Public Property PC As UShort
        Get
            Return Regs(7)
        End Get
        Set(value As UShort)
            Regs(7) = value
        End Set
    End Property

    Public Property IsLong As Boolean
    Public Property IsDouble As Boolean
    Public Property HasExited As Boolean

    Public Property Z As Boolean
    Public Property N As Boolean
    Public Property C As Boolean
    Public Property V As Boolean

    Private sw As StringWriter
    Public ReadOnly Property Output$
        Get
            Return sw.ToString
        End Get
    End Property

    Public Sub New(aout As AOut)
        MyBase.New(&H10000)
        Array.Copy(aout.Data, aout.Offset, Data, 0, aout.Data.Length - aout.Offset)
        Me.UseOct = aout.UseOct
        Regs(6) = &HFFF0
    End Sub

    Public Sub Run()
        HasExited = False
        sw = New StringWriter
        While Not HasExited
            RunStep()
        End While
    End Sub

    Public Sub RunStep()
        Dim mne = Disassemble(PC).Mnemonic
        sw.WriteLine("{0}: {1}", GetRegs, mne)
        Select Case Me(PC + 1) >> 4
            'Case 3 : Return ReadSrcDst("bit")
            'Case 4 : Return ReadSrcDst("bic")
            'Case 5 : Return ReadSrcDst("bis")
            'Case &O12 : Return ReadSrcDst("cmpb")
            'Case &O13 : Return ReadSrcDst("bitb")
            'Case &O14 : Return ReadSrcDst("bicb")
            'Case &O15 : Return ReadSrcDst("bisb")
            Case 0
                Exec0()
                Return
            Case 1 ' mov: MOVe
                Dim oprs = GetSrcDst()
                Dim src = oprs(0).GetValue(Me)
                oprs(1).SetValue(Me, src)
                SetFlags(src = 0, ConvShort(src) < 0, C, False)
                Return
            Case 2 ' cmp: CoMPare
                Dim oprs = GetSrcDst()
                Dim src = oprs(0).GetValue(Me)
                Dim dst = oprs(1).GetValue(Me)
                Dim val = CInt(ConvShort(src)) - CInt(ConvShort(dst))
                SetFlags(val = 0, val < 0, src < dst, val < -&H8000)
                Return
            Case 6 ' add: ADD
                Dim oprs = GetSrcDst()
                Dim src = oprs(0).GetValue(Me)
                Dim dst = oprs(1).GetValue(Me)
                Dim val = CInt(ConvShort(src)) + CInt(ConvShort(dst))
                oprs(1).SetValue(Me, CUShort(val And &HFFFF))
                SetFlags(val = 0, val < 0, CInt(src) + CInt(dst) >= &H10000, val >= &H8000)
                Return
            Case &O10
                Exec10()
                Return
            Case &O11 ' movb: MOVe Byte
                Dim oprs = GetSrcDst()
                Dim src = oprs(0).GetByte(Me)
                oprs(1).SetByte(Me, src)
                SetFlags(src = 0, ConvSByte(src) < 0, C, False)
                Return
            Case &O16 ' sub: SUBtract
                Dim oprs = GetSrcDst()
                Dim src = oprs(0).GetValue(Me)
                Dim dst = oprs(1).GetValue(Me)
                Dim val = CInt(ConvShort(dst)) - CInt(ConvShort(src))
                oprs(1).SetValue(Me, CUShort(val And &HFFFF))
                SetFlags(val = 0, val < 0, dst < src, val < -&H8000)
                Return
            Case &O17
                Exec17()
                Return
        End Select
        Abort("not implemented")
    End Sub

    Private Sub Exec0()
        Select Case Me(PC + 1)
            Case 1 ' br: BRanch
                PC = GetOffset(PC)
                Return
            Case 2 ' bne: Branch if Not Equal
                PC = If(Not Z, GetOffset(PC), PC + 2US)
                Return
            Case 3 ' beq: Branch if EQual
                PC = If(Z, GetOffset(PC), PC + 2US)
                Return
            Case 4 ' bge: Branch if Greater or Equal
                PC = If(Not (N Xor Me.V), GetOffset(PC), PC + 2US)
                Return
            Case 5 ' blt: Branch if Less Than
                PC = If(N Xor Me.V, GetOffset(PC), PC + 2US)
                Return
            Case 6 ' bgt: Branch if Greater Than
                PC = If(Not (Z Or (N Xor Me.V)), GetOffset(PC), PC + 2US)
                Return
            Case 7 ' ble: Branch if Less or Equal
                PC = If(Z Or (N Xor Me.V), GetOffset(PC), PC + 2US)
                Return
        End Select
        Dim len = 2US
        Dim v = ReadUInt16(PC)
        If v = &HA0 Then PC += 2US : Return ' nop
        Dim v1 = (v >> 9) And 7, v2 = (v >> 6) And 7
        Select Case v1
            Case 0
                Select Case v2
                    'Case 0
                    '    Select Case v And &O77
                    '        Case 0 : Return New OpCode("halt", 2)
                    '        Case 1 : Return New OpCode("wait", 2)
                    '        Case 2 : Return New OpCode("rti", 2)
                    '        Case 3 : Return New OpCode("bpt", 2)
                    '        Case 4 : Return New OpCode("iot", 2)
                    '        Case 5 : Return New OpCode("reset", 2)
                    '        Case 6 : Return New OpCode("rtt", 2)
                    '    End Select
                    'Case 3 : Return ReadDst("swab", bd, pos)
                    Case 1 ' jmp: JuMP
                        PC = GetDst().GetAddress(Me)
                        Return
                    Case 2
                        Select Case (v >> 3) And 7
                            'Case 3 : Return New OpCode("spl " + (v & 7), 2)
                            Case 0 ' rts: ReTurn from Subroutine
                                Dim r = v And 7
                                PC = Regs(r)
                                Regs(r) = ReadUInt16(GetInc(6))
                                Return
                        End Select
                End Select
            Case 4 ' jsr: Jump to SubRoutine
                Dim r = (v >> 6) And 7
                Dim dst = GetDst().GetAddress(Me)
                Write(GetDec(6), Regs(r))
                Regs(r) = PC
                PC = dst
                Return
            Case 5
                Select Case v2
                    'Case 0 : Return ReadDst("clr", bd, pos)
                    'Case 1 : Return ReadDst("com", bd, pos)
                    'Case 2 : Return ReadDst("inc", bd, pos)
                    'Case 3 : Return ReadDst("dec", bd, pos)
                    'Case 4 : Return ReadDst("neg", bd, pos)
                    'Case 5 : Return ReadDst("adc", bd, pos)
                    'Case 6 : Return ReadDst("sbc", bd, pos)
                    Case 7 ' tst: TeST
                        Dim dst = ConvShort(GetDst().GetValue(Me))
                        SetFlags(dst = 0, dst < 0, False, False)
                        Return
                End Select
            Case 6
                'Select Case v2
                '    Case 0 : Return ReadDst("ror", bd, pos)
                '    Case 1 : Return ReadDst("rol", bd, pos)
                '    Case 2 : Return ReadDst("asr", bd, pos)
                '    Case 3 : Return ReadDst("asl", bd, pos)
                '    Case 4 : Return ReadNum("mark", bd, pos)
                '    Case 5 : Return ReadDst("mfpi", bd, pos)
                '    Case 6 : Return ReadDst("mtpi", bd, pos)
                '    Case 7 : Return ReadDst("sxt", bd, pos)
                'End Select
        End Select
        Abort("not implemented")
    End Sub

    Private Sub Exec10()
        Select Case Me(PC + 1)
            'Case &H88 : Return New OpCode("emt " + bd.Enc(bd(pos)), 2)
            Case &H80 ' bpl: Branch if PLus
                PC = If(Not N, GetOffset(PC), PC + 2US)
                Return
            Case &H81 ' bmi: Branch if MInus
                PC = If(N, GetOffset(PC), PC + 2US)
                Return
            Case &H82 ' bhi: Branch if HIgher
                PC = If(Not (C Or Z), GetOffset(PC), PC + 2US)
                Return
            Case &H83 ' blos: Branch if LOwer or Same
                PC = If(C Or Z, GetOffset(PC), PC + 2US)
                Return
            Case &H84 ' bvc: Branch if oVerflow Clear
                PC = If(Not V, GetOffset(PC), PC + 2US)
                Return
            Case &H85 ' bvs: Branch if oVerflow Set
                PC = If(V, GetOffset(PC), PC + 2US)
                Return
            Case &H86 ' bcc: Branch if Carry Clear
                PC = If(Not C, GetOffset(PC), PC + 2US)
                Return
            Case &H87 ' bcs: Branch if Carry Set
                PC = If(C, GetOffset(PC), PC + 2US)
                Return
            Case &H89 ' sys
                ExecSys()
                Return
        End Select
        'Dim v = ReadUInt16(PC)
        'Select Case (v >> 6) And &O77
        '    Case &O50 : Return ReadDst("clrb", bd, pos)
        '    Case &O51 : Return ReadDst("comb", bd, pos)
        '    Case &O52 : Return ReadDst("incb", bd, pos)
        '    Case &O53 : Return ReadDst("decb", bd, pos)
        '    Case &O54 : Return ReadDst("negb", bd, pos)
        '    Case &O55 : Return ReadDst("adcb", bd, pos)
        '    Case &O56 : Return ReadDst("sbcb", bd, pos)
        '    Case &O57 : Return ReadDst("tstb", bd, pos)
        '    Case &O60 : Return ReadDst("rorb", bd, pos)
        '    Case &O61 : Return ReadDst("rolb", bd, pos)
        '    Case &O62 : Return ReadDst("asrb", bd, pos)
        '    Case &O63 : Return ReadDst("aslb", bd, pos)
        '    Case &O64 : Return ReadDst("mfpd", bd, pos)
        '    Case &O65 : Return ReadDst("mtpd", bd, pos)
        'End Select
        Abort("not implemented")
    End Sub

    Private Sub Exec17()
        Dim v = ReadUInt16(PC)
        Select Case v And &HFFF
            Case 1 ' setf: SET Float
                PC += 2US
                IsDouble = False
                Return
            Case 2 ' seti: SET Integer
                PC += 2US
                IsLong = False
                Return
            Case &O11 ' setd: SET Double
                PC += 2US
                IsDouble = True
                Return
            Case &O12 ' setl: SET Long
                PC += 2US
                IsLong = True
                Return
        End Select
        Abort("not implemented")
    End Sub

    Private Function GetSrcDst() As Operand()
        Dim v = ReadUInt16(PC)
        Dim src = New Operand((v >> 9) And 7, (v >> 6) And 7, Me, PC + 2)
        Return New Operand() {src, GetDst(src.Length + 2US)}
    End Function

    Private Function GetDst(Optional len As UShort = 2) As Operand
        Dim v = ReadUInt16(PC)
        Dim dst = New Operand((v >> 3) And 7, v And 7, Me, PC + len)
        PC += len + dst.Length
        Return dst
    End Function

    Public Sub Abort(msg$)
        sw.WriteLine(msg)
        'sw.WriteLine(GetRegs)
        HasExited = True
    End Sub

    Public Function Disassemble(pos%) As OpCode
        Return Disassembler.Disassemble(Me, pos)
    End Function

    Public Function GetInc(r%) As UShort
        Dim ret = Regs(r)
        Regs(r) += 2US
        Return ret
    End Function

    Public Function GetDec(r%) As UShort
        Regs(r) -= 2US
        Return Regs(r)
    End Function

    Public Function GetRegs$()
        Return String.Format(
            "r0={0}, r1={1}, r2={2}, r3={3}, r4={4}, r5={5}, sp={6}{{{7}, {8}}}, pc={9}",
            Enc(Regs(0)), Enc(Regs(1)), Enc(Regs(2)), Enc(Regs(3)), Enc(Regs(4)), Enc(Regs(5)),
            Enc(Regs(6)), Enc(ReadUInt16(Regs(6))), Enc(ReadUInt16(Regs(6) + 2)), Enc(Regs(7)))
    End Function

    Public Sub SetFlags(z As Boolean, n As Boolean, c As Boolean, v As Boolean)
        Me.Z = z
        Me.N = n
        Me.C = c
        Me.V = v
    End Sub
End Class
