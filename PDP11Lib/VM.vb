Imports System.IO
Imports System.Text

Partial Public Class VM
    Inherits BinData

    Private Shared nextPid% = 1
    Private pid%

    Public Regs(7) As UShort
    Private breakpt As UShort

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

    Private swt As StringWriter
    Private swo As StringWriter

    Public ReadOnly Property Trace$
        Get
            Return swt.ToString
        End Get
    End Property

    Public ReadOnly Property Output$
        Get
            Return swo.ToString
        End Get
    End Property

    Private verbose As Boolean
    Private prevState As VMState
    Private callStack As New Stack(Of VMState)
    Private args$()

    Public Sub New(verbose As Boolean)
        MyBase.New(&H10000)
        pid = nextPid
        nextPid += 1
        Me.verbose = verbose
    End Sub

    Public Sub Run()
        HasExited = False
        swt = New StringWriter
        swo = New StringWriter
        Dim cmdl = GetCommandLine()
        swt.WriteLine(cmdl)
        swo.WriteLine(cmdl)
        Dim prevStack = callStack.Count
        While Not HasExited
            RunStep()
            If (PC And 1) <> 0 Then Abort("invalid pc=" + Enc0(PC))
        End While
    End Sub

    Public Sub RunStep()
        While callStack.Count > 0 AndAlso Regs(6) > callStack.Peek.Regs(6) - 2
            callStack.Pop()
        End While
        prevState = New VMState(Me)
        If verbose Then WriteState(prevState)
        Dim op = OpCodes(ReadUInt16(GetInc(7, 2)))
        If Not op.Run(Me) Then Abort("not implemented")
    End Sub

    Public Function GetCommandLine$()
        Dim sb = New StringBuilder("#")
        For Each arg In args
            sb.Append(" ")
            Dim quo = arg.Contains(" ")
            If quo Then sb.Append("""")
            sb.Append(Escape(arg))
            If quo Then sb.Append("""")
        Next
        Return sb.ToString
    End Function

    Public Sub Abort(msg$)
        If Not verbose Then
            swt.WriteLine()
            WriteState(prevState)
        End If
        swt.WriteLine(msg)
        swt.WriteLine()
        swt.WriteLine("==== backtrace ====")
        For Each st In callStack
            WriteState(st)
        Next
        HasExited = True
    End Sub

    Private Sub WriteState(st As VMState)
        Dim bak = PC
        PC = st.Regs(7)
        Dim val = ReadUInt16(PC)
        swt.WriteLine("{0}: {1}", st, Disassembler.Disassemble(Me, PC))
        PC = bak
    End Sub

    Public Function GetInc(r%, size%) As UShort
        Dim ret = Regs(r)
        Regs(r) = CUShort((Regs(r) + size) And &HFFFF)
        Return ret
    End Function

    Public Function GetDec(r%, size%) As UShort
        Regs(r) = CUShort((Regs(r) - size) And &HFFFF)
        Return Regs(r)
    End Function

    Public Function GetReg32%(r%)
        Return (CInt(Regs(r)) << 16) Or CInt(Regs((r + 1) And 7))
    End Function

    Public Sub SetReg32(r%, v%)
        Regs(r) = CUShort((v >> 16) And &HFFFF)
        Regs((r + 1) And 7) = CUShort(v And &HFFFF)
    End Sub

    Public Sub SetFlags(z As Boolean, n As Boolean, c As Boolean, v As Boolean)
        Me.Z = z
        Me.N = n
        Me.C = c
        Me.V = v
    End Sub

    Public Sub PushStack()
        callStack.Push(New VMState(Me))
    End Sub
End Class
