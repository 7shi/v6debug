Imports System.IO
Imports System.Windows.Resources
Imports PDP11Lib

Partial Public Class MainPage
    Inherits UserControl

    Public Class DGEntry
        Public Property Name$
        Public Property Value$

        Public Sub New(n$, v$)
            Name = n
            Value = v
        End Sub
    End Class

    Private root As Byte()
    Private boot As BinData

    Private current%
    Private dlist As List(Of DisEntry)

    Public Sub New()
        InitializeComponent()

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

        Dim stacksrc(7) As DGEntry
        For i = 0 To 7
            stacksrc(i) = New DGEntry(String.Format("{0:x4}", i * 2), "0000")
        Next
        dgStk.ItemsSource = stacksrc

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

    Private Sub comboBox1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles comboBox1.SelectionChanged
        disasmBoot()
    End Sub

    Private Sub disasmBoot()
        Dim cur = Cursor
        Cursor = Cursors.Wait
        boot.UseOct = comboBox1.SelectedIndex = 1
        dlist = New List(Of DisEntry)
        For i = &H400 To &H413
            Dim de = New DisEntry(boot, i)
            dlist.Add(de)
            i += de.Length - 1
        Next
        dlist(current).Mark = ">"
        dgDis.ItemsSource = dlist
        Cursor = cur
    End Sub

    Private Sub btnStep_Click(sender As Object, e As RoutedEventArgs) Handles btnStep.Click
        If current < dlist.Count - 1 Then
            dlist(current).Mark = ""
            current += 1
            dlist(current).Mark = ">"
        End If
    End Sub
End Class
