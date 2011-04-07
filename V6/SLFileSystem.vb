﻿Imports System.IO
Imports PDP11Lib

Public Class SLFileSystem
    Inherits FileSystem

    Private root$

    Public Sub New(root$)
        Me.root = root
    End Sub

    Protected Overrides Function GetStream(p$) As Stream
        Dim pp = root + "/" + If(p.StartsWith("/"), p.Substring(1), p)
        Dim rs = Application.GetResourceStream(New Uri(pp, UriKind.Relative))
        Return If(rs IsNot Nothing, rs.Stream, Nothing)
    End Function
End Class
