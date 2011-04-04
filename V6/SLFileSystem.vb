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
        Return If(rs.Stream IsNot Nothing, rs.Stream, Nothing)
    End Function
End Class
