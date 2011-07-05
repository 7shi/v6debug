Public Class Operand
    Public Property VM As VM
    Public Property Type%
    Public Property Reg%
    Public Property Dist%
    Public Property Length As UShort
    Public Property PC As UShort
    Public Property Size As UShort

    Public Sub New(vm As VM, t%, r%, bd As BinData, ad%, sz As UShort)
        Me.VM = vm
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
                Case 0 : Return r + bd.GetReg(Reg, PC)
                Case 1 : Return "(" + r + ")" + bd.GetValue(Reg, Size, 0, 0)
                Case 2 : Return "$" + bd.Enc(bd.ReadUInt16(PC))
                Case 3 : Return "*$" + bd.EncAddr(bd.ReadUInt16(PC))
                Case 6 : Return bd.EncAddr(CUShort((PC + Dist) And &HFFFF))
                Case 7 : Return "*" + bd.EncAddr(CUShort((PC + Dist) And &HFFFF))
            End Select
        Else
            Select Case Type
                Case 0 : Return r + bd.GetReg(Reg, PC)
                Case 1 : Return "(" + r + ")" + bd.GetValue(Reg, Size, 0, 0)
                Case 2 : Return "(" + r + ")+" + bd.GetValue(Reg, Size, 0, Size)
                Case 3 : Return "*(" + r + ")+" + bd.GetPtr(Reg, Size, 0, 2)
                Case 4 : Return "-(" + r + ")" + bd.GetValue(Reg, Size, -Size, -Size)
                Case 5 : Return "*-(" + r + ")" + bd.GetPtr(Reg, Size, -2, -2)
                Case 6 : Return bd.GetRelative(Reg, Dist, PC - 2US) + bd.GetValue(Reg, Size, Dist, 0)
                Case 7 : Return "*" + bd.GetRelative(Reg, Dist, PC - 2US) + bd.GetPtr(Reg, Size, Dist, 0)
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Function GetAddress() As UShort
        If Reg = 7 Then
            Select Case Type
                Case 1 : Return PC
                Case 2 : Return CUShort(PC)
                Case 3 : Return VM.ReadUInt16(PC)
                Case 6 : Return CUShort(PC + Dist)
                Case 7 : Return VM.ReadUInt16(CUShort(PC + Dist))
            End Select
        Else
            Select Case Type
                Case 1 : Return VM.Regs(Reg)
                Case 2 : Return VM.GetInc(Reg, 2)
                Case 3 : Return VM.ReadUInt16(VM.GetInc(Reg, 2))
                Case 4 : Return VM.GetDec(Reg, 2)
                Case 5 : Return VM.ReadUInt16(VM.GetDec(Reg, 2))
                Case 6 : Return CUShort((VM.Regs(Reg) + Dist) And &HFFFF)
                Case 7 : Return VM.ReadUInt16(CUShort((VM.Regs(Reg) + Dist) And &HFFFF))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Overridable Function GetValue() As UShort
        If Reg = 7 Then
            Select Case Type
                Case 0 : Return PC
                Case 1, 2 : Return VM.ReadUInt16(PC)
                Case 3 : Return VM.ReadUInt16(VM.ReadUInt16(PC))
                Case 6 : Return VM.ReadUInt16(CUShort(PC + Dist))
                Case 7 : Return VM.ReadUInt16(VM.ReadUInt16(CUShort(PC + Dist)))
            End Select
        Else
            Select Case Type
                Case 0 : Return VM.Regs(Reg)
                Case 1 : Return VM.ReadUInt16(VM.Regs(Reg))
                Case 2 : Return VM.ReadUInt16(VM.GetInc(Reg, 2))
                Case 3 : Return VM.ReadUInt16(VM.ReadUInt16(VM.GetInc(Reg, 2)))
                Case 4 : Return VM.ReadUInt16(VM.GetDec(Reg, 2))
                Case 5 : Return VM.ReadUInt16(VM.ReadUInt16(VM.GetDec(Reg, 2)))
                Case 6 : Return VM.ReadUInt16(CUShort((VM.Regs(Reg) + Dist) And &HFFFF))
                Case 7 : Return VM.ReadUInt16(VM.ReadUInt16(CUShort((VM.Regs(Reg) + Dist) And &HFFFF)))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Overridable Function GetByte() As Byte
        Dim size = If(Reg = 6, 2, 1)
        If Reg = 7 Then
            Select Case Type
                Case 0 : Return CByte(PC And &HFF)
                Case 1, 2 : Return VM(PC)
                Case 3 : Return VM(VM.ReadUInt16(PC))
                Case 6 : Return VM(CUShort(PC + Dist))
                Case 7 : Return VM(VM.ReadUInt16(CUShort(PC + Dist)))
            End Select
        Else
            Select Case Type
                Case 0 : Return CByte(VM.Regs(Reg) And &HFF)
                Case 1 : Return VM(VM.Regs(Reg))
                Case 2 : Return VM(VM.GetInc(Reg, size))
                Case 3 : Return VM(VM.ReadUInt16(VM.GetInc(Reg, 2)))
                Case 4 : Return VM(VM.GetDec(Reg, size))
                Case 5 : Return VM(VM.ReadUInt16(VM.GetDec(Reg, 2)))
                Case 6 : Return VM(CUShort((VM.Regs(Reg) + Dist) And &HFFFF))
                Case 7 : Return VM(VM.ReadUInt16(CUShort((VM.Regs(Reg) + Dist) And &HFFFF)))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function
