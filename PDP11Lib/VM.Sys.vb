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
                Case 0 ' indir: INDIRect
                    Dim bak = PC
                    PC = args(0)
                    ExecSys()
                    PC = bak
                    Return
                Case 1 ' exit
                    HasExited = True
                    Return
                Case 4 ' write
                    SysWrite(args)
                    Return
            End Select
        End If
        Abort("invalid sys")
    End Sub

    Private Sub SysWrite(args As UShort())
        Dim f = Regs(0)
        If f = 1 Then
            Dim t = ReadText(Data, args(0), args(1))
            swt.Write(t)
            swo.Write(t)
            C = False
        Else
            C = True
        End If
    End Sub
End Class
