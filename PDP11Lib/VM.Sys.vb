Partial Public Class VM
    Private Sub ExecSys()
        Dim t = Data(PC)
        PC += 2US
        Select Case t
            Case 0
                Dim arg = ReadUInt16(PC)
                PC += 2US
                Select Case Data(arg)
                    Case 4 : SysWrite(arg) : Return
                End Select
        End Select
        Abort("invaid sys")
    End Sub

    Private Sub SysWrite(arg As UShort)
        Dim f = Regs(0)
        Dim p = ReadUInt16(arg + 2)
        Dim len = ReadUInt16(arg + 4)
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
