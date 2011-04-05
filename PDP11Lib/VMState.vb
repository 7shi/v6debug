Imports System.Text

Public Class VMState
    Private vm As VM
    Public Regs(7) As UShort
    Public Stack() As UShort
    Public Z As Boolean
    Public N As Boolean
    Public C As Boolean
    Public V As Boolean

    Public Sub New(vm As VM)
        Me.vm = vm
        Array.Copy(vm.Regs, Regs, Regs.Length)
        ReDim Stack(Math.Min(4, (&H10000 - Regs(6)) \ 2) - 1)
        For i = 0 To Stack.Length - 1
            Stack(i) = vm.ReadUInt16(Regs(6) + i * 2)
        Next
        Z = vm.Z
        N = vm.N
        C = vm.C
        V = vm.V
    End Sub

    Public Sub Restore()
        Array.Copy(Regs, vm.Regs, Regs.Length)
        For j = 0 To Stack.Length - 1
            vm.Write(Regs(6) + j * 2, Stack(j))
        Next
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
        Dim sb = New StringBuilder
        sb.Append(GetFlags)
        For i = 0 To Regs.Length - 1
            sb.AppendFormat(" {0}={1}", RegNames(i), vm.Enc0(Regs(i)))
            If i = 6 Then
                sb.Append("{")
                For j = 0 To Stack.Length - 1
                    If j > 0 Then sb.Append(" ")
                    sb.Append(vm.Enc0(Stack(j)))
                Next
                sb.Append("}")
            End If
        Next
        Return sb.ToString
    End Function
End Class
