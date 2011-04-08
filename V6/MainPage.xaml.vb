Imports System.IO
Imports PDP11Lib

Partial Public Class MainPage
    Inherits UserControl

    Private aout As AOut
    Private parg As ProcArg
    Private fs As New SLFileSystem("Tests")

    Private Class ProcArg
        Public Cmd$
        Public Srcs As String()
        Public Args As String()
        Public Verbose As Boolean
    End Class

    Public Sub New()
        InitializeComponent()
        Dim b = addTest(True, New String() {"hello1.c"}, "hello1")
        addTest(True, New String() {"hello2.c"}, "hello2")
        addTest(True, New String() {"hello3.c"}, "hello3")
        addTest(True, New String() {"hello4.c"}, "hello4")
        addTest(True, New String() {"args.c"}, "args", "test", "arg")
        addTest(True, New String() {"printo.c"}, "printo")
        addTest(False, Nothing, "/bin/nm", "args")
        addTest(False, Nothing, "/bin/as",
                "source/as/as11.s", "source/as/as12.s", "source/as/as13.s", "source/as/as14.s", "source/as/as15.s",
                "source/as/as16.s", "source/as/as17.s", "source/as/as18.s", "source/as/as19.s")
        addTest(False, Nothing, "/lib/as2", "/tmp/atm1a", "/tmp/atm2a", "/tmp/atm3a")
        addTest(False, Nothing, "/bin/ld", "-s", "-n", "a.out")
        addTest(False, Nothing, "/bin/cc", "-S", "args.c")
        addTest(False, New String() {"source/c/c0h.c", "source/c/c0t.s"},
                "/lib/c0", "args.c", "/tmp/ctm1a", "/tmp/ctm2a")
        addTest(False, Nothing, "source/c/cvopt", "source/c/table.s", "source/c/table.i")
        btnTest_Click(b, Nothing)
    End Sub

    Public Sub Clear()
        txtDis.Text = ""
        txtSym.Text = ""
        txtSrc.Text = ""
        txtBin.Text = ""
        txtTrace.Text = ""
        ignore = True
        TreeView1.Items.Clear()
        DataGrid1.ItemsSource = Nothing
        ignore = False
    End Sub

    Private Function addTest(verbose As Boolean, srcs$(), cmd$, ParamArray args$()) As Button
        Dim parg = New ProcArg With
                   {.Cmd = cmd, .Srcs = srcs, .Args = args, .Verbose = verbose}
        Dim p = cmd.LastIndexOf("/")
        Dim fn = If(p < 0, cmd, cmd.Substring(p + 1))
        Dim button = New Button With {.Content = fn, .Tag = parg}
        AddHandler button.Click, AddressOf btnTest_Click
        menuStack.Children.Add(button)
        Return button
    End Function

    Private Sub ReadFile(parg As ProcArg)
        Clear()
        Using s = fs.Open(parg.Cmd)
            ReadStream(s.Stream, parg)
        End Using
        txtSym.Text = VM.System(fs, "nm", parg.Cmd).Output
    End Sub

    Private Sub ReadStream(s As Stream, parg As ProcArg)
        Dim data(CInt(s.Length - 1)) As Byte
        s.Read(data, 0, data.Length)
        aout = New AOut(data, GetFileName(parg.Cmd))

        ignore = True
        Dim list = New List(Of String)
        If parg.Srcs IsNot Nothing Then
            For Each src In parg.Srcs
                If src.Contains("/") Then
                    list.Add(src)
                Else
                    Dim n = New TreeViewItem With {.Header = GetFileName(src), .Tag = src}
                    TreeView1.Items.Add(n)
                End If
            Next
        End If
        Dim objs = From sym In aout.GetSymbols
                   Where sym.ObjSym IsNot Nothing
                   Select sym.ObjSym
        For Each obj In objs
            Dim src = checkLib(obj.Name)
            If src IsNot Nothing Then List.Add(src)
        Next
        list.Sort()
        Dim dn As TreeViewItem = Nothing
        For Each src In list
            If fs.Exists(src) Then
                Dim sp = src.Split(CChar("/"))
                If dn Is Nothing OrElse sp(1) <> dn.Header.ToString Then
                    dn = New TreeViewItem With
                         {.Header = sp(1), .Tag = "README", .IsExpanded = True}
                    TreeView1.Items.Add(dn)
                End If
                Dim n = New TreeViewItem With {.Header = sp(2), .Tag = src}
                dn.Items.Add(n)
            End If
        Next
        Dim first = CType(TreeView1.Items(0), TreeViewItem)
        If first.Items.Count > 0 Then
            first = CType(first.Items(0), TreeViewItem)
        End If
        first.IsSelected = True
        showSource(first)
        ignore = False

        Me.parg = parg
        Run()
        btnSave.IsEnabled = True
    End Sub

    Private srcdic As New Dictionary(Of String, String)

    Private Function checkLib$(obj$)
        If srcdic.ContainsKey(obj) Then Return srcdic(obj)

        Dim p = obj.LastIndexOf(".")
        Dim fn = If(p < 0, obj, obj.Substring(0, p))
        For Each dir In New String() {"s4", "s5", "s1", "s2", "as", "c"}
            For Each ext In New String() {".s", ".c"}
                Dim pp = "source/" + dir + "/" + fn + ext
                If fs.Exists(pp) Then
                    srcdic.Add(obj, pp)
                    Return pp
                End If
            Next
        Next
        srcdic.Add(obj, Nothing)
        Return Nothing
    End Function

    Private Sub btnOpen_Click(sender As Object, e As RoutedEventArgs) Handles btnOpen.Click
        Dim ofd = New OpenFileDialog
        If ofd.ShowDialog() <> True Then Return

        Clear()
        Try
            Dim fi = ofd.File
            If fi.Length >= 64 * 1024 Then
                Throw New Exception("ファイルが大き過ぎます。上限は64KBです。")
            End If
            Using fs1 = ofd.File.OpenRead, fs2 = fs.Open(ofd.File.Name, True)
                Dim buf(CInt(fs1.Length - 1)) As Byte
                fs1.Read(buf, 0, buf.Length)
                fs2.Stream.Write(buf, 0, buf.Length)
            End Using
            ReadFile(New ProcArg With {.Cmd = ofd.File.Name})
        Catch ex As Exception
            txtDis.Text = ex.Message + Environment.NewLine +
                "読み込みに失敗しました。" + Environment.NewLine
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        Dim fe = TryCast(DataGrid1.SelectedItem, FileEntry)
        If fe Is Nothing Then Return

        Dim sfd = New SaveFileDialog
        If sfd.ShowDialog() <> True Then Return

        Using fs1 = fs.Open(fe.Path), fs2 = sfd.OpenFile
            Dim buf(CInt(fs1.Stream.Length - 1)) As Byte
            fs1.Stream.Read(buf, 0, buf.Length)
            fs2.Write(buf, 0, buf.Length)
        End Using
    End Sub

    Private Sub btnTest_Click(sender As Object, e As RoutedEventArgs)
        Dim button = CType(sender, Button)
        If button Is Nothing Then Return

        Dim cur = Cursor
        Cursor = Cursors.Wait
        ReadFile(CType(button.Tag, ProcArg))
        Cursor = cur
    End Sub

    Private Sub comboBox1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles comboBox1.SelectionChanged
        Run()
    End Sub

    Private Sub Run()
        If aout Is Nothing Then Return

        Dim cur = Cursor
        Cursor = Cursors.Wait
        aout.UseOct = comboBox1.SelectedIndex = 1
        Dim vm = New VM(aout, fs, parg.Verbose)
        vm.Run(parg.Args)
        txtDis.Text = aout.GetDisassemble
        txtTrace.Text = vm.Trace
        txtTrace.SelectionStart = txtTrace.Text.Length
        txtOut.Text += vm.Output
        txtOut.SelectionStart = txtOut.Text.Length
        Dim fsrc = New List(Of FileEntry)
        fsrc.Add(New FileEntry With {.Path = parg.Cmd, .Length = aout.Data.Length})
        For Each f In fs.GetFiles
            fsrc.Add(New FileEntry With {.Path = f, .Length = fs.GetLength(f)})
        Next
        Dim ign = ignore
        ignore = True
        DataGrid1.ItemsSource = fsrc
        ignore = ign
        DataGrid1.SelectedIndex = 0
        Cursor = cur
    End Sub

    Public Shared Function GetFileName$(path$)
        Dim p = path.LastIndexOf(CChar("/"))
        Return If(p < 0, path, path.Substring(p + 1))
    End Function

    Private ignore As Boolean

    Private Sub TreeView1_SelectedItemChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object)) Handles TreeView1.SelectedItemChanged
        If ignore Then Return
        showSource(TryCast(e.NewValue, TreeViewItem))
    End Sub

    Private Sub showSource(n As TreeViewItem)
        If n Is Nothing OrElse n.Tag Is Nothing Then
            txtSrc.Text = ""
        Else
            Using s = fs.Open(n.Tag.ToString)
                txtSrc.Text = ReadText(s.Stream)
                txtSrc.SelectionStart = 0
            End Using
        End If
    End Sub

    Public Class FileEntry
        Public Property Path$
        Public Property Length%
    End Class

    Private Sub DataGrid1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles DataGrid1.SelectionChanged
        showBinary(TryCast(DataGrid1.SelectedItem, FileEntry))
    End Sub

    Private Sub showBinary(fe As FileEntry)
        If ignore Then Return

        If fe Is Nothing Then
            txtBin.Text = ""
        Else
            Dim bd = New BinData(fs.GetAllBytes(fe.Path))
            txtBin.Text = bd.GetDump
        End If
    End Sub
End Class
