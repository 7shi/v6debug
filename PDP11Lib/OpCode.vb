Public Class OpCode
    Public Property Mnemonic$
    Public Property Length%

    Public Sub New(mnemonic$, length%)
        Me.Mnemonic = mnemonic
        Me.Length = length
    End Sub
End Class
