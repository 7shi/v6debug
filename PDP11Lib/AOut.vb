Imports System.IO

Public Class AOut
    Inherits BinData

    Public fmagic As UShort, tsize As UShort, dsize As UShort, bsize As UShort,
        ssize As UShort, entry, pad As UShort, relflg As UShort

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
    End Sub

    Public Sub Disassemble(iw As TextWriter)
        Dim opmagic = Disassembler.Disassemble(New Byte() {Data(0), Data(1)}, UseOct)
        iw.WriteLine("[{0:x4}] fmagic = {1}  {2}", 0, Enc0(fmagic), opmagic.Mnemonic)
        iw.WriteLine("[{0:x4}] tsize  = {1}", 2, Enc0(tsize))
        iw.WriteLine("[{0:x4}] dsize  = {1}", 4, Enc0(dsize))
        iw.WriteLine("[{0:x4}] bsize  = {1}", 6, Enc0(bsize))
        iw.WriteLine("[{0:x4}] ssize  = {1}", 8, Enc0(ssize))
        iw.WriteLine("[{0:x4}] entry  = {1}", 10, Enc0(entry))
        iw.WriteLine("[{0:x4}] pad    = {1}", 12, Enc0(pad))
        iw.WriteLine("[{0:x4}] relflg = {1}", 14, Enc0(relflg))
        iw.WriteLine()
        iw.WriteLine(".text")
        For i = 0 To tsize - 1
            Dim op = Disassembler.Disassemble(Me, i)
            Dim len = 2
            If Not op Is Nothing Then len = op.length
            Dim s = ReadUInt16(i)
            iw.Write("[{0:x4}] {1}: {2}", 16 + i, Enc0(CUShort(i)), Enc0(s))
            For j = 2 To 4 Step 2
                If j < len Then
                    iw.Write(" " + Enc0(ReadUInt16(i + j)))
                ElseIf UseOct Then
                    iw.Write("       ")
                Else
                    iw.Write("     ")
                End If
            Next
            iw.Write("  ")
            If Not op Is Nothing Then
                iw.Write(op.Mnemonic)
            Else
                iw.Write(Enc(s))
            End If
            iw.WriteLine()
            i += len - 1
        Next
    End Sub

    Public Function GetDisassemble$()
        Using sw = New StringWriter()
            Disassemble(sw)
            Return sw.ToString()
        End Using
    End Function
End Class
