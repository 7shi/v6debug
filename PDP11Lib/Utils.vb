Imports System.IO
Imports System.Text

Public Module Utils
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

    Public Function ReadText$(src As Byte(), start%, Optional length% = -1)
        Dim list = New List(Of Byte)
        Dim prev = 0
        Dim p = start
        Dim ep = start + length
        Do
            If length >= 0 AndAlso p >= ep Then Exit Do
            Dim b = src(p)
            If b <= 0 Then
                Exit Do
            ElseIf b = 13 OrElse (b = 10 AndAlso prev <> 13) Then
                list.Add(13)
                list.Add(10)
            ElseIf b <> 10 Then
                list.Add(CByte(b))
            End If
            prev = b
            p += 1
        Loop
        Dim bytes = list.ToArray
        Return Encoding.UTF8.GetString(bytes, 0, bytes.Length)
    End Function
End Module
