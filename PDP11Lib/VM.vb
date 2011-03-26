Imports System.IO

Public Class VM
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
    End Sub

    Public Sub Run()
        HasExited = False
        sw = New StringWriter
        While Not HasExited
            RunStep()
        End While
    End Sub

    Public Sub RunStep()
        Select Case Data(PC + 1) >> 4
            'Case 0 : Return Read0()
            'Case 2 : Return ReadSrcDst("cmp")
            'Case 3 : Return ReadSrcDst("bit")
            'Case 4 : Return ReadSrcDst("bic")
            'Case 5 : Return ReadSrcDst("bis")
            'Case 6 : Return ReadSrcDst("add")
            'Case &O10 : Return Read10()
            'Case &O11 : Return ReadSrcDst("movb")
            'Case &O12 : Return ReadSrcDst("cmpb")
            'Case &O13 : Return ReadSrcDst("bitb")
            'Case &O14 : Return ReadSrcDst("bicb")
            'Case &O15 : Return ReadSrcDst("bisb")
            'Case &O16 : Return ReadSrcDst("sub")
            Case 1 : ExecMov() : Return
            Case &O17 : Exec17() : Return
        End Select
        sw.WriteLine("not implemented: " + Disassemble(PC).Mnemonic)
        sw.WriteLine(GetRegs)
        HasExited = True
    End Sub

    Private Sub ExecMov()
        Dim len = 2
        Dim v = ReadUInt16(PC)
        Dim opr1 = New Operand((v >> 9) And 7, (v >> 6) And 7, Me, PC + len) : len += opr1.Length
        Dim opr2 = New Operand((v >> 3) And 7, v And 7, Me, PC + len) : len += opr2.Length
        PC += CUShort(len)
    End Sub

    Private Sub Exec17()
        Dim v = ReadUInt16(PC)
        PC += 2US
        Select Case v And &HFFF
            Case 1 : IsDouble = False   ' setf
            Case 2 : IsLong = False     ' seti
            Case &O11 : IsDouble = True ' setd
            Case &O12 : IsLong = True   ' setl
        End Select
    End Sub

    Public Function Disassemble(pos%) As OpCode
        Return Disassembler.Disassemble(Me, pos)
    End Function

    Public Function GetRegs$()
        Return String.Format(
            "r0={0}, r1={1}, r2={2}, r3={3}, r4={4}, r5={5}, sp={6}, pc={7}",
            Enc(Regs(0)), Enc(Regs(1)), Enc(Regs(2)), Enc(Regs(3)),
            Enc(Regs(4)), Enc(Regs(5)), Enc(Regs(6)), Enc(Regs(7)))
    End Function
End Class
