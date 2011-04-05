Public Class Symbol
    Public Property Name$
    Public Property Type%
    Public Property Address%
    Public Property ObjSym As Symbol

    Private locals As New Dictionary(Of Integer, Symbol)

    Public ReadOnly Property IsObject As Boolean
        Get
            Return Type = &H1F
        End Get
    End Property

    Public ReadOnly Property IsGlobal As Boolean
        Get
            Return Type = &H22 OrElse Type = &H23 OrElse Type = &H24
        End Get
    End Property

    Public ReadOnly Property IsLocal As Boolean
        Get
            Return Type = 1
        End Get
    End Property

    Public ReadOnly Property IsNull As Boolean
        Get
            Return Name Is Nothing AndAlso ObjSym Is Nothing
        End Get
    End Property

    Public ReadOnly Property HasLocal As Boolean
        Get
            Return locals.Count > 0
        End Get
    End Property

    Public Sub New(pos%)
        Address = pos
    End Sub

    Public Sub New(data As Byte(), pos%)
        Name = ReadString(data, pos, 8)
        Type = BitConverter.ToUInt16(data, pos + 8)
        Address = BitConverter.ToUInt16(data, pos + 10)
        If IsLocal And Address >= &H8000 Then Address -= &H10000
    End Sub

    Public Sub New(sym As Symbol)
        ObjSym = sym
        Address = sym.Address
    End Sub

    Public Overrides Function ToString() As String
        Dim pre = If(IsGlobal, ".globl ", "")
        Dim n = If(Name Is Nothing, "", If(IsObject, "(" + Name + ")", Name + ":"))
        Dim obj = If(ObjSym Is Nothing, "", If(pre + n = "", "", " ") + ObjSym.ToString)
        Return pre + n + obj
    End Function

    Public Sub SetSymbol(sym As Symbol)
        Name = sym.Name
        Type = sym.Type
        Address = sym.Address
    End Sub

    Public Sub AddLocal(sym As Symbol)
        locals.Add(sym.Address, sym)
    End Sub

    Public Function GetLocal(offset%) As Symbol
        Return If(locals.ContainsKey(offset), locals(offset), Nothing)
    End Function

    Public Function GetLocals() As Symbol()
        Return locals.Values.ToArray
    End Function
End Class
