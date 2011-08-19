Imports System.IO
Imports PDP11Lib

Public Class SLFileSystem
    Implements FileSystem

    Private root$
    Private fpos%(255), fname$(255)
    Private fdata(255) As Buffer

    Public Sub New(root$)
        Me.root = root
        CloseAll()
    End Sub

    Private Function getHandle%()
        For i = 3 To UBound(fpos)
            If fpos(i) < 0 Then Return i
        Next
        Return -1
    End Function

    Private deleted As New Buffer
    Private files As New Dictionary(Of String, Buffer)

    Private Function getBuffer(p$) As Buffer
        If files.ContainsKey(p) Then
            Dim ret = files(p)
            Return If(ret Is deleted, Nothing, ret)
        Else
            Dim pp = root + "/" + If(p.StartsWith("/"), p.Substring(1), p)
            Dim rs = Application.GetResourceStream(New Uri(pp, UriKind.Relative))
            If rs Is Nothing Then Return Nothing

            Using s = rs.Stream
                Dim ret = New Buffer
                ret.Length = CInt(s.Length)
                s.Read(ret.Data, 0, ret.Length)
#If NULLFS Then
                files(p) = ret
#End If
                Return ret
            End Using
        End If
    End Function

    Public Function Open%(p$) Implements FileSystem.Open%
        Dim buf = getBuffer(p)
        If buf Is Nothing Then Return -1

        Dim handle = getHandle()
        If handle < 0 Then Return -1

        fpos(handle) = 0
        fname(handle) = p
        fdata(handle) = buf
        Return handle
    End Function

    Public Function Create%(p$) Implements FileSystem.Create
        files(p) = New Buffer
        Return Open(p)
    End Function

    Public Function Close(h%) As Boolean Implements FileSystem.Close
        If fpos(h) < 0 Then Return False

        fpos(h) = -1
        fname(h) = Nothing
        fdata(h) = Nothing
        Return True
    End Function

    Public Sub CloseAll() Implements FileSystem.CloseAll
        Array.Clear(fpos, 0, 3)
        Array.Clear(fname, 0, fname.Length)
        Array.Clear(fdata, 0, fdata.Length)
        fname(0) = "stdin:"
        fname(1) = "stdout:"
        fname(2) = "stderr:"
        For i = 3 To UBound(fpos)
            fpos(i) = -1
        Next
    End Sub

    Public Function Read%(h%, data As Byte(), offset%, count%) Implements FileSystem.Read%
        Dim buf = fdata(h)
        Dim pos = fpos(h)
        Dim len = Math.Min(count, buf.Length - pos)
        Array.Copy(buf.Data, pos, data, offset, len)
        fpos(h) += len
        Return len
    End Function

    Public Sub Write(h%, data As Byte(), offset%, count%) Implements FileSystem.Write
        Dim buf = fdata(h)
        Dim last = fpos(h) + count
        If buf.Length < last Then buf.Length = last
        Array.Copy(data, offset, buf.Data, fpos(h), count)
        fpos(h) += count
    End Sub

    Public Sub Seek(h%, offset%, origin As SeekOrigin) Implements FileSystem.Seek
        If fpos(h) < 0 Then Exit Sub

        Select Case origin
            Case SeekOrigin.Begin
                fpos(h) = offset
            Case SeekOrigin.Current
                fpos(h) += offset
            Case SeekOrigin.End
                fpos(h) = fdata(h).Length + offset
        End Select
    End Sub

    Public Function Duplicate%(h%) Implements FileSystem.Duplicate%
        If fpos(h) < 0 Then Return -1

        Dim handle = getHandle()
        If handle < 0 Then Return -1

        fpos(handle) = fpos(h)
        fname(handle) = fname(h)
        fdata(handle) = fdata(h)
        Return handle
    End Function

    Public Function Delete(p$) As Boolean Implements FileSystem.Delete
        Dim buf = getBuffer(p)
        If buf Is Nothing Then Return False

        files(p) = deleted
        Return True
    End Function

    Public Function Link(src$, dst$) As Boolean Implements FileSystem.Link
        Dim buf = getBuffer(src)
        If buf Is Nothing Then Return False

        files(dst) = buf
        Return True
    End Function

    Public Function Exists(p$) As Boolean Implements FileSystem.Exists
        Return getBuffer(p) IsNot Nothing
    End Function

    Public Function GetLength%(p$) Implements FileSystem.GetLength
        Return getBuffer(p).Length
    End Function

    Public Function GetAllBytes(p$) As Byte() Implements FileSystem.GetAllBytes
        Return getBuffer(p).ToArray()
    End Function

    Public Function GetPath$(h%) Implements FileSystem.GetPath
        Return fname(h)
    End Function

    Public Function GetFiles() As String()
        Dim keys = (From f In files.Keys
                    Let buf = files(f)
                    Where buf IsNot Nothing AndAlso buf IsNot deleted
                    Select f).ToArray
        Array.Sort(keys)
        Return keys
    End Function

    Private Class Buffer
        Public Data(255) As Byte
        Private _length%

        Public Property Length%
            Get
                Return _length
            End Get

            Set(value%)
                Dim len = data.Length
                If len < value Then
                    While len < value
                        len += len
                    End While
                    ReDim Preserve data(len - 1)
                End If
                _length = value
            End Set
        End Property

        Public Function ToArray() As Byte()
            Dim ret(Length - 1) As Byte
            Array.Copy(data, ret, Length)
            Return ret
        End Function
    End Class
End Class
