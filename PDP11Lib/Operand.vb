Public Class Operand
    Public Property Type%
    Public Property Reg%
    Public Property Dist%
    Public Property Addr As UShort
    Public Property Length%

    Public Sub New(t%, r%, bd As BinData, ad%)
        Type = t
        Reg = r
        If t >= 6 Then
            Dist = bd.ReadInt16(ad)
            Length = 2
        ElseIf r = 7 AndAlso (t = 2 OrElse t = 3) Then
            Addr = CUShort(ad)
            Length = 2
        End If
    End Sub

    Public Overloads Function ToString$(bd As BinData, pc%)
        If Reg = 7 Then
            Select Case Type
                Case 2 : Return "$" + bd.Enc(bd(Addr))
                Case 3 : Return "*$" + bd.Enc(bd(Addr))
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

    Public Function GetValue(vm As VM) As UShort
        If Reg = 7 Then
            Select Case Type
                Case 2 : Return vm(Addr)
                Case 3 : Return vm.ReadUInt16(vm(Addr))
            End Select
        End If
        Select Case Type
            Case 0 : Return vm.Regs(Reg)
            Case 1 : Return vm.ReadUInt16(vm.Regs(Reg))
            Case 2 : Return vm.ReadUInt16Inc(Reg)
            Case 3 : Return vm.ReadUInt16(vm.ReadUInt16Inc(Reg))
            Case 4 : Return vm.ReadUInt16Dec(Reg)
            Case 5 : Return vm.ReadUInt16(vm.ReadUInt16Dec(Reg))
            Case 6 : Return vm.ReadUInt16(CUShort(vm.Regs(Reg) + Dist))
            Case 7 : Return vm.ReadUInt16(vm.ReadUInt16(CUShort(vm.Regs(Reg) + Dist)))
        End Select
        Throw New Exception("invalid operand")
    End Function

    Public Sub SetValue(vm As VM, v As UShort)
        If Reg = 7 Then
            Select Case Type
                Case 2 : vm.Write(Addr, v) : Return
                Case 3 : vm.Write(vm.ReadUInt16(vm(Addr)), v) : Return
            End Select
        End If
        Select Case Type
            Case 0 : vm.Regs(Reg) = v
            Case 1 : vm.Write(vm.Regs(Reg), v)
            Case 2 : vm.WriteInc(Reg, v)
            Case 3 : vm.Write(vm.ReadUInt16Inc(Reg), v)
            Case 4 : vm.WriteDec(Reg, v)
            Case 5 : vm.Write(vm.ReadUInt16Dec(Reg), v)
            Case 6 : vm.Write(CUShort(vm.Regs(Reg) + Dist), v)
            Case 7 : vm.Write(vm.ReadUInt16(CUShort(vm.Regs(Reg) + Dist)), v)
        End Select
    End Sub
End Class
