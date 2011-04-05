Imports System.IO
Imports PDP11Lib

Public Class SLFileSystem
    Inherits FileSystem

    Private root$

    Public Sub New(root$)
        Me.root = root
    End Sub

    Private Function GetResourceStream(p$) As Stream
        Dim pp = root + "/" + If(p.StartsWith("/"), p.Substring(1), p)
        Dim rs = Application.GetResourceStream(New Uri(pp, UriKind.Relative))
        Return If(rs IsNot Nothing, rs.Stream, Nothing)
    End Function

    Protected Overrides Function GetStream(p$) As Stream
        Return GetResourceStream(p)
    End Function

    Public Function Exists(p$) As Boolean
        Dim s = GetResourceStream(p)
        If s Is Nothing Then Return False
        s.Dispose()
        Return True
    End Function
End Class
