Imports System.IO
Imports PDP11Lib

Public Class SLFileSystem
    Inherits FileSystem

    Private root$

    Public Sub New(root$)
        Me.root = root
    End Sub

    Private files As New Dictionary(Of String, Byte())

    Protected Overrides Function GetStream(p$) As Stream
        If files.ContainsKey(p) Then
            Dim data = files(p)
            If data Is Nothing Then Return Nothing
            Dim ms = New MemoryStream
            ms.Write(data, 0, data.Length)
            ms.Position = 0
            Return ms
        Else
            Dim pp = root + "/" + If(p.StartsWith("/"), p.Substring(1), p)
            Dim rs = Application.GetResourceStream(New Uri(pp, UriKind.Relative))
            Return If(rs IsNot Nothing, rs.Stream, Nothing)
        End If
    End Function

    Protected Overrides Function CreateStream(p$) As Stream
        files(p) = New Byte() {}
        Return New MemoryStream()
    End Function

    Protected Overrides Sub CloseStream(p$, s As Stream)
        Dim ms = TryCast(s, MemoryStream)
        If ms IsNot Nothing AndAlso files(p) IsNot Nothing Then
            files(p) = ms.ToArray
        End If
    End Sub

    Public Overrides Function Delete(p$) As Boolean
        Dim s = GetStream(p)
        If s Is Nothing Then Return False
        s.Dispose()
        files(p) = Nothing
        Return True
    End Function

    Public Overrides Function Link(src$, dst$) As Boolean
        If Not files.ContainsKey(src) Then
            Dim data = GetAllBytes(src)
            If data Is Nothing Then Return False
            files.Add(src, data)
        End If
        files(dst) = files(src)
        Return True
    End Function

    Public Function GetFiles() As String()
        Dim keys = (From f In files.Keys Where files(f) IsNot Nothing Select f).ToArray
        Array.Sort(keys)
        Return keys
    End Function

    Public Function GetLength%(p$)
        If files.ContainsKey(p) Then Return files(p).Length
        Dim s = GetStream(p)
        If s Is Nothing Then Return -1
        Dim ret = CInt(s.Length)
        s.Dispose()
        Return ret
    End Function

    Public Function GetAllBytes(p$) As Byte()
        If files.ContainsKey(p) Then Return files(p)
        Dim s = GetStream(p)
        If s Is Nothing Then Return Nothing
        Dim ret(CInt(s.Length - 1)) As Byte
        s.Read(ret, 0, ret.Length)
        s.Dispose()
        Return ret
    End Function
End Class
