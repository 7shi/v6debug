Imports System.IO
Imports PDP11Lib

Partial Public Class MainPage
    Inherits UserControl

    Private aout As AOut

    Public Sub New()
        InitializeComponent()
        ReadResource("Tests/hello")
    End Sub

    Private Sub ReadResource(fn As String)
        Dim uri = New Uri(fn, UriKind.Relative)
        Dim rs = Application.GetResourceStream(uri)
        If Not rs Is Nothing Then
            Using s = rs.Stream
                ReadStream(s)
            End Using
        End If
    End Sub

    Private Sub ReadStream(s As Stream)
        Dim data(s.Length - 1) As Byte
        s.Read(data, 0, data.Length)
        ReadBytes(data)
    End Sub

    Private Sub ReadBytes(data() As Byte)
        aout = New AOut(data) With {.UseOct = comboBox1.SelectedIndex = 1}
        textBox1.Text = aout.GetDisassemble()
        textBox2.Text = aout.GetDump()
        btnSave.IsEnabled = True
    End Sub

    Private Sub btnOpen_Click(sender As Object, e As RoutedEventArgs)
        Dim ofd = New OpenFileDialog()
        If ofd.ShowDialog() <> True Then Return

        textBox1.Text = ""
        Try
            Dim fi = ofd.File
            If fi.Length >= 64 * 1024 Then
                Throw New Exception("ファイルが大き過ぎます。上限は64KBです。")
            End If
            Using fs = ofd.File.OpenRead()
                ReadStream(fs)
            End Using
        Catch ex As Exception
            textBox1.Text = ex.Message + Environment.NewLine +
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
        If aout Is Nothing Then Return

        Dim f = comboBox1.SelectedIndex = 1
        If aout.UseOct = f Then Return

        Dim cur = Cursor
        Cursor = Cursors.Wait
        aout.UseOct = f
        textBox1.Text = aout.GetDisassemble()
        textBox2.Text = aout.GetDump()
        Cursor = cur
    End Sub
End Class
