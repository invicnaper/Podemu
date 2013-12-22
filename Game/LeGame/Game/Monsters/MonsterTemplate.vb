Namespace Game
    Public Class MonsterTemplate

        Public Id As Integer
        Public Nom As String
        Public Skin As Integer
        Public Color(3) As Integer
        Public LevelList As New List(Of MonsterLevel)(5)
        Public AI As Integer
        Public MaxKamas, MinKamas, BaseExp As Integer

        Public ReadOnly Property GetRandomLevel() As MonsterLevel
            Get
                Return LevelList(Utils.Basic.Rand(0, LevelList.Count - 1))
            End Get
        End Property

    End Class
End Namespace