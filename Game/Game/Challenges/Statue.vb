Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class Statue : Inherits Challenge

        Sub New(ByVal Fight As Fight)
            MyBase.New(2, Fight)

            BasicDropBonus = 10
            BasicXpBonus = 10

            TeamDropBonus = 15
            TeamXpBonus = 15

            ShowTarget = False
            TargetId = 0

        End Sub

        Public StartPos As Integer

        Public Overrides Sub BeginTurn(ByVal Fighter As World.Fighter)
            StartPos = Fighter.Cell
        End Sub

        Public Overrides Sub EndTurn(ByVal Fighter As Fighter)
            If Fighter.Cell <> StartPos Then
                Fail()
            End If
        End Sub

    End Class
End Namespace