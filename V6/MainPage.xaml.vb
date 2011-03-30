Imports System.IO
Imports PDP11Lib

Partial Public Class MainPage
    Inherits UserControl

    Private aout As AOut

    Public Sub New()
        InitializeComponent()
        addTest("hello1")
        addTest("hello2")
        addTest("hello3")
        addTest("hello4")
        ReadResource("Tests/hello1")
    End Sub

    Public Sub Clear()
        txtDis.Text = ""
        txtSrc.Text = ""
        txtBin.Text = ""
        txtTrace.Text = ""
        txtOut.Text = ""
    End Sub

    Private Sub addTest(t$)
        Dim button = New Button With {.Content = t}
        AddHandler button.Click, AddressOf btnTest_Click
        menuStack.Children.Add(button)
    End Sub

    Private Sub ReadResource(fn$)
        Dim uri1 = New Uri(fn, UriKind.Relative)
        Dim rs1 = Application.GetResourceStream(uri1)
        If rs1 IsNot Nothing Then
            Using s = rs1.Stream
                ReadStream(s)
            End Using
        End If
        txtSrc.Text = ReadText(fn + ".c")
    End Sub

    Private Sub ReadStream(s As Stream)
        Dim data(CInt(s.Length - 1)) As Byte
        s.Read(data, 0, data.Length)
        ReadBytes(data)
    End Sub

    Private Sub ReadBytes(data As Byte())
        aout = New AOut(data)
        Run()
        btnSave.IsEnabled = True
    End Sub

    Private Sub btnOpen_Click(sender As Object, e As RoutedEventArgs)
        Dim ofd = New OpenFileDialog()
        If ofd.ShowDialog() <> True Then Return

        Clear()
        Try
            Dim fi = ofd.File
            If fi.Length >= 64 * 1024 Then
                Throw New Exception("ファイルが大き過ぎます。上限は64KBです。")
            End If
            Using fs = ofd.File.OpenRead()
                ReadStream(fs)
            End Using
        Catch ex As Exception
            txtDis.Text = ex.Message + Environment.NewLine +
                "読み込みに失敗しました。" + Environment.NewLine
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs)
        Dim sfd = New SaveFileDialog()
        If sfd.ShowDialog() <> True Then Return

        Using fs = sfd.OpenFile()
            fs.Write(aout.Data, 0, aout.Data.Length)
        End Using
    End Sub

    Private Sub btnTest_Click(sender As Object, e As RoutedEventArgs)
        Dim button = CType(sender, Button)
        If button Is Nothing Then Return

        Dim cur = Cursor
        Cursor = Cursors.Wait
        ReadResource("Tests/" + button.Content.ToString())
        Cursor = cur
    End Sub

    Private Sub comboBox1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Run()
    End Sub

    Private Sub Run()
        If aout Is Nothing Then Return

        Dim cur = Cursor
        Cursor = Cursors.Wait
        aout.UseOct = comboBox1.SelectedIndex = 1
        Dim vm = New VM(aout)
        vm.Run()
        txtDis.Text = aout.GetDisassemble()
        txtBin.Text = aout.GetDump()
        txtTrace.Text = vm.Trace
        txtTrace.SelectionStart = txtTrace.Text.Length
        txtOut.Text = vm.Output
        txtOut.SelectionStart = txtOut.Text.Length
        Cursor = cur
    End Sub
End Class
