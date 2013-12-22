Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class SubAreaTemplate

        Public Id As Integer
        Public CanConquest As Boolean
        Public Prism As Prism
        Public IsFixedGroup As Boolean

        Public GroupMinLevel As Integer
        Public GroupMaxLevel As Integer
        Public GroupMinSize As Integer
        Public GroupMaxSize As Integer
        Public MaxGroup As Integer

        Public RespawnTime As Integer

        Public MonstersList As New List(Of MonsterTemplate)
        Public Maps As New List(Of Map)


        Public ReadOnly Property GroupNumber As Integer
            Get
                Return If(MaxGroup = -1, 3, MaxGroup)
            End Get
        End Property

        Public Function GetGroup(ByVal Map As World.Map) As MonsterGroup

            If MonstersList.Count = 0 Then Return Nothing

            If IsFixedGroup Then

                Dim FixedGroup As New MonsterGroup()
                For Each Monster In MonstersList
                    Dim NewMonster As New Monster(Monster.GetRandomLevel())
                    FixedGroup.MonsterList.Add(NewMonster)
                Next

                Return FixedGroup

            End If


            Dim Nbr = 0
            Dim essais = 0

replay:

            If GroupMaxSize = -1 AndAlso GroupMaxSize = -1 Then

                Dim group1 = Map.MonsterList.FirstOrDefault(Function(m) m.MonsterList.Count >= 1 AndAlso m.MonsterList.Count <= 2) IsNot Nothing
                Dim group2 = Map.MonsterList.FirstOrDefault(Function(m) m.MonsterList.Count >= 3 AndAlso m.MonsterList.Count <= 4) IsNot Nothing
                Dim group3 = Map.MonsterList.FirstOrDefault(Function(m) m.MonsterList.Count >= 5 AndAlso m.MonsterList.Count <= 8) IsNot Nothing

                If Not group1 Then
                    Nbr = Basic.Rand(1, 2)
                ElseIf Not group2 Then
                    Nbr = Basic.Rand(3, 4)
                ElseIf Not group3 Then
                    Nbr = Basic.Rand(5, 8)
                Else
                    Return Nothing
                End If

            Else

                If GroupMinSize = -1 Then GroupMaxSize = 1
                If GroupMaxSize = -1 Then GroupMaxSize = 8

                Nbr = Basic.Rand(GroupMinSize, GroupMaxSize)

            End If

            Dim Group = MonsterGroup.RandomFromList(Nbr, MonstersList)

            If Group.Level < GroupMinLevel OrElse Group.Level > GroupMaxLevel Then
                If essais < 25 Then
                    essais += 1
                    GoTo replay
                Else
                    Return Nothing
                End If

            End If

            Return Group

        End Function

    End Class
End Namespace