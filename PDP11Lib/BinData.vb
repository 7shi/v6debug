Imports System.IO
Imports System.Text

Public Class BinData
    Public Property Offset%
    Public Property Data As Byte()
    Public UseOct As Boolean

    Public Sub New(data As Byte())
        Me.Data = data
    End Sub

    Public Sub New(size%)
        ReDim Data(size - 1)
    End Sub

    Default Public Property Indexer(pos%) As Byte
        Get
            Return Data(Offset + pos)
        End Get
        Set(value As Byte)
            Data(Offset + pos) = value
        End Set
    End Property

    Public Function ReadUInt16(pos%) As UShort
        Return BitConverter.ToUInt16(Data, Offset + pos)
    End Function

    Public Function ReadInt16(pos%) As Short
        Return BitConverter.ToInt16(Data, Offset + pos)
    End Function

    Public Sub Write(pos%, v As UShort)
        Dim buf = BitConverter.GetBytes(v)
        Array.Copy(buf, 0, Data, pos, buf.Length)
    End Sub

    Public Sub Write(pos%, ParamArray vs As UShort())
        For Each v In vs
            Dim buf = BitConverter.GetBytes(v)
            Array.Copy(buf, 0, Data, pos, buf.Length)
            pos += 2
        Next
    End Sub

    Public Function Dump(start%, end%) As DumpEntry()
        Dim list = New List(Of DumpEntry)
        For ad = start To [end] Step 16
            Dim de = New DumpEntry
            de.Addr = String.Format("{0:X4}", ad)
            Using sw = New StringWriter
                Dim sb = New StringBuilder
                For j = 0 To 15
                    If ad + j <= [end] Then
                        If j = 8 Then
                            sw.Write(" - ")
                        ElseIf j > 0 Then
                            sw.Write(" ")
                        End If
                        Dim b = Data(ad + j)
                        sw.Write("{0:X2}", b)
                        sb.Append(If(32 < b AndAlso b < 128, ChrW(b), CChar(".")))
                    Else
                        If j = 8 Then sw.Write("  ")
                        sw.Write("   ")
                    End If
                Next
                de.Dump = sw.ToString
                de.Ascii = sb.ToString
            End Using
            list.Add(de)
        Next
        Return list.ToArray
    End Function

    Public Function Enc0$(v As UShort)
        Return If(UseOct, Oct(v, 6), v.ToString("x4"))
    End Function

    Public Function Enc0$(v As Byte)
        Return If(UseOct, Oct(v, 3), v.ToString("x2"))
    End Function

    Public Function Enc$(v As UShort)
        Return If(UseOct, Oct(v, 7), "0x" + v.ToString("x4"))
    End Function

    Public Function Enc$(v As Byte)
        Return If(UseOct, Oct(v, 4), "0x" + v.ToString("x2"))
    End Function

    Public Function GetOffset(pos%) As UShort
        Dim d = CInt(Me(pos - 2))
        If d >= 128 Then d -= 256
        Return CUShort((pos + d * 2) And &HFFFF)
    End Function

    Public Overridable Function EncAddr$(addr As UShort)
        Return Enc(addr)
    End Function

    Public Overridable Function GetRelative$(r%, d%, ad%)
        Dim sign = If(d < 0, "-", "")
        Dim da = CUShort(Math.Abs(d))
        Dim dd = If(da < 10, d.ToString, If(d < 0, "-", "") + Enc(da))
        Return dd + "(" + RegNames(r) + ")"
    End Function
End Class

Public Class DumpEntry
    Public Property Addr$
    Public Property Dump$
    Public Property Ascii$
End Class
