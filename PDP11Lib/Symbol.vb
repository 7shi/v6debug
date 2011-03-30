Public Class Symbol
    Public Property Name$
    Public Property Type%
    Public Property Address%
    Public Property Source As Symbol

    Public ReadOnly Property IsObject As Boolean
        Get
            Return Type = &H1F
        End Get
    End Property

    Public Sub New(data As Byte(), pos%)
        Name = ReadText(data, pos, 8)
        Type = BitConverter.ToUInt16(data, pos + 8)
        Address = BitConverter.ToUInt16(data, pos + 10)
    End Sub

    Public Overrides Function ToString() As String
        Dim n = If(IsObject, "(" + Name + ")", Name + ":")
        Return If(Source Is Nothing, n, n + " " + Source.ToString)
    End Function
End Class
