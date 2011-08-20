Imports System.IO
Imports System.Text

Public Module Utils
    Public Function ConvShort(v As UShort) As Short
        Return CShort(If(v < &H8000, v, v - &H10000))
    End Function

    Public Function ConvSByte(v As Byte) As SByte
        Return CSByte(If(v < &H80, v, v - &H100))
    End Function

    Public Function GetRegString$(bd As BinData, r%, pc%)
        Return RegNames(r) + bd.GetReg(r, CUShort(pc And &HFFFF))
    End Function

    Public Function ReadText$(s As Stream)
        Dim list = New List(Of Byte)
        Dim prev = 0
        Do
            Dim b = s.ReadByte
            If b <= 0 Then
                Exit Do
            ElseIf b = 13 OrElse (b = 10 AndAlso prev <> 13) Then
                list.Add(13)
                list.Add(10)
            ElseIf b <> 10 Then
                list.Add(CByte(b))
            End If
            prev = b
        Loop
        Dim bytes = list.ToArray
        Return Encoding.UTF8.GetString(bytes, 0, bytes.Length)
    End Function

    Public Function ReadString$(src As Byte(), start%, Optional length% = -1)
        Dim list = New List(Of Byte)
        Dim len = 0
        Do
            If length >= 0 AndAlso len >= length Then Exit Do
            Dim b = src(start + len)
            If b = 0 Then Exit Do
            len += 1
        Loop
        Return Encoding.UTF8.GetString(src, start, len)
    End Function

    Public Function Escape$(s$)
        Dim sb = New StringBuilder
        For Each c In s
            Dim ch = AscW(c)
            Select Case ch
                Case 7 : sb.Append("\a")
                Case 8 : sb.Append("\b")
                Case 9 : sb.Append("\t")
                Case 10 : sb.Append("\n")
                Case 11 : sb.Append("\v")
                Case 12 : sb.Append("\f")
                Case 13 : sb.Append("\r")
                Case 34 : sb.Append("\""")
                Case 92 : sb.Append("\\")
                Case Else
                    If ch < 32 Then
                        sb.Append("\" + Convert.ToString(ch, 8))
                    Else
                        sb.Append(c)
                    End If
            End Select
        Next
        Return sb.ToString
    End Function

    Public Function Oct$(o As UInteger, c%)
        Dim ret = Convert.ToString(o, 8)
        If ret.Length < c Then
            ret = New String(CChar("0"), c - ret.Length) + ret
        End If
        Return ret
    End Function
End Module
