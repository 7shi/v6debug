Imports System.IO
Imports System.Text

Partial Public Class VM
    Private Sub ExecSys()
        Dim t = Me(PC)
        PC += 2US
        If t < SysNames.Length AndAlso SysNames(t) IsNot Nothing Then
            Dim args = New UShort() {}
            Dim argc = SysArgs(t)
            If argc > 0 Then
                ReDim args(argc - 1)
                For i = 0 To argc - 1
                    args(i) = ReadUInt16(PC)
                    PC += 2US
                Next
            End If
            Select Case t
                'Case 2 : _fork(args) : Return
                'Case 7 : _wait(args) : Return
                'Case 9 : _link(args) : Return
                'Case 12 : _chdir(args) : Return
                'Case 13 : _time(args) : Return
                'Case 14 : _mknod(args) : Return
                'Case 16 : _chown(args) : Return
                'Case 21 : _mount(args) : Return
                'Case 22 : _umount(args) : Return
                'Case 23 : _setuid(args) : Return
                'Case 24 : _getuid(args) : Return
                'Case 25 : _stime(args) : Return
                'Case 26 : _ptrace(args) : Return
                'Case 28 : _fstat(args) : Return
                'Case 30 : _smdate(args) : Return
                'Case 31 : _stty(args) : Return
                'Case 32 : _gtty(args) : Return
                'Case 34 : _nice(args) : Return
                'Case 35 : _sleep(args) : Return
                'Case 36 : _sync(args) : Return
                'Case 37 : _kill(args) : Return
                'Case 38 : _switch(args) : Return
                'Case 42 : _pipe(args) : Return
                'Case 43 : _times(args) : Return
                'Case 44 : _prof(args) : Return
                'Case 45 : _tiu(args) : Return
                'Case 46 : _setgid(args) : Return
                'Case 47 : _getgid(args) : Return
                Case 0 : _indir(args) : Return
                Case 1 : _exit(args) : Return
                Case 3 : _read(args) : Return
                Case 4 : _write(args) : Return
                Case 5 : _open(args) : Return
                Case 6 : _close(args) : Return
                Case 8 : _creat(args) : Return
                Case 10 : _unlink(args) : Return
                Case 11 : _exec(args) : Return
                Case 15 : _chmod(args) : Return
                Case 17 : _break(args) : Return
                Case 18 : _stat(args) : Return
                Case 19 : _seek(args) : Return
                Case 20 : _getpid(args) : Return
                Case 41 : _dup(args) : Return
                Case 48 : _signal(args) : Return
            End Select
        End If
        Abort("invalid sys")
    End Sub

    Private Sub _indir(args As UShort()) ' 0
        Dim bak = PC
        PC = args(0)
        ExecSys()
        PC = bak
    End Sub

    Private Sub _exit(args As UShort()) ' 1
        swt.WriteLine("sys exit: r0={0}", Enc(Regs(0)))
        HasExited = True
    End Sub

    Private Sub _read(args As UShort()) ' 3
        Dim fss = fs.GetStream(Regs(0))
        Dim p = If(fss Is Nothing, "", """" + Escape(fss.Path) + """")
        swt.Write("sys read: fd(r0)={0}{1}, buf={2}, len={3} => ",
                  Enc(Regs(0)), p, Enc(args(0)), args(1))
        Try
            Regs(0) = CUShort(fss.Stream.Read(Data, args(0), args(1)))
            C = False
            swt.WriteLine("readlen={0}", Regs(0))
        Catch ex As Exception
            Regs(0) = 0
            C = True
            swt.WriteLine(ex.Message)
        End Try
    End Sub

    Private Sub _write(args As UShort()) ' 4
        Dim fss = fs.GetStream(Regs(0))
        Dim p = If(fss Is Nothing, "", """" + Escape(fss.Path) + """")
        Dim t = ReadString(Data, args(0), args(1))
        swt.WriteLine("sys write: fd(r0)={0}{1}, buf={2}""{3}"", len={4}",
                      Enc(Regs(0)), p, Enc(args(0)), Escape(t), args(1))
        Try
            If fss.Stream Is Nothing Then
                t = t.Replace(vbLf, vbCrLf)
                swt.Write(t)
                swo.Write(t)
            Else
                fss.Stream.Write(Data, args(0), args(1))
            End If
            C = False
        Catch
            Regs(0) = 0
            C = True
        End Try
    End Sub

    Private Sub _open(args As UShort()) ' 5
        Dim p = ReadString(Data, args(0))
        swt.Write("sys open: path={0}""{1}"", mode={2} => ", Enc(args(0)), Escape(p), args(1))
        Dim fss = fs.Open(p)
        If fss IsNot Nothing Then
            Regs(0) = CUShort(fss.Handle)
            C = False
        Else
            Regs(0) = 0
            C = True
        End If
        swt.WriteLine("fd={0}", Enc(Regs(0)))
    End Sub

    Private Sub _close(args As UShort()) ' 6
        Dim fss = fs.GetStream(Regs(0))
        Dim p = If(fss Is Nothing, "", """" + Escape(fss.Path) + """")
        swt.WriteLine("sys close: fd(r0)={0}{1}", Enc(Regs(0)), p)
        Try
            fss.Dispose()
            C = False
        Catch
            Regs(0) = 0
            C = True
        End Try
    End Sub

    Private Sub _creat(args As UShort()) ' 8
        Dim p = ReadString(Data, args(0))
        Regs(0) = CUShort(fs.Open(p, True).Handle)
        swt.WriteLine("sys creat: path={0}""{1}"", mode=0{2} => fd={3}",
                      Enc(args(0)), Escape(p), Oct(args(1), 3), Enc(Regs(0)))
        C = False
    End Sub

    Private Sub _unlink(args As UShort()) ' 10
        Dim p = ReadString(Data, args(0))
        swt.WriteLine("sys unlink: path={0}""{1}""", Enc(args(0)), Escape(p))
        If fs.Exists(p) Then
            fs.Delete(p)
            C = False
        Else
            C = True
        End If
    End Sub

    Private Sub _exec(args As UShort()) ' 11
        Dim p = ReadString(Data, args(0))
        Dim argp = args(1)
        Dim sb = New StringBuilder
        Do
            Dim a = ReadUInt16(argp)
            If a = 0 Then Exit Do
            If sb.Length > 0 Then sb.Append(", ")
            sb.Append("""" + Escape(ReadString(Data, a)) + """")
            argp += 2US
        Loop
        swt.WriteLine("sys exec: path={0}""{1}"", arg={2}{{{3}}}",
                      Enc(args(0)), Escape(p), Enc(args(1)), sb.ToString)
        Abort("not implemented")
    End Sub

    Private Sub _chmod(args As UShort()) ' 15
        Dim p = ReadString(Data, args(0))
        swt.WriteLine("sys chmod: path={0}""{1}"", mode=0{2}",
                      Enc(args(0)), Escape(p), Oct(args(1), 3))
        C = False
    End Sub

    Private Sub _break(args As UShort()) ' 17
        Dim nd = args(0)
        swt.WriteLine("sys break: nd={0}", Enc(nd))
        If nd < aout.BreakPoint OrElse nd >= Regs(6) Then
            Regs(0) = 0
            C = True
        Else
            breakpt = nd
            C = False
        End If
    End Sub

    Private Sub _stat(args As UShort()) ' 18
        Dim p = ReadString(Data, args(0))
        swt.WriteLine("sys stat: path={0}""{1}"", stat={2}", Enc(args(0)), Escape(p), EncAddr(args(1)))
        C = Not fs.Exists(p)
    End Sub

    Private Sub _seek(args As UShort()) ' 19
        Dim fss = fs.GetStream(Regs(0))
        Dim p = If(fss Is Nothing, "", """" + Escape(fss.Path) + """")
        swt.WriteLine("sys seek: fd(r0)={0}{1}, offset={2}, origin={3}",
                      Enc(Regs(0)), p, Enc(args(0)), args(1))
        Try
            Regs(0) = CUShort(fss.Stream.Seek(args(0), CType(args(1), SeekOrigin)))
            C = False
        Catch
            Regs(0) = 0
            C = True
        End Try
    End Sub

    Private Sub _getpid(args As UShort()) ' 20
        Regs(0) = CUShort(pid And &HFFFF)
        swt.WriteLine("sys getpid => {0}", Enc(Regs(0)))
        C = False
    End Sub

    Private Sub _dup(args As UShort()) ' 41
        Dim fss = fs.GetStream(Regs(0))
        Dim p = If(fss Is Nothing, "", """" + Escape(fss.Path) + """")
        swt.WriteLine("sys dup: fd(r0)={0}{1}", Enc(Regs(0)), p)
        Try
            Dim fss2 = fs.Duplicate(fss)
            Regs(0) = CUShort(fss2.Handle)
            C = False
        Catch
            Regs(0) = 0
            C = True
        End Try
    End Sub

    Private Sub _signal(args As UShort()) ' 48
        Dim sig = args(0)
        Dim sn = "" & sig
        If sig < SigNames.Length AndAlso SigNames(sig) IsNot Nothing Then
            sn = SigNames(sig)
        End If
        swt.WriteLine("sys signal: sig={0}, cb={1}", sn, EncAddr(args(1)))
        Regs(0) = 1 ' always ignore
    End Sub
End Class
