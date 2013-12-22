Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class Survivant : Inherits Challenge

        Sub New(ByVal Fight As Fight)
            MyBase.New(33, Fight)

            BasicDropBonus = 10
            BasicXpBonus = 10

            TeamDropBonus = 15
            TeamXpBonus = 15

            ShowTarget = False
            TargetId = 0

        End Sub


        Public Overrides Sub EndTurn(ByVal Fighter As World.Fighter)
            For Each p In Fight.Fighters.Where(Function(f) f.Team = Fighter.Team)
                If p.Dead OrElse p.Abandon Then
                    Fail()
                End If
            Next
        End Sub

    End Class
End Namespace