End Class

Public Class DestOperand
    Inherits Operand

    Public Sub New(vm As VM, t%, r%, bd As BinData, ad%, sz As UShort)
        MyBase.New(vm, t, r, bd, ad, sz)
    End Sub

    Public Overrides Function GetValue() As UShort
        If Reg = 7 Then
            Return MyBase.GetValue()
        Else
            Select Case Type
                Case 0 : Return VM.Regs(Reg)
                Case 1, 2 : Return VM.ReadUInt16(VM.Regs(Reg))
                Case 3 : Return VM.ReadUInt16(VM.ReadUInt16(VM.Regs(Reg)))
                Case 4 : Return VM.ReadUInt16(VM.Regs(Reg) - 2)
                Case 5 : Return VM.ReadUInt16(VM.ReadUInt16(VM.Regs(Reg) - 2))
                Case 6 : Return VM.ReadUInt16(CUShort((VM.Regs(Reg) + Dist) And &HFFFF))
                Case 7 : Return VM.ReadUInt16(VM.ReadUInt16(CUShort((VM.Regs(Reg) + Dist) And &HFFFF)))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Sub SetValue(v As UShort)
        If Reg = 7 Then
            Select Case Type
                Case 0 : PC = v : Return
                Case 1, 2 : VM.Write(PC, v) : Return
                Case 3 : VM.Write(VM.ReadUInt16(PC), v) : Return
                Case 6 : VM.Write(CUShort(PC + Dist), v) : Return
                Case 7 : VM.Write(VM.ReadUInt16(CUShort(PC + Dist)), v) : Return
            End Select
        Else
            Select Case Type
                Case 0 : VM.Regs(Reg) = v : Return
                Case 1 : VM.Write(VM.Regs(Reg), v) : Return
                Case 2 : VM.Write(VM.GetInc(Reg, 2), v) : Return
                Case 3 : VM.Write(VM.ReadUInt16(VM.GetInc(Reg, 2)), v) : Return
                Case 4 : VM.Write(VM.GetDec(Reg, 2), v) : Return
                Case 5 : VM.Write(VM.ReadUInt16(VM.GetDec(Reg, 2)), v) : Return
                Case 6 : VM.Write(CUShort((VM.Regs(Reg) + Dist) And &HFFFF), v) : Return
                Case 7 : VM.Write(VM.ReadUInt16(CUShort((VM.Regs(Reg) + Dist) And &HFFFF)), v) : Return
            End Select
        End If
        Throw New Exception("invalid operand")
    End Sub

    Public Overrides Function GetByte() As Byte
        Dim size = If(Reg = 6, 2, 1)
        If Reg = 7 Then
            Return MyBase.GetByte()
        Else
            Select Case Type
                Case 0 : Return CByte(VM.Regs(Reg) And &HFF)
                Case 1, 2 : Return VM(VM.Regs(Reg))
                Case 3 : Return VM(VM.ReadUInt16(VM.Regs(Reg)))
                Case 4 : Return VM(VM.Regs(Reg) - size)
                Case 5 : Return VM(VM.ReadUInt16(VM.Regs(Reg) - 2))
                Case 6 : Return VM(CUShort((VM.Regs(Reg) + Dist) And &HFFFF))
                Case 7 : Return VM(VM.ReadUInt16(CUShort((VM.Regs(Reg) + Dist) And &HFFFF)))
            End Select
        End If
        Throw New Exception("invalid operand")
    End Function

    Public Sub SetByte(b As Byte)
        Dim size = If(Reg = 6, 2, 1)
        If Reg = 7 Then
            Select Case Type
                Case 0 : PC = If(b < &H80, b, CUShort((b - &H100) And &HFFFF)) : Return
                Case 1, 2 : VM(PC) = b : Return
                Case 3 : VM(VM.ReadUInt16(PC)) = b : Return
                Case 6 : VM(CUShort(PC + Dist)) = b : Return
                Case 7 : VM(VM.ReadUInt16(CUShort(PC + Dist))) = b : Return
            End Select
        Else
            Select Case Type
                Case 0 : VM.Regs(Reg) = If(b < &H80, b, CUShort((b - &H100) And &HFFFF)) : Return
                Case 1 : VM(VM.Regs(Reg)) = b : Return
                Case 2 : VM(VM.GetInc(Reg, size)) = b : Return
                Case 3 : VM(VM.ReadUInt16(VM.GetInc(Reg, 2))) = b : Return
                Case 4 : VM(VM.GetDec(Reg, size)) = b : Return
                Case 5 : VM(VM.ReadUInt16(VM.GetDec(Reg, 2))) = b : Return
                Case 6 : VM(CUShort((VM.Regs(Reg) + Dist) And &HFFFF)) = b : Return
                Case 7 : VM(VM.ReadUInt16((CUShort(VM.Regs(Reg) + Dist) And &HFFFF))) = b : Return
            End Select
        End If
        Throw New Exception("invalid operand")
    End Sub
End Class
