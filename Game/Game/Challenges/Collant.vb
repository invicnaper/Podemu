﻿Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class Collant : Inherits Challenge

        Sub New(ByVal Fight As Fight)
            MyBase.New(37, Fight)

            BasicDropBonus = 10
            BasicXpBonus = 10

            TeamDropBonus = 15
            TeamXpBonus = 15

            ShowTarget = False
            TargetId = 0

        End Sub


        Public Overrides Sub EndTurn(ByVal Fighter As World.Fighter)
            Dim Cells = Utils.Cells.NearestCells(Fight.Map, Fighter.Cell)
            For Each player In Fight.Fighters.Where(Function(f) f.Team = Fighter.Team)
                If Cells.Contains(player.Cell) Then
                    Exit Sub
                End If
            Next
            Fail()
        End Sub

    End Class
End Namespace