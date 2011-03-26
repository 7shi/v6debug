Public Class Operand
    Public Property Type%
    Public Property Reg%
    Public Property Dist%
    Public Property Value%
    Public Property Length%

    Public Sub New(t%, r%, bd As BinData, pos%)
        Type = t
        Reg = r
        If t >= 6 Then
            Dist = bd.ReadInt16(pos)
            Length = 2
        ElseIf r = 7 AndAlso (t = 2 OrElse t = 3) Then
            Value = bd.ReadUInt16(pos)
            Length = 2
        End If
    End Sub

    Public Overloads Function ToString$(bd As BinData, pc%)
        If Reg = 7 Then
            Select Case Type
                Case 2 : Return "$" + bd.Enc(CUShort(Value And &HFFFF))
                Case 3 : Return "*$" + bd.Enc(CUShort(Value And &HFFFF))
                Case 6 : Return bd.Enc(CUShort(pc + Dist))
                Case 7 : Return "*" + bd.Enc(CUShort(pc + Dist))
            End Select
        End If
        Dim r = RegNames(Reg)
        Dim sign = If(Dist < 0, "-", "")
        Dim v3a = Math.Abs(Dist)
        Dim dd = Dist.ToString
        If v3a >= 10 Then dd = sign + bd.Enc(CUShort(v3a))
        Select Case Type
            Case 0 : Return r
            Case 1 : Return "(" + r + ")"
            Case 2 : Return "(" + r + ")+"
            Case 3 : Return "*(" + r + ")+"
            Case 4 : Return "-(" + r + ")"
            Case 5 : Return "*-(" + r + ")"
            Case 6 : Return dd + "(" + r + ")"
            Case 7 : Return "*" + dd + "(" + r + ")"
        End Select
        Throw New Exception("invalid argument")
    End Function
End Class
