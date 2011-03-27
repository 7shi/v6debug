Imports System.IO

Public Class AOut
    Inherits BinData

    Public fmagic As UShort, tsize As UShort, dsize As UShort, bsize As UShort,
        ssize As UShort, entry, pad As UShort, relflg As UShort

    Private syms As New Dictionary(Of Integer, String)
    Private srcs As New Dictionary(Of Integer, String)

    Public Sub New(data As Byte())
        MyBase.New(data)
        Offset = 16

        fmagic = BitConverter.ToUInt16(data, 0)
        tsize = BitConverter.ToUInt16(data, 2)
        dsize = BitConverter.ToUInt16(data, 4)
        bsize = BitConverter.ToUInt16(data, 6)
        ssize = BitConverter.ToUInt16(data, 8)
        entry = BitConverter.ToUInt16(data, 10)
        pad = BitConverter.ToUInt16(data, 12)
        relflg = BitConverter.ToUInt16(data, 14)

        For i = Offset + tsize + dsize To data.Length - 1 Step 12
            Dim name = ReadText(data, i, 8)
            Dim type = BitConverter.ToUInt16(data, i + 8)
            Dim addr = BitConverter.ToUInt16(data, i + 10)
            If name.EndsWith(".o") Then
                If Not srcs.ContainsKey(addr) Then srcs.Add(addr, name)
            ElseIf type <> 2 Then
                If Not syms.ContainsKey(addr) Then syms.Add(addr, name)
            End If
        Next
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
        For i = 0 To tsize - 1
            Dim sym = GetSym(i)
            If sym <> "" Then tw.WriteLine("       " + sym)

            Dim op = Disassembler.Disassemble(Me, i)
            Dim len = 2
            If Not op Is Nothing Then len = op.Length
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
            If Not op Is Nothing Then
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

    Public Function GetSym$(pos%)
        Dim sym = ""
        If syms.ContainsKey(pos) Then sym = syms(pos) + ":"
        If srcs.ContainsKey(pos) Then
            If sym <> "" Then sym += " "
            sym += "(" + srcs(pos) + ")"
        End If
        Return sym
    End Function
End Class
