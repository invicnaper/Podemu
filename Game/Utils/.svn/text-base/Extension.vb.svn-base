Imports System.Runtime.CompilerServices

Namespace Utils

    Module UtilsExtension

        <Extension()>
        Public Sub Remove(Of T)(ByRef List As IList(Of T), ByVal Predicate As Func(Of T, Boolean))

            Dim Ok = List.Where(Function(f) Predicate(f)).ToList()

            For Each Value In Ok
                List.Remove(Value)
            Next

        End Sub


    End Module
End Namespace