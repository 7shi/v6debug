﻿Imports System.IO

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
        For i = Offset + tsize + dsize To image.Length - 1 Step 12
            Dim sym = New Symbol(image, i)
            If syms.ContainsKey(sym.Address) Then
                syms(sym.Address).SetSymbol(sym)
            ElseIf sym.IsObject Then
                syms.Add(sym.Address, New Symbol(sym))
            Else
                syms.Add(sym.Address, sym)
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
        tw.WriteLine(".text [{0:x4}]{1} - [{2:x4}]{3}",
                     16, Enc(0US), tsize + 15, Enc(tsize - 1US))
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

        Dim baddr = tsize + dsize
        tw.WriteLine()
        tw.WriteLine(".data [{0:x4}]{1} - [{2:x4}]{3}",
                     tsize + 16, Enc(tsize), baddr + 15, Enc(baddr - 1US))
        Dim dsyms = From sym In symlist Where GetSection(sym) = 2 Select sym
        For Each sym In dsyms
            tw.WriteLine("[{0:x4}] {1}: {2}:",
                         sym.Address + 16, Enc0(CUShort(sym.Address)), sym.Name)
        Next

        Dim last = baddr + bsize
        tw.WriteLine()
        tw.WriteLine(".bss  [----]{0} - [----]{1}", Enc(baddr), Enc(last - 1US))
        Dim bsyms = From sym In symlist Where GetSection(sym) = 3 Select sym
        For Each sym In bsyms
            tw.WriteLine("[----] {0}: {1}:", Enc0(CUShort(sym.Address)), sym.Name)
        Next
    End Sub

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
        If sym Is Nothing Then Return ad
        Dim d = addr - sym.Address
        Dim ad2 = sym.Name
        If d > 0 Then ad2 += "+" + Enc(CUShort(d))
        Return ad2 + "<" + ad + ">"
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
End Class
