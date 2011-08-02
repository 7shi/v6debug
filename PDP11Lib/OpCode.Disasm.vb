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

    Public ReadOnly OpCodes(65535) As OpCode

    Sub New()
        For i = 0 To 65535
            OpCodes(i) = New OpCode(i)
        Next
    End Sub
End Module

Partial Public Class OpCode

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
