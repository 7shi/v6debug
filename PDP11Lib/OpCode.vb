Partial Public Class OpCode
    Public Property Length As UShort

    Private val%
    Private disasm As Func(Of BinData, Integer, String)

    Public Sub New(value%)
        val = value
        SetInst()
    End Sub

    Public Function Disassemble$(bd As BinData, pos%)
        Return If(disasm Is Nothing, Nothing, disasm(bd, pos))
    End Function
End Class
