Imports System.IO

Public MustInherit Class FileSystem
    Friend Class FSObject
        Public Property Path$
        Public Property Count%
        Public Property Stream As Stream
    End Class

    Public Class FSStream
        Implements IDisposable

        Private Shared handleCount% = 64
        Private _Handle%
        Private parent As FileSystem
        Private target As FSObject

        Public ReadOnly Property Stream As Stream
            Get
                Return target.Stream
            End Get
        End Property

        Public ReadOnly Property Handle%
            Get
                Return _Handle
            End Get
        End Property

        Friend Sub New(fs As FileSystem, fso As FSObject)
            parent = fs
            target = fso
            fso.Count += 1
            _Handle = handleCount
            handleCount += 1
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If target Is Nothing Then Return

            parent.Close(Me)
            target.Count -= 1
            If target.Count = 0 Then parent.Close(target)
            parent = Nothing
            target = Nothing
        End Sub
    End Class

    Private fsobjs As New Dictionary(Of String, FSObject)
    Private fshnds As New Dictionary(Of Integer, FSStream)

    Protected MustOverride Function GetStream(p$) As Stream

    Public Function Open(p$) As FSStream
        Dim fso As FSObject
        If fsobjs.ContainsKey(p) Then
            fso = fsobjs(p)
        Else
            Dim s = GetStream(p)
            If s Is Nothing Then Return Nothing
            fso = New FSObject With {.Path = p, .Stream = s}
            fsobjs.Add(p, fso)
        End If
        Dim ret = New FSStream(Me, fso)
        fshnds.Add(ret.Handle, ret)
        Return ret
    End Function

    Friend Sub Close(fss As FSStream)
        fshnds.Remove(fss.Handle)
    End Sub

    Friend Sub Close(fso As FSObject)
        fso.Stream.Dispose()
        fsobjs.Remove(fso.Path)
    End Sub
End Class
