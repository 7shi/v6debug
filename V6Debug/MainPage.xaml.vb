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
    End Sub

    Public Sub Clear()
        txtDis.Text = ""
        txtSrc.Text = ""
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
        txtOut.Text += vm.Output
        txtOut.SelectionStart = txtOut.Text.Length
        Dim fsrc = New List(Of FileEntry)
        fsrc.Add(New FileEntry With {.Path = parg.Cmd, .Length = aout.Data.Length})
        For Each f In fs.GetFiles
            fsrc.Add(New FileEntry With {.Path = f, .Length = fs.GetLength(f)})
        Next
        Cursor = cur
    End Sub

    Public Shared Function GetFileName$(path$)
        Dim p = path.LastIndexOf(CChar("/"))
        Return If(p < 0, path, path.Substring(p + 1))
    End Function

    Public Class FileEntry
        Public Property Path$
        Public Property Length%
    End Class
End Class
