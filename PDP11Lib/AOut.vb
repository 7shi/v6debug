Imports System.IO

Public Class AOut
    Inherits BinData

    Public fmagic As UShort, tsize As UShort, dsize As UShort, bsize As UShort,
        ssize As UShort, entry, pad As UShort, relflg As UShort

    Public Property Path$

    Private symlist As New List(Of Symbol)

    Public Function GetSymbols() As Symbol()
        Return symlist.ToArray
    End Function

    Public Sub New(image As Byte(), p$)
        MyBase.New(image)
        Offset = 16
        Path = p

        fmagic = BitConverter.ToUInt16(image, 0)
        tsize = BitConverter.ToUInt16(image, 2)
        dsize = BitConverter.ToUInt16(image, 4)
        bsize = BitConverter.ToUInt16(image, 6)
        ssize = BitConverter.ToUInt16(image, 8)
        entry = BitConverter.ToUInt16(image, 10)
        pad = BitConverter.ToUInt16(image, 12)
        relflg = BitConverter.ToUInt16(image, 14)

        Dim syms As New Dictionary(Of Integer, Symbol)
        Dim last As Symbol = Nothing
        For i = Offset + tsize + dsize To image.Length - 1 Step 12
            Dim sym = New Symbol(image, i)
            If sym.IsLocal Then
                If last IsNot Nothing Then last.AddLocal(sym)
            ElseIf sym.Type <> &H14 Then
                If sym.IsObject Then sym = New Symbol(sym)
                If syms.ContainsKey(sym.Address) Then
                    last = syms(sym.Address)
                    last.SetSymbol(sym)
                Else
                    syms.Add(sym.Address, sym)
                    last = sym
                End If
            End If
        Next
        symlist.AddRange(syms.Values)
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
        tw.WriteLine(".text [{0:x4}] {1} - [{2:x4}] {3}",
                     16, Enc0(0US), tsize + 15, Enc0(tsize - 1US))
        Dim en = symlist.GetEnumerator
        Dim hasSym = en.MoveNext()
        Dim thunks = New Dictionary(Of Integer, Symbol)
        For i = 0 To tsize - 1
            While hasSym AndAlso i >= en.Current.Address
                i = en.Current.Address
                tw.WriteLine("{0}{1}", Space(7), en.Current)
                For Each local In en.Current.GetLocals
                    tw.WriteLine("{0}{1} = {2}", Space(11), local.Name, local.Address)
                Next
                hasSym = en.MoveNext()
            End While
            If ReadUInt16(i) = &H8900 Then
                Dim ad = ReadUInt16(i + 2)
                If Not thunks.ContainsKey(ad) Then thunks.Add(ad, New Symbol(ad))
            End If
            Dim maxlen = If(hasSym, en.Current.Address - i, 0)
            i += Disassemble(tw, i, maxlen) - 1
        Next

        Dim baddr = tsize + dsize
        If dsize > 0 Then
            tw.WriteLine()
            tw.WriteLine(".data [{0:x4}] {1} - [{2:x4}] {3}",
                         tsize + 16, Enc0(tsize), baddr + 15, Enc0(baddr - 1US))
            Dim dsyms = From sym In symlist.Concat(thunks.Values)
                        Where GetSection(sym) = 2
                        Select sym
                        Order By sym.Address * 2 + If(sym.IsNull, 1, 0)
            For Each sym In dsyms
                If Not sym.IsNull Then
                    tw.WriteLine("[{0:x4}] {1}: {2}", sym.Address + 16, Enc0(CUShort(sym.Address)), sym)
                Else
                    Disassemble(tw, sym.Address)
                End If
            Next
        End If

        If bsize > 0 Then
            Dim brkpt = BreakPoint
            tw.WriteLine()
            tw.WriteLine(".bss  [----] {0} - [----] {1}", Enc0(baddr), Enc0(brkpt - 1US))
            Dim bsyms = From sym In symlist Where GetSection(sym) = 3 Select sym
            For Each sym In bsyms
                tw.WriteLine("[----] {0}: {1}", Enc0(CUShort(sym.Address)), sym)
            Next
        End If
    End Sub

    Private Function Disassemble%(tw As TextWriter, i%, Optional maxlen% = 0)
        Dim spclen = Enc0(0US).Length
        Dim op = Disassembler.Disassemble(Me, i)
        Dim len = 2
        If op IsNot Nothing Then
            If maxlen > 0 AndAlso op.Length > maxlen Then
                op = Nothing
                len = maxlen
            Else
                len = op.Length
            End If
        End If
        Dim s = ReadUInt16(i)
        tw.Write("[{0:x4}] {1}: {2}", 16 + i, Enc0(CUShort(i)), Enc0(s))
        For j = 2 To 4 Step 2
            If j < len Then
                tw.Write(" " + Enc0(ReadUInt16(i + j)))
            Else
                tw.Write(Space(1 + spclen))
            End If
        Next
        tw.Write("  ")
        If op IsNot Nothing Then
            tw.Write(op.Mnemonic)
        Else
            For j = 0 To len - 1 Step 2
                If j > 0 Then tw.Write("; ")
                tw.Write(Enc(ReadUInt16(i + j)))
            Next
        End If
        If len > 6 Then
            Dim indent = Space(9 + spclen * 2)
            For j = 6 To op.Length Step 2
                If ((j - 2) And 3) = 0 Then
                    tw.WriteLine()
                    tw.Write(indent)
                End If
                If j < len Then
                    tw.Write(" " + Enc0(ReadUInt16(i + j)))
                Else
                    tw.Write(Space(1 + spclen))
                End If
            Next
        End If
        tw.WriteLine()
        Return len
    End Function

    Public Function GetDisassemble$()
        Using sw = New StringWriter()
            Disassemble(sw)
            Return sw.ToString()
        End Using
    End Function

    Public Function GetSymbol(addr%) As Symbol
        Dim sect = GetSection(addr)
        Dim ret As Symbol = Nothing
        If addr < tsize + dsize + bsize Then
            For Each sym In symlist
                If addr < sym.Address Then Exit For
                If GetSection(sym) = sect Then ret = sym
            Next
        End If
        Return ret
    End Function

    Public Overrides Function EncAddr(addr As UShort) As String
        Dim ad = MyBase.EncAddr(addr)
        Dim sym = GetSymbol(addr)
        If sym Is Nothing OrElse sym.Name Is Nothing Then Return ad
        Dim d = addr - sym.Address
        Dim ad2 = sym.Name
        If d > 0 Then ad2 += "+" + Enc(CUShort(d))
        Return ad2 + "<" + ad + ">"
    End Function

    Public Overrides Function GetRelative$(r%, d%, ad%)
        If r = 5 Then
            Dim sym = GetSymbol(ad)
            If sym IsNot Nothing Then
                Dim local = sym.GetLocal(d)
                If local IsNot Nothing Then
                    Return local.Name + "(" + RegNames(r) + ")"
                End If
            End If
        End If
        Return MyBase.GetRelative(r, d, ad)
    End Function

    Public Function GetSection%(pos%)
        If pos < 0 Then Return 0
        If pos < tsize Then Return 1
        Dim baddr = tsize + dsize
        If pos < baddr Then Return 2
        If pos < baddr + bsize Then Return 3
        Return 0
    End Function

    Public Function GetSection%(sym As Symbol)
        Return GetSection(sym.Address)
    End Function

    Public ReadOnly Property BreakPoint As UShort
        Get
            Return tsize + dsize + bsize
        End Get
    End Property
End Class
