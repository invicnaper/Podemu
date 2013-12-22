Namespace Game
    Public Class ItemSetTemplate

        Public Id As Integer
        Public Nom As String
        Public ListItems As New List(Of Integer)
        Public EffectList As New Dictionary(Of Integer, List(Of ItemSetEffect))

    End Class
End Namespace