﻿Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class Econome : Inherits Challenge

        Sub New(ByVal Fight As Fight)
            MyBase.New(5, Fight)

            BasicDropBonus = 10
            BasicXpBonus = 10

            TeamDropBonus = 15
            TeamXpBonus = 15

            ShowTarget = False
            TargetId = 0

        End Sub

        Private UsedSpells As New Dictionary(Of Fighter, List(Of Integer))

        Public Overrides Sub CheckSpell(ByVal Fighter As World.Fighter, ByVal Effect As SpellEffect, ByVal Cibles As System.Collections.Generic.List(Of World.Fighter), ByVal Cell As Integer)

            If Not UsedSpells.ContainsKey(Fighter) Then
                UsedSpells.Add(Fighter, New List(Of Integer))
                Exit Sub
            End If

            If UsedSpells(Fighter).Contains(Effect.SpellID) AndAlso Fighter.LaunchedTurnSpells.LongCount(Function(e) e.SpellId = Effect.SpellID) <= 1 Then
                Fail()
            Else
                UsedSpells(Fighter).Add(Effect.SpellID)
            End If

        End Sub

    End Class
End Namespace