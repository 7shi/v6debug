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
            Return If(data Is Nothing, Nothing, New MemoryStream(data))
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
        If ms IsNot Nothing Then
            files(p) = ms.ToArray
        End If
    End Sub
End Class
