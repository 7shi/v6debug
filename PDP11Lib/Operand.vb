﻿Public Class Operand
    Public Property Type%
    Public Property Reg%
    Public Property Length As UShort

    Public Sub New(t%, r%)
        Type = t
        Reg = r
        If t >= 6 OrElse ((t = 2 OrElse t = 3) AndAlso r = 7) Then Length = 2
    End Sub

    Public Shadows Function ToString$(bd As BinData, pc%, size%)
        Dim dist = 0
        If Type >= 6 Then dist = bd.ReadInt16(pc) : pc += 2US
        Dim r = RegNames(Reg)
        If Reg = 7 Then
            Select Case Type
                Case 0 : Return r + bd.GetReg(Reg, CUShort(pc))
                Case 1 : Return "(" + r + ")" + bd.GetValue(Reg, size, 0, 0)
                Case 2 : Return "$" + bd.Enc(bd.ReadUInt16(pc))
                Case 3 : Return "*$" + bd.EncAddr(bd.ReadUInt16(pc))
                Case 6 : Return bd.EncAddr(CUShort((pc + dist) And &HFFFF))
                Case 7 : Return "*" + bd.EncAddr(CUShort((pc + dist) And &HFFFF))
            End Select
        Else
            Select Case Type
                Case 0 : Return r + bd.GetReg(Reg, CUShort(pc))
                Case 1 : Return "(" + r + ")" + bd.GetValue(Reg, size, 0, 0)
                Case 2 : Return "(" + r + ")+" + bd.GetValue(Reg, size, 0, size)
                Case 3 : Return "*(" + r + ")+" + bd.GetPtr(Reg, size, 0, 2)
                Case 4 : Return "-(" + r + ")" + bd.GetValue(Reg, size, -size, -size)
                Case 5 : Return "*-(" + r + ")" + bd.GetPtr(Reg, size, -2, -2)
                Case 6 : Return bd.GetRelative(Reg, dist, pc - 2US) + bd.GetValue(Reg, size, dist, 0)
                Case 7 : Return "*" + bd.GetRelative(Reg, dist, pc - 2US) + bd.GetPtr(Reg, size, dist, 0)
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Function GetAddress(vm As VM) As UShort
        Dim dist = If(Type < 6, 0, vm.ReadInt16(vm.GetInc(7, 2)))
        Select Case Type
            Case 1 : Return vm.Regs(Reg)
            Case 2 : Return vm.GetInc(Reg, 2)
            Case 3 : Return vm.ReadUInt16(vm.GetInc(Reg, 2))
            Case 4 : Return vm.GetDec(Reg, 2)
            Case 5 : Return vm.ReadUInt16(vm.GetDec(Reg, 2))
            Case 6 : Return CUShort((vm.Regs(Reg) + dist) And &HFFFF)
            Case 7 : Return vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF))
        End Select
        Throw New Exception("invalid operand")
    End Function

    Public Overridable Function GetValue(vm As VM) As UShort
        Dim dist = If(Type < 6, 0, vm.ReadInt16(vm.GetInc(7, 2)))
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
        Throw New Exception("invalid operand")
    End Function

    Public Overridable Function GetByte(vm As VM) As Byte
        Dim dist = If(Type < 6, 0, vm.ReadInt16(vm.GetInc(7, 2)))
        Dim size = If(Reg < 6, 1, 2)
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
        Throw New Exception("invalid operand")
    End Function
End Class

Public Class DestOperand
    Inherits Operand

    Public Sub New(t%, r%)
        MyBase.New(t, r)
    End Sub

    Public Overrides Function GetValue(vm As VM) As UShort
        Dim dist = If(Type < 6, 0, If(Reg = 7, 2, 0) + vm.ReadInt16(vm.PC))
        Select Case Type
            Case 0 : Return vm.Regs(Reg)
            Case 1, 2 : Return vm.ReadUInt16(vm.Regs(Reg))
            Case 3 : Return vm.ReadUInt16(vm.ReadUInt16(vm.Regs(Reg)))
            Case 4 : Return vm.ReadUInt16(vm.Regs(Reg) - 2)
            Case 5 : Return vm.ReadUInt16(vm.ReadUInt16(vm.Regs(Reg) - 2))
            Case 6 : Return vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF))
            Case 7 : Return vm.ReadUInt16(vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF)))
        End Select
        Throw New Exception("invalid operand")
    End Function

    Public Sub SetValue(vm As VM, v As UShort)
        Dim dist = If(Type < 6, 0, vm.ReadInt16(vm.GetInc(7, 2)))
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
        Throw New Exception("invalid operand")
    End Sub

    Public Overrides Function GetByte(vm As VM) As Byte
        Dim dist = If(Type < 6, 0, If(Reg = 7, 2, 0) + vm.ReadInt16(vm.PC))
        Dim size = If(Reg < 6, 1, 2)
        Select Case Type
            Case 0 : Return CByte(vm.Regs(Reg) And &HFF)
            Case 1, 2 : Return vm(vm.Regs(Reg))
            Case 3 : Return vm(vm.ReadUInt16(vm.Regs(Reg)))
            Case 4 : Return vm(vm.Regs(Reg) - size)
            Case 5 : Return vm(vm.ReadUInt16(vm.Regs(Reg) - 2))
            Case 6 : Return vm(CUShort((vm.Regs(Reg) + dist) And &HFFFF))
            Case 7 : Return vm(vm.ReadUInt16(CUShort((vm.Regs(Reg) + dist) And &HFFFF)))
        End Select
        Throw New Exception("invalid operand")
    End Function

    Public Sub SetByte(vm As VM, b As Byte)
        Dim dist = If(Type < 6, 0, vm.ReadInt16(vm.GetInc(7, 2)))
        Dim size = If(Reg < 6, 1, 2)
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
        Throw New Exception("invalid operand")
    End Sub
End Class
