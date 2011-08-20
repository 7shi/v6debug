Imports System.IO
Imports System.Windows.Resources
Imports PDP11Lib

Partial Public Class MainPage
    Inherits UserControl

    Private parg As ProcArg
    Private root As Byte()
    Private boot As BinData

    Private Class ProcArg
        Public Cmd$
        Public Srcs As String()
        Public Args As String()
        Public Verbose As Boolean
    End Class

    Public Sub New()
        InitializeComponent()
        Dim b = addTest(True, New String() {"hello1.c"}, "hello1")

        Using rs = Application.GetResourceStream(New Uri("v6root.zip", UriKind.Relative)).Stream
            Dim sri = New StreamResourceInfo(rs, Nothing)
            Using s = Application.GetResourceStream(sri, New Uri("v6root", UriKind.Relative)).Stream
                ReDim root(CInt(s.Length) - 1)
                s.Read(root, 0, root.Length)
            End Using
        End Using

        boot = New BinData(&H10000)
        boot.Write(&O2000,
                   &O42113,
                   &O12706, &O2000,
                   &O12700, &O0,
                   &O10003,
                   &O303,
                   &O6303,
                   &O6303,
                   &O6303,
                   &O6303,
                   &O6303,
                   &O12701, &O177412,
                   &O10311,
                   &O5041,
                   &O12741, &O177000,
                   &O12741, &O5,
                   &O5002,
                   &O5003,
                   &O12704, &O2020,
                   &O5005,
                   &O105711,
                   &O100376,
                   &O105011,
                   &O5007)
        boot.Write(&O100000,
                   &O12700, &O177412, &O5040, &O10040,
                   &O12740, &O5, &O105710, &O2376, &O5007)
        txtSrc.Text = boot.GetDump(&O2000, &O2071) + vbCrLf + boot.GetDump(&O100000, &O100021)

        disasmBoot()
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

    Private Sub btnTest_Click(sender As Object, e As RoutedEventArgs)
        Dim button = CType(sender, Button)
        If button Is Nothing Then Return

        Dim cur = Cursor
        Cursor = Cursors.Wait
        ReadFile(CType(button.Tag, ProcArg))
        Cursor = cur
    End Sub

    Private Sub comboBox1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles comboBox1.SelectionChanged
        disasmBoot()
    End Sub

    Private Sub disasmBoot()
        Dim cur = Cursor
        Cursor = Cursors.Wait
        boot.UseOct = comboBox1.SelectedIndex = 1
        Dim sw = New StringWriter
        For i = &O2000 To &O2071
            i += Disassemble(boot, sw, i, 0) - 1
        Next
        sw.WriteLine()
        For i = &O100000 To &O100021
            i += Disassemble(boot, sw, i, 0) - 1
        Next
        txtDis.Text = sw.ToString
        Cursor = cur
    End Sub
End Class
