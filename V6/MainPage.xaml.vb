Imports System.IO
Imports PDP11Lib

Partial Public Class MainPage
    Inherits UserControl

    Private aout As AOut
    Private parg As ProcArg
    Private fs As New SLFileSystem("Tests")

    Private Class ProcArg
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
        addTest(False, New String() {"nm.c"}, "nm", "args")
        btnTest_Click(b, Nothing)
    End Sub

    Public Sub Clear()
        txtDis.Text = ""
        txtSym.Text = ""
        txtSrc.Text = ""
        txtBin.Text = ""
        txtTrace.Text = ""
        txtOut.Text = ""
        ignore = True
        ListBox1.Items.Clear()
        ignore = False
    End Sub

    Private Function addTest(verbose As Boolean, srcs$(), t$, ParamArray args$()) As Button
        Dim parg = New ProcArg With {.Srcs = srcs, .Args = args, .Verbose = verbose}
        Dim button = New Button With {.Content = t, .Tag = parg}
        AddHandler button.Click, AddressOf btnTest_Click
        menuStack.Children.Add(button)
        Return button
    End Function

    Private Sub ReadFile(path$, parg As ProcArg)
        Clear()
        Using s = fs.Open(path)
            ReadStream(s.Stream, GetFileName(path), parg)
        End Using
        txtSym.Text = VM.System(fs, "nm", path).Output
    End Sub

    Private Sub ReadStream(s As Stream, path$, parg As ProcArg)
        Dim data(CInt(s.Length - 1)) As Byte
        s.Read(data, 0, data.Length)
        aout = New AOut(data, path)

        ignore = True
        For Each src In parg.Srcs
            ListBox1.Items.Add(src)
        Next
        Dim objs = From sym In aout.GetSymbols
                   Where sym.ObjSym IsNot Nothing
                   Select sym.ObjSym
        Dim list = New List(Of String)
        For Each obj In objs
            Dim src = checkLib(obj.Name)
            If src IsNot Nothing Then list.Add(src)
        Next
        list.Sort()
        For Each src In list
            ListBox1.Items.Add(src)
        Next
        ignore = False
        ListBox1.SelectedIndex = 0

        Me.parg = parg
        Run()
        btnSave.IsEnabled = True
    End Sub

    Private Function checkLib$(obj$)
        Dim p = obj.LastIndexOf(".")
        If p < 0 Then Return Nothing
        Dim fn = obj.Substring(0, p)
        For Each dir In New String() {"s4", "s5"}
            For Each ext In New String() {".s", ".c"}
                If fs.Exists(dir + "/" + fn + ext) Then
                    Return dir + "/" + fn + ext
                End If
            Next
        Next
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
            Using fs = ofd.File.OpenRead
                ReadStream(fs, ofd.File.Name, Nothing)
            End Using
        Catch ex As Exception
            txtDis.Text = ex.Message + Environment.NewLine +
                "読み込みに失敗しました。" + Environment.NewLine
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        Dim sfd = New SaveFileDialog
        If sfd.ShowDialog() <> True Then Return

        Using fs = sfd.OpenFile
            fs.Write(aout.Data, 0, aout.Data.Length)
        End Using
    End Sub

    Private Sub btnTest_Click(sender As Object, e As RoutedEventArgs)
        Dim button = CType(sender, Button)
        If button Is Nothing Then Return

        Dim cur = Cursor
        Cursor = Cursors.Wait
        ReadFile(button.Content.ToString, CType(button.Tag, ProcArg))
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
        txtBin.Text = aout.GetDump
        txtTrace.Text = vm.Trace
        txtTrace.SelectionStart = txtTrace.Text.Length
        txtOut.Text = vm.Output
        txtOut.SelectionStart = txtOut.Text.Length
        Cursor = cur
    End Sub

    Public Shared Function GetFileName$(path$)
        Dim p = path.LastIndexOf(CChar("/"))
        Return If(p < 0, path, path.Substring(p + 1))
    End Function

    Private ignore As Boolean

    Private Sub ListBox1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ListBox1.SelectionChanged
        If ignore Then Return
        Dim sel = ListBox1.SelectedItem
        If sel IsNot Nothing Then
            Using s = fs.Open(sel.ToString)
                txtSrc.Text = ReadText(s.Stream)
            End Using
        Else
            txtSrc.Text = ""
        End If
    End Sub
End Class
