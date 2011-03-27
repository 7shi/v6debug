Partial Public Class VM
    Private Sub ExecSys()
        Dim t = Data(PC)
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
            For i = 0 To len - 1
                Dim b = Data(p + i)
                If b = 10 Then
                    sw.WriteLine()
                Else
                    sw.Write(ChrW(b))
                End If
            Next
            C = False
        Else
            C = True
        End If
    End Sub
End Class
