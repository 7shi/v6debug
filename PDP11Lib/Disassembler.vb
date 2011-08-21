Imports System.IO

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

    Public ReadOnly OpCodes(65535) As OpCode

    Sub New()
        For i = 0 To 65535
            OpCodes(i) = New OpCode(i)
        Next
    End Sub

    Public Function Disassemble$(bd As BinData, pos%, Optional op As OpCode = Nothing)
        If op Is Nothing Then op = OpCodes(bd.ReadUInt16(pos))
        Dim vm = TryCast(bd, VM)
        Dim st = If(vm IsNot Nothing, New VMState(vm), Nothing)
        Dim ret = op.Disassemble(bd, pos)
        If st IsNot Nothing Then st.Restore()
        Return ret
    End Function
End Module

Public Class DisEntry
    Inherits DependencyObject

    Public Property Mark$
        Get
            Return CStr(GetValue(MarkProperty))
        End Get
        Set(value$)
            SetValue(Markproperty, value)
        End Set
    End Property

    Public Shared ReadOnly MarkProperty As DependencyProperty =
        DependencyProperty.Register("Mark", GetType(String), GetType(DisEntry), Nothing)

    Public Property Addr$
    Public Property Dump$
    Public Property Dis$
    Public Property Length%

    Public Sub New()
        Mark = ""
    End Sub

    Public Sub New(bd As BinData, i%, Optional maxlen% = 0)
        MyClass.New()
        Dim spclen = bd.Enc0(0US).Length
        Dim s = bd.ReadUInt16(i)
        Dim op = OpCodes(s)
        Dim dis = Disassemble(bd, i, op)
        Dim len = 2
        If dis IsNot Nothing Then
            If maxlen > 0 AndAlso op.Length > maxlen Then
                op = Nothing
                len = maxlen
            Else
                len = op.Length
            End If
        End If
        Addr = bd.Enc0(CUShort(i))
        Using sw = New StringWriter
            sw.Write(bd.Enc0(s))
            For j = 2 To len - 1 Step 2
                If j > 2 Then
                    sw.WriteLine()
                    sw.Write(Space(spclen))
                End If
                For k = 0 To 2 Step 2
                    If j + k < len - 1 Then sw.Write(" " + bd.Enc0(bd.ReadUInt16(i + j + k)))
                Next
            Next
            Dump = sw.ToString
        End Using
        If dis IsNot Nothing Then
            Me.Dis = dis
        Else
            Using sw = New StringWriter
                For j = 0 To len - 1 Step 2
                    If j > 0 Then sw.Write("; ")
                    sw.Write(bd.Enc(bd.ReadUInt16(i + j)))
                Next
            End Using
        End If
        Length = len
    End Sub
End Class
