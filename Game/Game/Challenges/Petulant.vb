﻿Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class Petulant : Inherits Challenge

        Sub New(ByVal Fight As Fight)
            MyBase.New(41, Fight)

            BasicDropBonus = 10
            BasicXpBonus = 10

            TeamDropBonus = 15
            TeamXpBonus = 15

            ShowTarget = False
            TargetId = 0

        End Sub


        Public Overrides Sub EndTurn(ByVal Fighter As World.Fighter)
            If Fighter.PA > 0 Then
                Fail()
            End If
        End Sub

    End Class
End Namespace