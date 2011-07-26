Imports System.IO

Public Interface FileSystem
    Function Open%(p$)
    Function Create%(p$)
    Function Close(h%) As Boolean
    Sub CloseAll()
    Function Read%(h%, data As Byte(), offset%, count%)
    Sub Write(h%, data As Byte(), offset%, count%)
    Sub Seek(h%, offset%, origin As SeekOrigin)
    Function Duplicate%(h%)
    Function Delete(p$) As Boolean
    Function Link(src$, dst$) As Boolean
    Function Exists(p$) As Boolean
    Function GetLength%(p$)
    Function GetAllBytes(p$) As Byte()
    Function GetPath$(h%)
End Interface
