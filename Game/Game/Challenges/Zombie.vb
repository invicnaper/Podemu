﻿Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class Zombie : Inherits Challenge

        Sub New(ByVal Fight As Fight)
            MyBase.New(1, Fight)

            BasicDropBonus = 10
            BasicXpBonus = 10

            TeamDropBonus = 15
            TeamXpBonus = 15

            ShowTarget = False
            TargetId = 0

        End Sub


        Public Overrides Sub CheckMovement(ByVal FromCell As Integer, ByVal ToCell As Integer, ByVal Len As Integer)
            If Len <> 1 Then
                Fail()
            End If
        End Sub

        Public Overrides Sub EndTurn(ByVal Fighter As Fighter)
            If Fighter.Stats.MaxPM - Fighter.PM <> 1 Then
                Fail()
            End If
        End Sub

    End Class
End Namespace