Public Class Operand
    Public Property Type%
    Public Property Reg%
    Public Property Length As UShort
    Public Property Size As UShort

    Public Sub New(t%, r%, sz As UShort)
        Type = t
        Reg = r
        Size = sz
        If t >= 6 OrElse ((t = 2 OrElse t = 3) AndAlso r = 7) Then Length = 2
    End Sub

    Public Shadows Function ToString$(bd As BinData, pc%)
        Dim dist = 0
        If Type >= 6 Then dist = bd.ReadInt16(pc) : pc += 2US
        Dim r = RegNames(Reg)
        If Reg = 7 Then
            Select Case Type
                Case 0 : Return r + bd.GetReg(Reg, CUShort(pc))
                Case 1 : Return "(" + r + ")" + bd.GetValue(Reg, Size, 0, 0)
                Case 2 : Return "$" + bd.Enc(bd.ReadUInt16(pc))
                Case 3 : Return "*$" + bd.EncAddr(bd.ReadUInt16(pc))
                Case 6 : Return bd.EncAddr(CUShort((pc + dist) And &HFFFF))
                Case 7 : Return "*" + bd.EncAddr(CUShort((pc + dist) And &HFFFF))
            End Select
        Else
            Select Case Type
                Case 0 : Return r + bd.GetReg(Reg, CUShort(pc))
                Case 1 : Return "(" + r + ")" + bd.GetValue(Reg, Size, 0, 0)
                Case 2 : Return "(" + r + ")+" + bd.GetValue(Reg, Size, 0, Size)
                Case 3 : Return "*(" + r + ")+" + bd.GetPtr(Reg, Size, 0, 2)
                Case 4 : Return "-(" + r + ")" + bd.GetValue(Reg, Size, -Size, -Size)
                Case 5 : Return "*-(" + r + ")" + bd.GetPtr(Reg, Size, -2, -2)
                Case 6 : Return bd.GetRelative(Reg, dist, pc - 2US) + bd.GetValue(Reg, Size, dist, 0)
                Case 7 : Return "*" + bd.GetRelative(Reg, dist, pc - 2US) + bd.GetPtr(Reg, Size, dist, 0)
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Function GetAddress(vm As VM, pc%) As UShort
        Dim dist = 0
        If Type >= 6 Then dist = vm.ReadInt16(pc) : pc += 2US
        If Reg = 7 Then
            Select Case Type
                Case 1, 2 : Return CUShort(pc)
                Case 3 : Return vm.ReadUInt16(pc)
                Case 6 : Return CUShort(pc + dist)
                Case 7 : Return vm.ReadUInt16(CUShort(pc + dist))
            End Select
        Else
            Select Case Type
                Case 1 : Return vm.Regs(Reg)
                Case 2 : Return vm.GetInc(Reg, 2)
                Case 3 : Return vm.ReadUInt16(vm.GetInc(Reg, 2))
                Case 4 : Return vm.GetDec(Reg, 2)
                Case 5 : Return vm.ReadUInt16(vm.GetDec(Reg, 2))
                Case 6 : Return CUShort((vm.Regs(Reg) + dist) And &HFFFF)
                Case 7 : Return vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Overridable Function GetValue(vm As VM, pc%) As UShort
        Dim dist = 0
        If Type >= 6 Then dist = vm.ReadInt16(pc) : pc += 2US
        If Reg = 7 Then
            Select Case Type
                Case 0 : Return CUShort(pc)
                Case 1, 2 : Return vm.ReadUInt16(pc)
                Case 3 : Return vm.ReadUInt16(vm.ReadUInt16(pc))
                Case 6 : Return vm.ReadUInt16(CUShort(pc + dist))
                Case 7 : Return vm.ReadUInt16(vm.ReadUInt16(CUShort(pc + dist)))
            End Select
        Else
            Select Case Type
                Case 0 : Return vm.Regs(Reg)
                Case 1 : Return vm.ReadUInt16(vm.Regs(Reg))
                Case 2 : Return vm.ReadUInt16(vm.GetInc(Reg, 2))
                Case 3 : Return vm.ReadUInt16(vm.ReadUInt16(vm.GetInc(Reg, 2)))
                Case 4 : Return vm.ReadUInt16(vm.GetDec(Reg, 2))
                Case 5 : Return vm.ReadUInt16(vm.ReadUInt16(vm.GetDec(Reg, 2)))
                Case 6 : Return vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF))
                Case 7 : Return vm.ReadUInt16(vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF)))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Overridable Function GetByte(vm As VM, pc%) As Byte
        Dim dist = 0
        If Type >= 6 Then dist = vm.ReadInt16(pc) : pc += 2US
        Dim size = If(Reg = 6, 2, 1)
        If Reg = 7 Then
            Select Case Type
                Case 0 : Return CByte(pc And &HFF)
                Case 1, 2 : Return vm(pc)
                Case 3 : Return vm(vm.ReadUInt16(pc))
                Case 6 : Return vm(CUShort(pc + dist))
                Case 7 : Return vm(vm.ReadUInt16(CUShort(pc + dist)))
            End Select
        Else
            Select Case Type
                Case 0 : Return CByte(vm.Regs(Reg) And &HFF)
                Case 1 : Return vm(vm.Regs(Reg))
                Case 2 : Return vm(vm.GetInc(Reg, size))
                Case 3 : Return vm(vm.ReadUInt16(vm.GetInc(Reg, 2)))
                Case 4 : Return vm(vm.GetDec(Reg, size))
                Case 5 : Return vm(vm.ReadUInt16(vm.GetDec(Reg, 2)))
                Case 6 : Return vm(CUShort((vm.Regs(Reg) + dist) And &HFFFF))
                Case 7 : Return vm(vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF)))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function
