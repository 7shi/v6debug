Imports System.Text

Public Class VMState
    Private vm As VM
    Public Regs(7) As UShort
    Public Stack(7) As Byte
    Public Z As Boolean
    Public N As Boolean
    Public C As Boolean
    Public V As Boolean

    Public Sub New(vm As VM)
        Me.vm = vm
        Array.Copy(vm.Regs, Regs, Regs.Length)
        Dim stlen = Math.Min(8, &H10000 - Regs(6))
        Array.Copy(vm.Data, Regs(6), Stack, 0, stlen)
        Z = vm.Z
        N = vm.N
        C = vm.C
        V = vm.V
    End Sub

    Public Sub Restore()
        Array.Copy(Regs, vm.Regs, Regs.Length)
        Dim stlen = Math.Min(8, &H10000 - Regs(6))
        Array.Copy(Stack, 0, vm.Data, Regs(6), stlen)
        vm.SetFlags(Z, N, C, V)
    End Sub

    Public Function GetFlags$()
        Dim sb = New StringBuilder
        sb.Append(If(Z, "Z", "-"))
        sb.Append(If(N, "N", "-"))
        sb.Append(If(C, "C", "-"))
        sb.Append(If(V, "V", "-"))
        Return sb.ToString
    End Function

    Public Overrides Function ToString() As String
        Return String.Format(
            "{0} r0={1} r1={2} r2={3} r3={4} r4={5} r5={6} sp={7}{{{8} {9} {10} {11}}} pc={12}",
            GetFlags,
            vm.Enc0(Regs(0)),
            vm.Enc0(Regs(1)),
            vm.Enc0(Regs(2)),
            vm.Enc0(Regs(3)),
            vm.Enc0(Regs(4)),
            vm.Enc0(Regs(5)),
            vm.Enc0(Regs(6)),
            vm.Enc0(BitConverter.ToUInt16(Stack, 0)),
            vm.Enc0(BitConverter.ToUInt16(Stack, 2)),
            vm.Enc0(BitConverter.ToUInt16(Stack, 4)),
            vm.Enc0(BitConverter.ToUInt16(Stack, 6)),
            vm.Enc0(Regs(7)))
    End Function
End Class
