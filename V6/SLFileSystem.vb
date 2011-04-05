Imports System.IO
Imports PDP11Lib

Public Class SLFileSystem
    Inherits FileSystem

    Private root$

    Public Sub New(root$)
        Me.root = root
    End Sub

    Protected Overrides Function GetStream(p$) As Stream
        Dim uri = New Uri(root + "/" + p, UriKind.Relative)
        Dim rs = Application.GetResourceStream(uri)
        Return If(rs IsNot Nothing, rs.Stream, Nothing)
    End Function

    Public Function Exists(p$) As Boolean
        Dim uri = New Uri(root + "/" + p, UriKind.Relative)
        Dim rs = Application.GetResourceStream(uri)
        If rs Is Nothing Then Return False
        rs.Stream.Dispose()
        Return True
    End Function
End Class
