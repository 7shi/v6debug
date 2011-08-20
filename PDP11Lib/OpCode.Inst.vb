Imports System.Text

Partial Public Class OpCode
    Private Sub SetInst()
        Select Case val >> 12
            Case 0
                Set0()
            Case 1
                SetSrcDst("mov", 2) ' MOVe
                exec = Sub(vm)
                           Dim v = src.GetValue(vm)
                           dst.SetValue(vm, v)
                           vm.SetFlags(v = 0, ConvShort(v) < 0, vm.C, False)
                       End Sub
            Case 2
                SetSrcDst("cmp", 2) ' CoMPare
                exec = Sub(vm)
                           Dim vsrc = src.GetValue(vm)
                           Dim vdst = dst.GetValue(vm)
                           Dim v = CInt(ConvShort(vsrc)) - CInt(ConvShort(vdst))
                           vm.SetFlags(v = 0, v < 0, vsrc < vdst, v < -&H8000)
                       End Sub
            Case 3
                SetSrcDst("bit", 2) ' BIt Test
                exec = Sub(vm)
                           Dim v = src.GetValue(vm) And dst.GetValue(vm)
                           vm.SetFlags(v = 0, (v And &H8000) <> 0, vm.C, False)
                       End Sub
            Case 4
                SetSrcDst("bic", 2) ' BIt Clear
                exec = Sub(vm)
                           Dim v = (Not src.GetValue(vm)) And dst.PeekValue(vm)
                           dst.SetValue(vm, v)
                           vm.SetFlags(v = 0, (v And &H8000) <> 0, vm.C, False)
                       End Sub
            Case 5
                SetSrcDst("bis", 2) ' BIt Set
                exec = Sub(vm)
                           Dim v = src.GetValue(vm) Or dst.PeekValue(vm)
                           dst.SetValue(vm, v)
                           vm.SetFlags(v = 0, (v And &H8000) <> 0, vm.C, False)
                       End Sub
            Case 6
                SetSrcDst("add", 2) ' ADD
                exec = Sub(vm)
                           Dim vsrc = src.GetValue(vm)
                           Dim vdst = dst.PeekValue(vm)
                           Dim v = CInt(ConvShort(vsrc)) + CInt(ConvShort(vdst))
                           dst.SetValue(vm, CUShort(v And &HFFFF))
                           vm.SetFlags(v = 0, v < 0, CInt(vsrc) + CInt(vdst) >= &H10000, v >= &H8000)
                       End Sub
            Case 7
                Set7()
            Case &O10
                Set10()
            Case &O11
                SetSrcDst("movb", 1) ' MOVe Byte
                exec = Sub(vm)
                           Dim v = src.GetByte(vm)
                           dst.SetByte(vm, v)
                           vm.SetFlags(v = 0, ConvSByte(v) < 0, vm.C, False)
                       End Sub
            Case &O12
                SetSrcDst("cmpb", 1) ' CoMPare Byte
                exec = Sub(vm)
                           Dim vsrc = src.GetByte(vm)
                           Dim vdst = dst.GetByte(vm)
                           Dim v = CInt(ConvSByte(vsrc)) - CInt(ConvSByte(vdst))
                           vm.SetFlags(v = 0, v < 0, vsrc < vdst, v < -&H80)
                       End Sub
            Case &O13
                SetSrcDst("bitb", 1) ' BIt Test Byte
                exec = Sub(vm)
                           Dim v = src.GetByte(vm) And dst.GetByte(vm)
                           vm.SetFlags(v = 0, (v And &H80) <> 0, vm.C, False)
                       End Sub
            Case &O14
                SetSrcDst("bicb", 1) ' BIt Clear Byte
                exec = Sub(vm)
                           Dim v = (Not src.GetByte(vm)) And dst.PeekByte(vm)
                           dst.SetByte(vm, v)
                           vm.SetFlags(v = 0, (v And &H80) <> 0, vm.C, False)
                       End Sub
            Case &O15
                SetSrcDst("bisb", 1) ' BIt Set Byte
                exec = Sub(vm)
                           Dim v = src.GetByte(vm) Or dst.PeekByte(vm)
                           dst.SetByte(vm, v)
                           vm.SetFlags(v = 0, (v And &H80) <> 0, vm.C, False)
                       End Sub
            Case &O16
                SetSrcDst("sub", 2) ' SUBtract
                exec = Sub(vm)
                           Dim vsrc = src.GetValue(vm)
                           Dim vdst = dst.PeekValue(vm)
                           Dim v = CInt(ConvShort(vdst)) - CInt(ConvShort(vsrc))
                           dst.SetValue(vm, CUShort(v And &HFFFF))
                           vm.SetFlags(v = 0, v < 0, vdst < vsrc, v < -&H8000)
                       End Sub
            Case &O17
                Set17()
        End Select
    End Sub

    Private Sub Set0()
        Select Case val >> 8
            Case 1
                SetOffset("br") ' BRanch
                exec = Sub(vm)
                           vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case 2
                SetOffset("bne") ' Branch if Not Equal
                exec = Sub(vm)
                           If Not vm.Z Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case 3
                SetOffset("beq") ' Branch if EQual
                exec = Sub(vm)
                           If vm.Z Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case 4
                SetOffset("bge") ' Branch if Greater or Equal
                exec = Sub(vm)
                           If Not (vm.N Xor vm.V) Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case 5
                SetOffset("blt") ' Branch if Less Than
                exec = Sub(vm)
                           If vm.N Xor vm.V Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case 6
                SetOffset("bgt") ' Branch if Greater Than
                exec = Sub(vm)
                           If Not (vm.Z Or (vm.N Xor vm.V)) Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case 7
                SetOffset("ble") ' Branch if Less or Equal
                exec = Sub(vm)
                           If vm.Z Or (vm.N Xor vm.V) Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
        End Select
        If exec IsNot Nothing Then Return

        Dim v1 = (val >> 9) And 7, v2 = (val >> 6) And 7
        Select Case v1
            Case 0 ' 00 0x xx
                Select Case v2
                    Case 0 ' 00 00 xx
                        Select Case val And &O77
                            Case 0 : SetMne("halt")
                            Case 1 : SetMne("wait")
                            Case 2 : SetMne("rti")
                            Case 3 : SetMne("bpt")
                            Case 4 : SetMne("iot")
                            Case 5 : SetMne("reset")
                            Case 6 : SetMne("rtt")
                        End Select
                    Case 1
                        SetDst("jmp", 2) ' JuMP
                        exec = Sub(vm)
                                   vm.PC = dst.GetAddress(vm)
                               End Sub
                    Case 2 ' 00 02 xx
                        Select Case (val >> 3) And 7
                            Case 0
                                SetReg("rts") ' ReTurn from Subroutine
                                exec = Sub(vm)
                                           vm.PC = vm.Regs(reg)
                                           vm.Regs(reg) = vm.ReadUInt16(vm.GetInc(6, 2))
                                       End Sub
                            Case 3 : SetMne("spl " + (val & 7))
                            Case 4 To 7 ' 00 02 4x - 00 02 7x
                                ' cl*/se*/ccc/scc: CLear/SEt (Condition Codes)
                                Dim mne As String
                                Select Case val
                                    Case &O240, &O260
                                        mne = "nop" ' No OPeration
                                        exec = Sub(vm)
                                               End Sub
                                    Case &O257 : mne = "ccc"
                                    Case &O277 : mne = "scc"
                                    Case Else
                                        mne = If((val And 16) <> 0, "se", "cl") +
                                            If((val And 8) <> 0, "n", "") +
                                            If((val And 4) <> 0, "z", "") +
                                            If((val And 2) <> 0, "v", "") +
                                            If((val And 1) <> 0, "c", "")
                                End Select
                                SetMne(mne)
                                If exec Is Nothing Then
                                    exec = Sub(vm)
                                               Dim f = (val And 16) <> 0
                                               If (val And 8) <> 0 Then vm.N = f
                                               If (val And 4) <> 0 Then vm.Z = f
                                               If (val And 2) <> 0 Then vm.V = f
                                               If (val And 1) <> 0 Then vm.C = f
                                           End Sub
                                End If
                        End Select
                    Case 3 ' SWAp Bytes
                        SetDst("swab", 2)
                        exec = Sub(vm)
                                   Dim vdst = dst.PeekValue(vm)
                                   Dim bh = (vdst >> 8) And &HFF
                                   Dim bl = vdst And &HFF
                                   Dim v = CUShort(((bl << 8) Or bh) And &HFFFF)
                                   dst.SetValue(vm, v)
                                   vm.SetFlags(v = 0, (v And &H8000) <> 0, False, False)
                               End Sub
                End Select
            Case 4
                SetRegDst("jsr", 2) ' Jump to SubRoutine
                exec = Sub(vm)
                           If reg = 7 Then vm.PushStack()
                           Dim ad = dst.GetAddress(vm)
                           vm.Write(vm.GetDec(6, 2), vm.Regs(reg))
                           vm.Regs(reg) = vm.PC
                           vm.PC = ad
                       End Sub
            Case 5
                Select Case v2
                    Case 0
                        SetDst("clr", 2) ' CLeaR
                        exec = Sub(vm)
                                   dst.SetValue(vm, 0)
                                   vm.SetFlags(True, False, False, False)
                               End Sub
                    Case 1
                        SetDst("com", 2) ' COMplement
                        exec = Sub(vm)
                                   Dim v = Not dst.PeekValue(vm)
                                   dst.SetValue(vm, v)
                                   vm.SetFlags(v = 0, (v And &H8000) <> 0, True, False)
                               End Sub
                    Case 2
                        SetDst("inc", 2) ' INCrement
                        exec = Sub(vm)
                                   Dim v = CInt(ConvShort(dst.PeekValue(vm))) + 1
                                   dst.SetValue(vm, CUShort(v And &HFFFF))
                                   vm.SetFlags(v = 0, v < 0, vm.C, v = &H8000)
                               End Sub
                    Case 3
                        SetDst("dec", 2) ' DECrement
                        exec = Sub(vm)
                                   Dim v = CInt(ConvShort(dst.PeekValue(vm))) - 1
                                   dst.SetValue(vm, CUShort(v And &HFFFF))
                                   vm.SetFlags(v = 0, v < 0, vm.C, v = -&H8001)
                               End Sub
                    Case 4
                        SetDst("neg", 2) ' NEGate
                        exec = Sub(vm)
                                   Dim vdst = dst.PeekValue(vm)
                                   Dim val1 = -ConvShort(vdst)
                                   Dim val2 = CUShort(val1 And &HFFFF)
                                   dst.SetValue(vm, val2)
                                   vm.SetFlags(val1 = 0, val1 < 0, val1 <> 0, val1 = &H8000)
                               End Sub
                    Case 5
                        SetDst("adc", 2) ' ADd Carry
                        exec = Sub(vm)
                                   Dim v = CInt(ConvShort(dst.PeekValue(vm))) + If(vm.C, 1, 0)
                                   dst.SetValue(vm, CUShort(v And &HFFFF))
                                   vm.SetFlags(v = 0, v < 0, vm.C AndAlso v = 0, v = &H8000)
                               End Sub
                    Case 6
                        SetDst("sbc", 2) ' SuBtract Carry
                        exec = Sub(vm)
                                   Dim v = CInt(ConvShort(dst.PeekValue(vm))) - If(vm.C, 1, 0)
                                   dst.SetValue(vm, CUShort(v And &HFFFF))
                                   vm.SetFlags(v = 0, v < 0, vm.C AndAlso v = -1, v = -&H8001)
                               End Sub
                    Case 7
                        SetDst("tst", 2) ' TeST
                        exec = Sub(vm)
                                   Dim v = ConvShort(dst.GetValue(vm))
                                   vm.SetFlags(v = 0, v < 0, False, False)
                               End Sub
                End Select
            Case 6
                Select Case v2
                    Case 0
                        SetDst("ror", 2) ' ROtate Right
                        exec = Sub(vm)
                                   Dim vdst = dst.PeekValue(vm)
                                   Dim val1 = (vdst >> 1) Or If(vm.C, &H8000US, 0US)
                                   dst.SetValue(vm, val1)
                                   Dim lsb0 = (vdst And 1) <> 0
                                   Dim msb1 = vm.C
                                   vm.SetFlags(val1 = 0, msb1, lsb0, msb1 <> lsb0)
                               End Sub
                    Case 1
                        SetDst("rol", 2) ' ROtate Left
                        exec = Sub(vm)
                                   Dim vdst = dst.PeekValue(vm)
                                   Dim val1 = CUShort((CUInt(vdst) << 1) And &HFFFF) Or If(vm.C, 1US, 0US)
                                   dst.SetValue(vm, val1)
                                   Dim msb0 = (vdst And &H8000) <> 0
                                   Dim msb1 = (val1 And &H8000) <> 0
                                   vm.SetFlags(val1 = 0, msb1, msb0, msb1 <> msb0)
                               End Sub
                    Case 2
                        SetDst("asr", 2) ' Arithmetic Shift Right
                        exec = Sub(vm)
                                   Dim vdst = dst.PeekValue(vm)
                                   Dim val1 = ConvShort(vdst) >> 1
                                   dst.SetValue(vm, CUShort(val1 And &HFFFF))
                                   Dim lsb0 = (vdst And 1) <> 0
                                   Dim msb1 = val1 < 0
                                   vm.SetFlags(val1 = 0, msb1, lsb0, msb1 <> lsb0)
                               End Sub
                    Case 3
                        SetDst("asl", 2) ' Arithmetic Shift Left
                        exec = Sub(vm)
                                   Dim vdst = dst.PeekValue(vm)
                                   Dim val1 = CUShort((CUInt(vdst) << 1) And &HFFFF)
                                   dst.SetValue(vm, val1)
                                   Dim msb0 = (vdst And &H8000) <> 0
                                   Dim msb1 = val1 < 0
                                   vm.SetFlags(val1 = 0, msb1, msb0, msb1 <> msb0)
                               End Sub
                    Case 4
                        SetNum("mark") ' MARK
                        exec = Sub(vm)
                                   Dim nn = val And &O77
                                   vm.Regs(6) = CUShort((vm.Regs(6) + 2 * nn) And &HFFFF)
                                   vm.PC = vm.Regs(5)
                                   vm.Regs(5) = vm.ReadUInt16(vm.GetInc(6, 2))
                               End Sub
                    Case 5
                        SetDst("mfpi", 2)
                        exec = Sub(vm)
                               End Sub
                    Case 6
                        SetDst("mtpi", 2)
                        exec = Sub(vm)
                               End Sub
                    Case 7
                        SetDst("sxt", 2) ' Sign eXTend
                        exec = Sub(vm)
                                   dst.SetValue(vm, If(vm.N, &HFFFFUS, 0US))
                                   vm.SetFlags(Not vm.N, vm.N, vm.C, vm.V)
                               End Sub
                End Select
        End Select
    End Sub

    Private Sub Set7()
        Select Case (val >> 9) And 7
            Case 0
                SetSrcReg("mul", 2) ' MULtiply
                exec = Sub(vm)
                           Dim vsrc = ConvShort(src.GetValue(vm))
                           Dim vdst = CInt(vm.Regs(reg)) * vsrc
                           If (reg And 1) = 0 Then
                               vm.SetReg32(reg, vdst)
                           Else
                               vm.Regs(reg) = CUShort(vdst And &HFFFF)
                           End If
                           vm.SetFlags(vdst = 0, vdst < 0, vdst < -&H8000 OrElse vdst >= &H8000, False)
                       End Sub
            Case 1
                SetSrcReg("div", 2) ' DIVide
                exec = Sub(vm)
                           Dim vsrc = ConvShort(src.GetValue(vm))
                           If vsrc = 0 OrElse Math.Abs(ConvShort(vm.Regs(reg))) > Math.Abs(vsrc) Then
                               vm.SetFlags(False, False, vsrc = 0, True)
                           Else
                               Dim vdst = vm.GetReg32(reg)
                               Dim r1 = vdst \ vsrc
                               Dim r2 = vdst Mod vsrc
                               vm.Regs(reg) = CUShort(r1 And &HFFFF)
                               vm.Regs((reg + 1) And 7) = CUShort(r2 And &HFFFF)
                               vm.SetFlags(r1 = 0, r1 < 0, False, False)
                           End If
                       End Sub
            Case 2
                SetSrcReg("ash", 2) ' Arithmetic SHift
                exec = Sub(vm)
                           Dim vsrc = src.GetValue(vm) And &O77
                           Dim val0 = ConvShort(vm.Regs(reg))
                           If vsrc = 0 Then
                               vm.SetFlags(val0 = 0, val0 < 0, vm.C, False)
                           Else
                               If (vsrc And &O40) = 0 Then
                                   Dim val1 = val0 << (vsrc - 1)
                                   Dim val2 = val1 << 1
                                   vm.Regs(reg) = CUShort(val2 And &HFFFF)
                                   vm.SetFlags(val2 = 0, val2 < 0, (val1 And 1) <> 0, val0 <> val2)
                               Else
                                   Dim val1 = val0 >> (63 - vsrc)
                                   Dim val2 = val1 >> 1
                                   vm.Regs(reg) = CUShort(val2 And &HFFFF)
                                   vm.SetFlags(val2 = 0, val2 < 0, val1 < 0, val0 <> val2)
                               End If
                           End If
                       End Sub
            Case 3
                SetSrcReg("ashc", 2) ' Arithmetic SHift Combined
                exec = Sub(vm)
                           Dim vsrc = src.GetValue(vm) And &O77
                           Dim val0 = vm.GetReg32(reg)
                           If vsrc = 0 Then
                               vm.SetFlags(val0 = 0, val0 < 0, vm.C, False)
                           Else
                               If (vsrc And &O40) = 0 Then
                                   Dim val1 = val0 << (vsrc - 1)
                                   Dim val2 = val1 << 1
                                   vm.SetReg32(reg, val2)
                                   vm.SetFlags(val2 = 0, val2 < 0, (val1 And 1) <> 0, val0 <> val2)
                               Else
                                   Dim val1 = val0 >> (63 - vsrc)
                                   Dim val2 = val1 >> 1
                                   vm.SetReg32(reg, val2)
                                   vm.SetFlags(val2 = 0, val2 < 0, val1 < 0, val0 <> val2)
                               End If
                           End If
                       End Sub
            Case 4
                SetRegDst("xor", 2) ' eXclusive OR
                exec = Sub(vm)
                           Dim vdst = vm.Regs(reg) Xor dst.PeekValue(vm)
                           dst.SetValue(vm, vdst)
                           vm.SetFlags(vdst = 0, (vdst And &H8000) <> 0, vm.C, False)
                       End Sub
            Case 5
                Select Case (val >> 3) And &O77
                    Case 0 : SetReg("fadd")
                    Case 1 : SetReg("fsub")
                    Case 2 : SetReg("fmul")
                    Case 3 : SetReg("fdiv")
                End Select
            Case 7
                SetRegOffset("sob") ' Subtract One from register, Branch if not zero
                exec = Sub(vm)
                           vm.Regs(reg) = CUShort((vm.Regs(reg) - 1) And &HFFFF)
                           If vm.Regs(reg) <> 0 Then vm.PC = CUShort(vm.PC - (val And &O77) * 2)
                       End Sub
        End Select
    End Sub

    Private Sub Set10()
        Select Case val >> 8
            Case &H80
                SetOffset("bpl") ' Branch if PLus
                exec = Sub(vm)
                           If Not vm.N Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case &H81
                SetOffset("bmi") ' Branch if MInus
                exec = Sub(vm)
                           If vm.N Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case &H82
                SetOffset("bhi") ' Branch if HIgher
                exec = Sub(vm)
                           If Not (vm.C Or vm.Z) Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case &H83
                SetOffset("blos") ' Branch if LOwer or Same
                exec = Sub(vm)
                           If vm.C Or vm.Z Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case &H84
                SetOffset("bvc") ' Branch if oVerflow Clear
                exec = Sub(vm)
                           If Not vm.V Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case &H85
                SetOffset("bvs") ' Branch if oVerflow Set
                exec = Sub(vm)
                           If vm.V Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case &H86
                SetOffset("bcc") ' Branch if Carry Clear
                exec = Sub(vm)
                           If Not vm.C Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case &H87
                SetOffset("bcs") ' Branch if Carry Set
                exec = Sub(vm)
                           If vm.C Then vm.PC = vm.GetOffset(vm.PC)
                       End Sub
            Case &H88
                disasm = Function(bd, pos) "emt " + bd.Enc(bd(pos))
                Length = 2
            Case &H89
                Dim arg = val And 255
                If arg < SysNames.Length AndAlso SysNames(arg) IsNot Nothing Then
                    disasm =
                        Function(bd, pos)
                            Dim sb = New StringBuilder("sys " + SysNames(arg))
                            If arg = 0 Then ' indir
                                Dim ad = bd.ReadUInt16(pos + 2)
                                Dim argad = bd.EncAddr(ad)
                                Dim p = argad.IndexOf("{")
                                If p > 0 Then argad = argad.Substring(0, p)
                                sb.Append("; " + argad + "{" + Disassembler.Disassemble(bd, ad) + "}")
                            ElseIf arg = 8 Or arg = 15 Then ' creat/chmod
                                sb.Append("; " + bd.EncAddr(bd.ReadUInt16(pos + 4)))
                                sb.Append("; " + "0" + Convert.ToString(bd.ReadUInt16(pos + 4), 8))
                            ElseIf arg = 48 Then ' signal
                                Dim sig = bd.ReadUInt16(pos + 2)
                                If sig < SigNames.Length AndAlso SigNames(sig) IsNot Nothing Then
                                    sb.Append("; " + SigNames(sig))
                                Else
                                    sb.Append("; " & sig)
                                End If
                                sb.Append("; " + bd.EncAddr(bd.ReadUInt16(pos + 4)))
                            Else
                                Dim argc = SysArgs(arg)
                                For i = 1 To argc
                                    sb.Append("; " + bd.Enc(bd.ReadUInt16(pos + i * 2)))
                                Next
                            End If
                            Return sb.ToString
                        End Function
                    Length = CUShort(2 + SysArgs(arg) * 2)
                Else
                    disasm = Function(bd, pos) "sys " & arg
                    Length = 2
                End If
                'exec = Sub(vm)
                '           vm.ExecSys()
                '       End Sub
        End Select
        If exec IsNot Nothing Then Return

        Select Case (val >> 6) And &O77
            Case &O50
                SetDst("clrb", 1) ' CLeaR Byte
                exec = Sub(vm)
                           dst.SetByte(vm, 0)
                           vm.SetFlags(True, False, False, False)
                       End Sub
            Case &O51
                SetDst("comb", 1) ' COMplement Byte
                exec = Sub(vm)
                           Dim v = Not dst.PeekByte(vm)
                           dst.SetByte(vm, v)
                           vm.SetFlags(v = 0, (v And &H80) <> 0, True, False)
                       End Sub
            Case &O52
                SetDst("incb", 1) ' INCrement Byte
                exec = Sub(vm)
                           Dim v = CInt(ConvSByte(dst.PeekByte(vm))) + 1
                           dst.SetByte(vm, CByte(v And &HFF))
                           vm.SetFlags(v = 0, v < 0, vm.C, v = &H80)
                       End Sub
            Case &O53
                SetDst("decb", 1) ' DECrement Byte
                exec = Sub(vm)
                           Dim v = CInt(ConvSByte(dst.PeekByte(vm))) - 1
                           dst.SetByte(vm, CByte(v And &HFF))
                           vm.SetFlags(v = 0, v < 0, vm.C, v = -&H81)
                       End Sub
            Case &O54
                SetDst("negb", 1) ' NEGate Byte
                exec = Sub(vm)
                           Dim vdst = dst.PeekByte(vm)
                           Dim val1 = -ConvSByte(vdst)
                           Dim val2 = CByte(val1 And &HFF)
                           dst.SetByte(vm, val2)
                           vm.SetFlags(val1 = 0, val1 < 0, val1 <> 0, val1 = &H80)
                       End Sub
            Case &O55
                SetDst("adcb", 1) ' ADd Carry Byte
                exec = Sub(vm)
                           Dim v = CInt(ConvSByte(dst.PeekByte(vm))) + If(vm.C, 1, 0)
                           dst.SetByte(vm, CByte(v And &HFF))
                           vm.SetFlags(v = 0, v < 0, vm.C AndAlso v = 0, v = &H80)
                       End Sub
            Case &O56
                SetDst("sbcb", 1) ' SuBtract Carry Byte
                exec = Sub(vm)
                           Dim v = CInt(ConvSByte(dst.PeekByte(vm))) - If(vm.C, 1, 0)
                           dst.SetByte(vm, CByte(v And &HFF))
                           vm.SetFlags(v = 0, v < 0, vm.C AndAlso v = -1, v = -&H81)
                       End Sub
            Case &O57
                SetDst("tstb", 1) ' TeST Byte
                exec = Sub(vm)
                           Dim vdst = ConvSByte(dst.GetByte(vm))
                           vm.SetFlags(vdst = 0, vdst < 0, False, False)
                       End Sub
            Case &O60
                SetDst("rorb", 1) ' ROtate Right Byte
                exec = Sub(vm)
                           Dim vdst = dst.PeekByte(vm)
                           Dim val1 = vdst >> 1
                           If vm.C Then val1 = CByte(val1 + &H80)
                           dst.SetByte(vm, val1)
                           Dim lsb0 = (vdst And 1) <> 0
                           Dim msb1 = vm.C
                           vm.SetFlags(val1 = 0, msb1, lsb0, msb1 <> lsb0)
                       End Sub
            Case &O61
                SetDst("rolb", 1) ' ROtate Left Byte
                exec = Sub(vm)
                           Dim vdst = dst.PeekByte(vm)
                           Dim val1 = CByte(((CUInt(vdst) << 1) + If(vm.C, 1, 0)) And &HFF)
                           dst.SetByte(vm, val1)
                           Dim msb0 = (vdst And &H80) <> 0
                           Dim msb1 = (val1 And &H80) <> 0
                           vm.SetFlags(val1 = 0, msb1, msb0, msb1 <> msb0)
                       End Sub
            Case &O62
                SetDst("asrb", 1) ' Arithmetic Shift Right Byte
                exec = Sub(vm)
                           Dim vdst = dst.PeekByte(vm)
                           Dim val1 = ConvSByte(vdst) >> 1
                           dst.SetByte(vm, CByte(val1 And &HFF))
                           Dim lsb0 = (vdst And 1) <> 0
                           Dim msb1 = val1 < 0
                           vm.SetFlags(val1 = 0, msb1, lsb0, msb1 <> lsb0)
                       End Sub
            Case &O63
                SetDst("aslb", 1) ' Arithmetic Shift Left Byte
                exec = Sub(vm)
                           Dim vdst = dst.PeekByte(vm)
                           Dim val1 = CByte((CUInt(vdst) << 1) And &HFF)
                           dst.SetByte(vm, val1)
                           Dim msb0 = (vdst And &H80) <> 0
                           Dim msb1 = val1 < 0
                           vm.SetFlags(val1 = 0, msb1, msb0, msb1 <> msb0)
                       End Sub
            Case &O64 : SetDst("mfpd", 1)
            Case &O65 : SetDst("mtpd", 1)
        End Select
    End Sub

    Private Sub Set17()
        Select Case val And &HFFF
            Case 1
                SetMne("setf") ' SET Float
                exec = Sub(vm)
                           vm.IsDouble = False
                       End Sub
            Case 2
                SetMne("seti") ' SET Integer
                exec = Sub(vm)
                           vm.IsLong = False
                       End Sub
            Case &O11
                SetMne("setd") ' SET Double
                exec = Sub(vm)
                           vm.IsDouble = True
                       End Sub
            Case &O12
                SetMne("setl") ' SET Long
                exec = Sub(vm)
                           vm.IsLong = True
                       End Sub
        End Select
    End Sub
End Class
