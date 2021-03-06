﻿Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class Versatile : Inherits Challenge

        Sub New(ByVal Fight As Fight)
            MyBase.New(6, Fight)

            BasicDropBonus = 10
            BasicXpBonus = 10

            TeamDropBonus = 15
            TeamXpBonus = 15

            ShowTarget = False
            TargetId = 0

        End Sub


        Public Overrides Sub CheckSpell(ByVal Fighter As World.Fighter, ByVal Effect As SpellEffect, ByVal Cibles As System.Collections.Generic.List(Of World.Fighter), ByVal Cell As Integer)

            If Fighter.LaunchedTurnSpells.Where(Function(us) us.SpellId = Effect.SpellID).Count() > 1 Then
                Fail()
            End If

        End Sub

    End Class
End Namespace