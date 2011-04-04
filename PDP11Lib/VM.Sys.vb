Imports System.Text

Partial Public Class VM
    Private Sub ExecSys()
        Dim t = Me(PC)
        PC += 2US
        If t < SysNames.Length AndAlso SysNames(t) IsNot Nothing Then
            Dim args = New UShort() {}
            Dim argc = SysArgs(t)
            If argc > 0 Then
                ReDim args(argc - 1)
                For i = 0 To argc - 1
                    args(i) = ReadUInt16(PC)
                    PC += 2US
                Next
            End If
            Select Case t
                Case 0 : _indir(args) : Return
                Case 1 : _exit(args) : Return
                Case 4 : _write(args) : Return
                Case 5 : _open(args) : Return
            End Select
        End If
        Abort("invalid sys")
    End Sub

    Private Sub _indir(args As UShort())
        Dim bak = PC
        PC = args(0)
        ExecSys()
        PC = bak
    End Sub

    Private Sub _exit(args As UShort())
        HasExited = True
    End Sub

    Private Sub _write(args As UShort())
        Dim t = ReadString(Data, args(0), args(1))
        swt.WriteLine("sys write: r0={0}, {1}""{2}"", {3}",
                      Enc0(Regs(0)), Enc0(args(0)), Escape(t), args(1))
        Dim f = Regs(0)
        If f = 1 Then
            t = t.Replace(vbLf, vbCrLf)
            swt.Write(t)
            swo.Write(t)
            C = False
        Else
            C = True
        End If
    End Sub

    Private Sub _open(args As UShort())
        Dim p = ReadString(Data, args(0))
        swt.WriteLine("sys open: {0}""{1}"", {2}", Enc0(args(0)), Escape(p), args(1))
        Dim fss = fs.Open(p)
        If fss IsNot Nothing Then
            Regs(0) = CUShort(fss.Handle And &HFFFF)
            C = False
        Else
            Regs(0) = 0
            C = True
        End If
    End Sub
End Class
