Public Class Operand
    Public Property Type%
    Public Property Reg%
    Public Property Dist%
    Public Property Length As UShort
    Public Property PC As UShort
    Public Property Size As UShort

    Public Sub New(t%, r%, bd As BinData, ad%, sz As UShort)
        Type = t
        Reg = r
        Size = sz
        PC = CUShort(ad)
        If t >= 6 Then
            Dist = bd.ReadInt16(ad)
            Length = 2
            PC += 2US
        ElseIf r = 7 AndAlso (t = 2 OrElse t = 3) Then
            Length = 2
        End If
    End Sub

    Public Overloads Function ToString$(bd As BinData)
        Dim r = RegNames(Reg)
        If Reg = 7 Then
            Select Case Type
                Case 0 : Return r + bd.GetReg(Reg)
                Case 1 : Return "(" + r + ")" + bd.GetValue(Reg, Size, 0, 0)
                Case 2 : Return "$" + bd.Enc(bd.ReadUInt16(PC))
                Case 3 : Return "*$" + bd.EncAddr(bd.ReadUInt16(PC))
                Case 4 : Return "-(" + r + ")" + bd.GetValue(Reg, Size, -Size, -Size)
                Case 5 : Return "*-(" + r + ")" + bd.GetPtr(Reg, Size, -Size, -Size)
                Case 6 : Return bd.EncAddr(CUShort((PC + Dist) And &HFFFF))
                Case 7 : Return "*" + bd.Enc(CUShort((PC + Dist) And &HFFFF))
            End Select
        Else
            Select Case Type
                Case 0 : Return r + bd.GetReg(Reg)
                Case 1 : Return "(" + r + ")" + bd.GetValue(Reg, Size, 0, 0)
                Case 2 : Return "(" + r + ")+" + bd.GetValue(Reg, Size, 0, Size)
                Case 3 : Return "*(" + r + ")+" + bd.GetPtr(Reg, Size, 0, Size)
                Case 4 : Return "-(" + r + ")" + bd.GetValue(Reg, Size, -Size, -Size)
                Case 5 : Return "*-(" + r + ")" + bd.GetPtr(Reg, Size, -Size, -Size)
                Case 6 : Return bd.GetRelative(Reg, Dist, PC - 2US) + bd.GetValue(Reg, Size, Dist, 0)
                Case 7 : Return "*" + bd.GetRelative(Reg, Dist, PC - 2US) + bd.GetPtr(Reg, Size, Dist, 0)
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Function GetAddress(vm As VM) As UShort
        If Reg = 7 Then
            Select Case Type
                Case 1 : Return PC
                Case 2 : Return CUShort(PC)
                Case 3 : Return vm.ReadUInt16(PC)
                Case 6 : Return CUShort(PC + Dist)
                Case 7 : Return vm.ReadUInt16(CUShort(PC + Dist))
            End Select
        Else
            Select Case Type
                Case 1 : Return vm.Regs(Reg)
                Case 2 : Return vm.GetInc(Reg, Size)
                Case 3 : Return vm.ReadUInt16(vm.GetInc(Reg, Size))
                Case 4 : Return vm.GetDec(Reg, Size)
                Case 5 : Return vm.ReadUInt16(vm.GetDec(Reg, Size))
                Case 6 : Return CUShort(vm.Regs(Reg) + Dist)
                Case 7 : Return vm.ReadUInt16(CUShort(vm.Regs(Reg) + Dist))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Function GetValue(vm As VM) As UShort
        If Reg = 7 Then
            Select Case Type
                Case 0 : Return PC
                Case 1, 2 : Return vm.ReadUInt16(PC)
                Case 3 : Return vm.ReadUInt16(vm.ReadUInt16(PC))
                Case 6 : Return vm.ReadUInt16(CUShort(PC + Dist))
                Case 7 : Return vm.ReadUInt16(vm.ReadUInt16(CUShort(PC + Dist)))
            End Select
        Else
            Select Case Type
                Case 0 : Return vm.Regs(Reg)
                Case 1 : Return vm.ReadUInt16(vm.Regs(Reg))
                Case 2 : Return vm.ReadUInt16(vm.GetInc(Reg, Size))
                Case 3 : Return vm.ReadUInt16(vm.ReadUInt16(vm.GetInc(Reg, Size)))
                Case 4 : Return vm.ReadUInt16(vm.GetDec(Reg, Size))
                Case 5 : Return vm.ReadUInt16(vm.ReadUInt16(vm.GetDec(Reg, Size)))
                Case 6 : Return vm.ReadUInt16(CUShort(vm.Regs(Reg) + Dist))
                Case 7 : Return vm.ReadUInt16(vm.ReadUInt16(CUShort(vm.Regs(Reg) + Dist)))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Sub SetValue(vm As VM, v As UShort)
        If Reg = 7 Then
            Select Case Type
                Case 0 : vm.Regs(Reg) = v : Return
                Case 1, 2 : vm.Write(PC, v) : Return
                Case 3 : vm.Write(vm.ReadUInt16(PC), v) : Return
                Case 6 : vm.Write(CUShort(PC + Dist), v) : Return
                Case 7 : vm.Write(vm.ReadUInt16(CUShort(PC + Dist)), v) : Return
            End Select
        Else
            Select Case Type
                Case 0 : vm.Regs(Reg) = v : Return
                Case 1 : vm.Write(vm.Regs(Reg), v) : Return
                Case 2 : vm.Write(vm.GetInc(Reg, Size), v) : Return
                Case 3 : vm.Write(vm.ReadUInt16(vm.GetInc(Reg, Size)), v) : Return
                Case 4 : vm.Write(vm.GetDec(Reg, Size), v) : Return
                Case 5 : vm.Write(vm.ReadUInt16(vm.GetDec(Reg, Size)), v) : Return
                Case 6 : vm.Write(CUShort(vm.Regs(Reg) + Dist), v) : Return
                Case 7 : vm.Write(vm.ReadUInt16(CUShort(vm.Regs(Reg) + Dist)), v) : Return
            End Select
        End If
        Throw New Exception("invalid operand")
    End Sub

    Public Function GetByte(vm As VM) As Byte
        If Reg = 7 Then
            Select Case Type
                Case 0 : Return CByte(PC And &HFF)
                Case 1, 2 : Return vm(PC)
                Case 3 : Return vm(vm.ReadUInt16(PC))
                Case 6 : Return vm(CUShort(PC + Dist))
                Case 7 : Return vm(vm.ReadUInt16(CUShort(PC + Dist)))
            End Select
        Else
            Select Case Type
                Case 0 : Return CByte(vm.Regs(Reg) And &HFF)
                Case 1 : Return vm(vm.Regs(Reg))
                Case 2 : Return vm(vm.GetInc(Reg, Size))
                Case 3 : Return vm(vm.ReadUInt16(vm.GetInc(Reg, Size)))
                Case 4 : Return vm(vm.GetDec(Reg, Size))
                Case 5 : Return vm(vm.ReadUInt16(vm.GetDec(Reg, Size)))
                Case 6 : Return vm(CUShort(vm.Regs(Reg) + Dist))
                Case 7 : Return vm(vm.ReadUInt16(CUShort(vm.Regs(Reg) + Dist)))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Sub SetByte(vm As VM, b As Byte)
        If Reg = 7 Then
            Select Case Type
                Case 0 : vm.Regs(Reg) = b : Return
                Case 1, 2 : vm(PC) = b : Return
                Case 3 : vm(vm.ReadUInt16(PC)) = b : Return
                Case 6 : vm(CUShort(PC + Dist)) = b : Return
                Case 7 : vm(vm.ReadUInt16(CUShort(PC + Dist))) = b : Return
            End Select
        Else
            Select Case Type
                Case 0 : vm.Regs(Reg) = b : Return
                Case 1 : vm(vm.Regs(Reg)) = b : Return
                Case 2 : vm(vm.GetInc(Reg, Size)) = b : Return
                Case 3 : vm(vm.ReadUInt16(vm.GetInc(Reg, Size))) = b : Return
                Case 4 : vm(vm.GetDec(Reg, Size)) = b : Return
                Case 5 : vm(vm.ReadUInt16(vm.GetDec(Reg, Size))) = b : Return
                Case 6 : vm(CUShort(vm.Regs(Reg) + Dist)) = b : Return
                Case 7 : vm(vm.ReadUInt16(CUShort(vm.Regs(Reg) + Dist))) = b : Return
            End Select
        End If
        Throw New Exception("invalid operand")
    End Sub
End Class
