Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class MonsterIA

#Region " Déclarations "

        Dim Fight As Fight
        Dim Monster As Fighter

        Public Sub New(ByVal eFight As Fight, ByVal eMonster As Fighter)
            Fight = eFight
            Monster = eMonster
        End Sub

#End Region

#Region " Fonctions "

        Dim UsableCells As New List(Of Integer)
        Dim OriginalCell As Integer = 0
        Dim BestSpell As SpellLevel = Nothing
        Dim BestLaunchCell As Integer = 0
        Dim BestMoveCell As Integer = 0
        Dim BestScore As Integer = 0

        Dim HasMoved As Boolean = False
        Dim HasAttacked As Boolean = False

        Private Sub ResetAll()

            BestSpell = Nothing
            BestLaunchCell = 0
            BestMoveCell = 0
            BestScore = 0

            HasMoved = False
            HasAttacked = False

        End Sub

        Private Sub GenerateAllCells()

            UsableCells.Clear()
            UsableCells.Add(Monster.Cell)

            For i As Integer = 1 To 1024

                If Fight.IsValidCell(i) Then
                    If Utils.Cells.GoalDistance(Fight.Map, Monster.Cell, i) <= Monster.PM Then
                        If Not UsableCells.Contains(i) Then UsableCells.Add(i)
                    End If
                End If

            Next

        End Sub

        Private Sub GenerateBestCell(ByVal SpellLevel As SpellLevel)

            For i As Integer = 1 To 1024
                If Fight.Map.IsCellReachable(i) AndAlso Not Fight.GetFighterFromCell(i) Is Nothing AndAlso Fight.CanLaunchSpell(Monster, SpellLevel, i) Then

                    Dim Score As Integer = SpellLevel.GetScore(Monster, i)
                    If Score > 0 AndAlso Not Fight.GetFighterFromCell(i) Is Nothing AndAlso Fight.GetFighterFromCell(i).Team <> Monster.Team Then Score += 50
                    Score -= Utils.Cells.GoalDistance(Fight.Map, OriginalCell, Monster.Cell)
                    If Score > BestScore Then
                        BestScore = Score
                        BestLaunchCell = i
                        BestSpell = SpellLevel
                        BestMoveCell = Monster.Cell
                    End If

                End If
            Next

        End Sub

        Private Sub GenerateBestSpell()

            GenerateAllCells()

            OriginalCell = Monster.Cell
            For Each Cell As Integer In UsableCells

                If Cell = Monster.Cell Or Fight.IsValidCell(Cell) Then
                    Monster.Cell = Cell
                    For Each SpellLevel As SpellLevel In Monster.Monster.Level.SpellList
                        GenerateBestCell(SpellLevel)
                    Next
                End If

            Next
            Monster.Cell = OriginalCell

        End Sub

        Private Sub ApplyBestMovement()

            If BestScore <> 0 Then
                If Monster.Cell <> BestMoveCell Then
                    Dim Path As New Utils.Pathfinding
                    Dim NewPath As String = Path.Pathing(Fight.Map, Monster.Cell, BestMoveCell, True, Monster.PM, True, Fight)
                    If Not Fight.PlayerMove(Monster, NewPath, Utils.Cells.GetPathLenght(Monster.Cell, Fight.Map, NewPath)) Then
                        Exit Sub
                    End If
                End If
                Fight.LaunchSpell(Monster, BestSpell, BestLaunchCell)
                HasMoved = True
                HasAttacked = True
            End If


        End Sub

        Private Sub ApplyEndMove(ByVal Recul As Boolean)

            Dim NearestPlayer As Integer = -1
            Dim BestDistance As Integer = Integer.MaxValue

            For Each Player As Fighter In Fight.Fighters
                If Player.Team <> Monster.Team Then
                    Dim Distance As Integer = Utils.Cells.GoalDistance(Fight.Map, Monster.Cell, Player.Cell)
                    If Distance < BestDistance Then
                        NearestPlayer = Player.Cell
                        BestDistance = Distance
                    End If
                End If
            Next

            If NearestPlayer <> -1 Then

                GenerateAllCells()

                Dim BestCell As Integer = -1
                BestDistance = 1000

                For Each Cell As Integer In UsableCells
                    Dim Distance As Integer = Utils.Cells.GoalDistance(Fight.Map, NearestPlayer, Cell)
                    If Recul Then
                        If Distance > BestDistance Then
                            BestCell = Cell
                            BestDistance = Distance
                        End If
                    Else
                        If Distance < BestDistance Then
                            BestCell = Cell
                            BestDistance = Distance
                        End If
                    End If
                Next

                If BestCell <> -1 AndAlso BestCell <> Monster.Cell Then
                    Dim Path As New Utils.Pathfinding
                    Dim NewPath As String = Path.Pathing(Fight.Map, Monster.Cell, BestCell, True, Monster.PM, True, Fight)
                    If NewPath <> "" Then Fight.PlayerMove(Monster, NewPath, Utils.Cells.GetPathLenght(Monster.Cell, Fight.Map, NewPath))
                End If

            End If


            If Fight.WalkOnTrap(Monster.Cell) Then
                Fight.LaunchTrapEffects(Monster, Monster.Cell)
            End If

        End Sub

        Private Sub ApplyEndMovement()

            If Not HasAttacked Then
                ApplyEndMove(False)
            Else
                ApplyEndMove(True)
            End If

        End Sub

#End Region

#Region " Application "

        Public Sub ApplyThread()

            Try

                Do

                    ResetAll()
                    GenerateBestSpell()
                    ApplyBestMovement()

                Loop While HasMoved

                ApplyEndMovement()

                Fight.FinishTurn()

            Catch ex As Exception
                Fight.FinishTurn()
            End Try

        End Sub

        Public Sub Apply()

            Dim FightThread As New Threading.Tasks.Task(AddressOf ApplyThread)
            FightThread.Start()

        End Sub

#End Region

    End Class
End Namespace