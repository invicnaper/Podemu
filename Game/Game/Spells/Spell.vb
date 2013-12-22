Namespace Game
    Public Class Spell

        Public SpellId As Integer
        Public Name As String
        Public AnimationId As Integer
        Public SpriteInfos As String
        Public LevelList As New List(Of SpellLevel)

        Public Function LevelExist(ByVal Level As Integer) As Boolean
            Return Level <= LevelList.Count
        End Function

        Public Function AtLevel(ByVal Level As Integer) As SpellLevel
            Return LevelList(Level - 1)
        End Function

    End Class
End Namespace