End Class

Public Class DestOperand
    Inherits Operand

    Public Sub New(t%, r%, sz As UShort)
        MyBase.New(t, r, sz)
    End Sub

    Public Overrides Function GetValue(vm As VM, pc%) As UShort
        If Reg = 7 Then
            Return MyBase.GetValue(vm, pc)
        Else
            Dim dist = 0
            If Type >= 6 Then dist = vm.ReadInt16(pc) : pc += 2US
            Select Case Type
                Case 0 : Return vm.Regs(Reg)
                Case 1, 2 : Return vm.ReadUInt16(vm.Regs(Reg))
                Case 3 : Return vm.ReadUInt16(vm.ReadUInt16(vm.Regs(Reg)))
                Case 4 : Return vm.ReadUInt16(vm.Regs(Reg) - 2)
                Case 5 : Return vm.ReadUInt16(vm.ReadUInt16(vm.Regs(Reg) - 2))
                Case 6 : Return vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF))
                Case 7 : Return vm.ReadUInt16(vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF)))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Sub SetValue(vm As VM, pc%, v As UShort)
        Dim dist = 0
        If Type >= 6 Then dist = vm.ReadInt16(pc) : pc += 2US
        If Reg = 7 Then
            Select Case Type
                Case 0 : pc = v : Return
                Case 1, 2 : vm.Write(pc, v) : Return
                Case 3 : vm.Write(vm.ReadUInt16(pc), v) : Return
                Case 6 : vm.Write(CUShort(pc + dist), v) : Return
                Case 7 : vm.Write(vm.ReadUInt16(CUShort(pc + dist)), v) : Return
            End Select
        Else
            Select Case Type
                Case 0 : vm.Regs(Reg) = v : Return
                Case 1 : vm.Write(vm.Regs(Reg), v) : Return
                Case 2 : vm.Write(vm.GetInc(Reg, 2), v) : Return
                Case 3 : vm.Write(vm.ReadUInt16(vm.GetInc(Reg, 2)), v) : Return
                Case 4 : vm.Write(vm.GetDec(Reg, 2), v) : Return
                Case 5 : vm.Write(vm.ReadUInt16(vm.GetDec(Reg, 2)), v) : Return
                Case 6 : vm.Write(CUShort((vm.Regs(Reg) + dist) And &HFFFF), v) : Return
                Case 7 : vm.Write(vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF)), v) : Return
            End Select
        End If
        Throw New Exception("invalid operand")
    End Sub

    Public Overrides Function GetByte(vm As VM, pc%) As Byte
        Dim size = If(Reg = 6, 2, 1)
        If Reg = 7 Then
            Return MyBase.GetByte(vm, pc)
        Else
            Dim dist = 0
            If Type >= 6 Then dist = vm.ReadInt16(pc) : pc += 2US
            Select Case Type
                Case 0 : Return CByte(vm.Regs(Reg) And &HFF)
                Case 1, 2 : Return vm(vm.Regs(Reg))
                Case 3 : Return vm(vm.ReadUInt16(vm.Regs(Reg)))
                Case 4 : Return vm(vm.Regs(Reg) - size)
                Case 5 : Return vm(vm.ReadUInt16(vm.Regs(Reg) - 2))
                Case 6 : Return vm(CUShort((vm.Regs(Reg) + dist) And &HFFFF))
                Case 7 : Return vm(vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF)))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Sub SetByte(vm As VM, pc%, b As Byte)
        Dim dist = 0
        If Type >= 6 Then dist = vm.ReadInt16(pc) : pc += 2US
        Dim size = If(Reg = 6, 2, 1)
        If Reg = 7 Then
            Select Case Type
                Case 0 : pc = If(b < &H80, b, CUShort((b - &H100) And &HFFFF)) : Return
                Case 1, 2 : vm(pc) = b : Return
                Case 3 : vm(vm.ReadUInt16(pc)) = b : Return
                Case 6 : vm(CUShort(pc + dist)) = b : Return
                Case 7 : vm(vm.ReadUInt16(CUShort(pc + dist))) = b : Return
            End Select
        Else
            Select Case Type
                Case 0 : vm.Regs(Reg) = If(b < &H80, b, CUShort((b - &H100) And &HFFFF)) : Return
                Case 1 : vm(vm.Regs(Reg)) = b : Return
                Case 2 : vm(vm.GetInc(Reg, size)) = b : Return
                Case 3 : vm(vm.ReadUInt16(vm.GetInc(Reg, 2))) = b : Return
                Case 4 : vm(vm.GetDec(Reg, size)) = b : Return
                Case 5 : vm(vm.ReadUInt16(vm.GetDec(Reg, 2))) = b : Return
                Case 6 : vm(CUShort((vm.Regs(Reg) + dist) And &HFFFF)) = b : Return
                Case 7 : vm(vm.ReadUInt16((CUShort(vm.Regs(Reg) + dist) And &HFFFF))) = b : Return
            End Select
        End If
        Throw New Exception("invalid operand")
    End Sub
End Class
