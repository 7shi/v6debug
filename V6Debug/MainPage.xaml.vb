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

        Dim regsrc(7) As DGEntry
        Dim regs = {"r0", "r1", "r2", "r3", "r4", "r5(fp)", "r6(sp)", "r7(pc)"}
        For i = 0 To 7
            regsrc(i) = New DGEntry(regs(i), "0000")
        Next
        dgReg.ItemsSource = regsrc

        boot = New BinData(&H10000)
        boot.Write(&H400,
                   &O12700, &O177414, &O5040, &O5040,
                   &O10040, &O12740, &O5, &O105710,
                   &O2376, &O5007)

        Dim mlist = New List(Of DumpEntry)
        mlist.AddRange(boot.Dump(0, &H41F))
        dgMem.ItemsSource = mlist

        disasmBoot()
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

    Private Sub btnTest_Click(sender As Object, e As RoutedEventArgs)
        Dim button = CType(sender, Button)
        If button Is Nothing Then Return

        Dim cur = Cursor
        Cursor = Cursors.Wait
        'ReadFile(CType(button.Tag, ProcArg))
        Cursor = cur
    End Sub

    Private Sub comboBox1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles comboBox1.SelectionChanged
        disasmBoot()
    End Sub

    Private Sub disasmBoot()
        Dim cur = Cursor
        Cursor = Cursors.Wait
        boot.UseOct = comboBox1.SelectedIndex = 1
        Dim list = New List(Of DisEntry)
        For i = &H400 To &H413
            Dim de = New DisEntry(boot, i)
            list.Add(de)
            i += de.Length - 1
        Next
        dgDis.ItemsSource = list
        Cursor = cur
    End Sub

    Public Class DGEntry
        Public Property Name$
        Public Property Value$

        Public Sub New(n$, v$)
            Name = n
            Value = v
        End Sub
    End Class
End Class
