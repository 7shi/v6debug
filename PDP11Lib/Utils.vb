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

    Public Function ReadText$(path$)
        Dim uri = New Uri(path, UriKind.Relative)
        Dim rs = Application.GetResourceStream(uri)
        If rs IsNot Nothing Then
            Using s = rs.Stream
                Return ReadText(s)
            End Using
        End If
        Return ""
    End Function

    Public Function ReadText$(src As Byte(), start%, length%)
        Dim list = New List(Of Byte)
        Dim prev = 0
        For i = 0 To length - 1
            Dim b = src(start + i)
            If b <= 0 Then
                Exit For
            ElseIf b = 13 OrElse (b = 10 AndAlso prev <> 13) Then
                list.Add(13)
                list.Add(10)
            ElseIf b <> 10 Then
                list.Add(CByte(b))
            End If
            prev = b
        Next
        Dim bytes = list.ToArray
        Return Encoding.UTF8.GetString(bytes, 0, bytes.Length)
    End Function
End Module
