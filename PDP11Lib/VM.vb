Imports System.IO
Imports System.Text

Partial Public Class VM
    Inherits BinData

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

    Private aout As AOut
    Private fs As FileSystem
    Private verbose As Boolean
    Private prevState As VMState
    Private callStack As New Stack(Of VMState)
    Private args$()

    Public Sub New(aout As AOut, fs As FileSystem, verbose As Boolean)
        MyBase.New(&H10000)
        Array.Copy(aout.Data, aout.Offset, Data, 0, aout.tsize + aout.dsize)
        Me.UseOct = aout.UseOct
        Me.aout = aout
        Me.fs = fs
        Me.verbose = verbose
        breakpt = aout.BreakPoint
        PC = aout.entry
        SetArgs(New String() {aout.Path})
    End Sub

    Public Sub SetArgs(args$())
        Me.args = args
        Dim p = &H10000
        Dim list = New List(Of Integer)
        For i = args.Length - 1 To 0 Step -1
            Dim bytes = Encoding.UTF8.GetBytes(args(i))
            Dim len = (bytes.Length \ 2) * 2 + 2
            p -= len
            Array.Copy(bytes, 0, Data, p, bytes.Length)
            Array.Clear(Data, p + bytes.Length, len - bytes.Length)
            list.Add(p)
        Next
        For Each arg In list
            p -= 2
            Write(p, CUShort(arg))
        Next
        p -= 2
        Write(p, CUShort(args.Length))
        Regs(6) = CUShort(p)
    End Sub

    Public Sub Run(args$())
        If args IsNot Nothing Then
            Dim args2$(args.Length)
            Array.Copy(args, 0, args2, 1, args.Length)
            args2(0) = aout.Path
            SetArgs(args2)
        Else
            SetArgs(New String() {aout.Path})
        End If
        Run()
    End Sub

    Public Sub Run()
        HasExited = False
        swt = New StringWriter
        swo = New StringWriter
        Dim cmdl = GetCommandLine()
        swt.WriteLine(cmdl)
        swo.WriteLine(cmdl)
        Dim cur As Symbol = Nothing
        Dim prevStack = callStack.Count
        While Not HasExited
            If verbose Then
                Dim sym = aout.GetSymbol(PC)
                If cur IsNot sym Then
                    swt.Write("     ")
                    If prevStack > callStack.Count Then
                        swt.WriteLine("<{0}", sym.Name)
                    ElseIf PC = sym.Address Then
                        swt.WriteLine("{0}", sym)
                    Else
                        swt.WriteLine(">{0}", sym.Name)
                    End If
                    cur = sym
                    prevStack = callStack.Count
                End If
            End If
            RunStep()
        End While
        fs.CloseAll()
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

    Private Function GetSrcDst(size As UShort) As Operand()
        Dim v = ReadUInt16(PC)
        Dim src = New Operand((v >> 9) And 7, (v >> 6) And 7, Me, PC + 2, size)
        Return New Operand() {src, GetDst(size, src.Length + 2US)}
    End Function

    Private Function GetDst(size As UShort, Optional len As UShort = 2) As Operand
        Dim v = ReadUInt16(PC)
        Dim dst = New Operand((v >> 3) And 7, v And 7, Me, PC + len, size)
        PC += len + dst.Length
        Return dst
    End Function

    Public Sub Abort(msg$)
        If Not verbose Then
            swt.WriteLine()
            WriteState(prevState, True)
        End If
        swt.WriteLine(msg)
        swt.WriteLine()
        swt.WriteLine("==== backtrace ====")
        For Each st In callStack
            WriteState(st, True)
        Next
        HasExited = True
    End Sub

    Private Sub WriteState(st As VMState, showSym As Boolean)
        Dim bak = PC
        PC = st.Regs(7)
        If showSym Then
            Dim sym = aout.GetSymbol(PC)
            If sym IsNot Nothing Then
                swt.WriteLine("in {0}", sym)
            End If
        End If
        Dim op = Disassemble(PC)
        swt.WriteLine("{0}: {1}", st, If(op IsNot Nothing, op.Mnemonic, Enc(ReadUInt16(PC))))
        PC = bak
    End Sub

    Public Function Disassemble(pos%) As OpCode
        Return Disassembler.Disassemble(Me, pos)
    End Function

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

    Public Overrides Function EncAddr(addr As UShort) As String
        Return aout.EncAddr(addr) + "{" + Enc0(ReadUInt16(addr)) + "}"
    End Function

    Public Overrides Function GetReg$(r%)
        Return "{" + Enc0(Regs(r)) + "}"
    End Function

    Public Overrides Function GetValue$(r%, size%, d1%, d2%)
        Dim ad = CUShort((Regs(r) + d1) And &HFFFF)
        Dim p = If(size = 2, Enc0(ReadUInt16(ad)), Enc0(Me(ad)))
        Regs(r) = CUShort((Regs(r) + d2) And &HFFFF)
        Return "{" + Enc0(ad) + ":" + p + "}"
    End Function

    Public Overrides Function GetPtr$(r%, size%, d1%, d2%)
        Dim ad = CUShort((Regs(r) + d1) And &HFFFF)
        Dim p = ReadUInt16(ad)
        Regs(r) = CUShort((Regs(r) + d2) And &HFFFF)
        Dim pp = If(size = 2, Enc0(ReadUInt16(p)), Enc0(Me(p)))
        Return "{" + Enc0(ad) + ":" + Enc0(p) + ":" + pp + "}"
    End Function

    Public Overrides Function GetRelative$(r%, d%, ad%)
        Return aout.GetRelative(r, d, ad)
    End Function

    Public Shared Function System(fs As FileSystem, cmd$, ParamArray args$()) As VM
        Dim data As Byte()
        Dim s = fs.Open(cmd)
        If s Is Nothing Then s = fs.Open("bin/" + cmd)
        ReDim data(CInt(s.Stream.Length - 1))
        s.Stream.Read(data, 0, data.Length)
        s.Dispose()

        Dim aout = New AOut(data, cmd)
        Dim vm = New VM(aout, fs, False)
        vm.Run(args)
        Return vm
    End Function
End Class
