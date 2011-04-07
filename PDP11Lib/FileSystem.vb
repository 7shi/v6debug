Imports System.IO

Public MustInherit Class FileSystem
    Friend Class FSObject
        Public Property Path$
        Public Property Count%
        Public Property Stream As Stream
    End Class

    Public Class FSStream
        Implements IDisposable

        Private _Handle%
        Private parent As FileSystem
        Friend target As FSObject

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

        Friend Sub New(fs As FileSystem, fso As FSObject, hnd%)
            parent = fs
            target = fso
            fso.Count += 1
            _Handle = hnd
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

    Private nextHandle%
    Private fsobjs As New Dictionary(Of String, FSObject)
    Private fshnds As New Dictionary(Of Integer, FSStream)

    Public Sub New()
        CloseAll()
    End Sub

    Protected MustOverride Function GetStream(p$) As Stream
    Protected MustOverride Sub Create(p$)

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
        Dim ret = New FSStream(Me, fso, nextHandle)
        nextHandle += 1
        fshnds.Add(ret.Handle, ret)
        Return ret
    End Function

    Friend Sub Close(fss As FSStream)
        fshnds.Remove(fss.Handle)
    End Sub

    Friend Sub Close(fso As FSObject)
        If fso.Stream IsNot Nothing Then
            fso.Stream.Dispose()
            fso.Stream = Nothing
        End If
        fsobjs.Remove(fso.Path)
    End Sub

    Public Sub CloseAll()
        Dim streams = fshnds.Values.ToArray
        For Each fss In streams
            fss.Dispose()
        Next
        nextHandle = 64
        Dim stdout = New FSObject With {.Path = "stdout:"}
        fsobjs.Add(stdout.Path, stdout)
        Dim sstdout = New FSStream(Me, stdout, 1)
        fshnds.Add(sstdout.Handle, sstdout)
    End Sub

    Public Function GetStream(handle%) As FSStream
        Return If(fshnds.ContainsKey(handle), fshnds(handle), Nothing)
    End Function

    Public Function Duplicate(fss As FSStream) As FSStream
        Dim ret = New FSStream(Me, fss.target, nextHandle)
        nextHandle += 1
        fshnds.Add(ret.Handle, ret)
        Return ret
    End Function

    Public Function Exists(p$) As Boolean
        Dim s = GetStream(p)
        If s Is Nothing Then Return False
        s.Dispose()
        Return True
    End Function
End Class
