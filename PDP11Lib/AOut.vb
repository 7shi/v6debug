Imports System.IO

Public Class AOut
    Inherits BinData

    Public fmagic As UShort, tsize As UShort, dsize As UShort, bsize As UShort,
        ssize As UShort, entry, pad As UShort, relflg As UShort

    Private symlist As New List(Of Symbol)

    Public Sub New(image As Byte())
        MyBase.New(image)
        Offset = 16

        fmagic = BitConverter.ToUInt16(image, 0)
        tsize = BitConverter.ToUInt16(image, 2)
        dsize = BitConverter.ToUInt16(image, 4)
        bsize = BitConverter.ToUInt16(image, 6)
        ssize = BitConverter.ToUInt16(image, 8)
        entry = BitConverter.ToUInt16(image, 10)
        pad = BitConverter.ToUInt16(image, 12)
        relflg = BitConverter.ToUInt16(image, 14)

        Dim syms As New Dictionary(Of Integer, Symbol)
        Dim srcs As New Dictionary(Of Integer, Symbol)
        For i = Offset + tsize + dsize To image.Length - 1 Step 12
            Dim sym = New Symbol(image, i)
            If sym.IsObject Then
                If Not srcs.ContainsKey(sym.Address) Then
                    srcs.Add(sym.Address, sym)
                End If
            ElseIf sym.Type <> 2 Then
                If Not syms.ContainsKey(sym.Address) Then
                    syms.Add(sym.Address, sym)
                    symlist.Add(sym)
                End If
            End If
        Next
        For Each src In srcs.Values
            If syms.ContainsKey(src.Address) Then
                syms(src.Address).Source = src
            Else
                symlist.Add(src)
            End If
        Next
        symlist.Sort(Function(a, b) a.Address - b.Address)
    End Sub

    Public Sub Disassemble(tw As TextWriter)
        Dim opmagic = Disassembler.Disassemble(New Byte() {Data(0), Data(1)}, UseOct)
        tw.WriteLine("[{0:x4}] fmagic = {1}  {2}", 0, Enc0(fmagic), opmagic.Mnemonic)
        tw.WriteLine("[{0:x4}] tsize  = {1}", 2, Enc0(tsize))
        tw.WriteLine("[{0:x4}] dsize  = {1}", 4, Enc0(dsize))
        tw.WriteLine("[{0:x4}] bsize  = {1}", 6, Enc0(bsize))
        tw.WriteLine("[{0:x4}] ssize  = {1}", 8, Enc0(ssize))
        tw.WriteLine("[{0:x4}] entry  = {1}", 10, Enc0(entry))
        tw.WriteLine("[{0:x4}] pad    = {1}", 12, Enc0(pad))
        tw.WriteLine("[{0:x4}] relflg = {1}", 14, Enc0(relflg))
        tw.WriteLine()
        tw.WriteLine(".text")
        Dim en = symlist.GetEnumerator
        Dim hasSym = en.MoveNext()
        For i = 0 To tsize - 1
            If hasSym AndAlso en.Current.Address = i Then
                tw.WriteLine("       {0}", en.Current)
                hasSym = en.MoveNext()
            End If

            Dim op = Disassembler.Disassemble(Me, i)
            Dim len = 2
            If op IsNot Nothing Then len = op.Length
            Dim s = ReadUInt16(i)
            tw.Write("[{0:x4}] {1}: {2}", 16 + i, Enc0(CUShort(i)), Enc0(s))
            For j = 2 To 4 Step 2
                If j < len Then
                    tw.Write(" " + Enc0(ReadUInt16(i + j)))
                ElseIf UseOct Then
                    tw.Write("       ")
                Else
                    tw.Write("     ")
                End If
            Next
            tw.Write("  ")
            If op IsNot Nothing Then
                tw.Write(op.Mnemonic)
            Else
                tw.Write(Enc(s))
            End If
            tw.WriteLine()
            i += len - 1
        Next
    End Sub

    Public Function GetDisassemble$()
        Using sw = New StringWriter()
            Disassemble(sw)
            Return sw.ToString()
        End Using
    End Function

    Public Function GetSymbol(addr%) As Symbol
        Dim ret As Symbol = Nothing
        For Each sym In symlist
            If addr < sym.Address Then Exit For
            ret = sym
        Next
        Return ret
    End Function
End Class
