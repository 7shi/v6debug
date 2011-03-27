Imports System.Text

Partial Public Class VM
    Private Sub ExecSys()
        Dim t = Me(PC)
        PC += 2US
        Select Case t
            Case 0 ' indirect
                Dim bak = PC + 2US
                PC = ReadUInt16(PC)
                ExecSys()
                PC = bak
                Return
            Case 1 ' exit
                HasExited = True
                Return
            Case 4 ' write
                SysWrite()
                Return
        End Select
        Abort("invaid sys")
    End Sub

    Private Sub SysWrite()
        Dim f = Regs(0)
        Dim p = ReadUInt16(PC)
        Dim len = ReadUInt16(PC + 2)
        PC += 4US
        If f = 1 Then
            sw.Write(ReadText(Data, p, len))
            C = False
        Else
            C = True
        End If
    End Sub
End Class
