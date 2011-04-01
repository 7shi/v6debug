Public Class Symbol
    Public Property Name$
    Public Property Type%
    Public Property Address%
    Public Property ObjSym As Symbol

    Public ReadOnly Property IsObject As Boolean
        Get
            Return Type = &H1F
        End Get
    End Property

    Public ReadOnly Property IsGlobal As Boolean
        Get
            Return Type = &H22
        End Get
    End Property

    Public Sub New(data As Byte(), pos%)
        Name = ReadText(data, pos, 8)
        Type = BitConverter.ToUInt16(data, pos + 8)
        Address = BitConverter.ToUInt16(data, pos + 10)
    End Sub

    Public Sub New(sym As Symbol)
        ObjSym = sym
    End Sub

    Public Overrides Function ToString() As String
        Dim pre = If(IsGlobal, ".globl ", "")
        Dim n = If(IsObject, "(" + Name + ")", Name + ":")
        Dim obj = If(ObjSym Is Nothing, "", " " + ObjSym.ToString)
        Return pre + n + obj
    End Function

    Public Sub SetSymbol(sym As Symbol)
        If sym.IsObject Then
            ObjSym = sym
        ElseIf Not IsGlobal Then
            Name = sym.Name
            Type = sym.Type
            Address = sym.Address
            If sym.ObjSym IsNot Nothing Then ObjSym = sym.ObjSym
        End If
    End Sub
End Class
