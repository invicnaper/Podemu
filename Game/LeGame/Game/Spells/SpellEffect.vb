﻿Imports Podemu.World
Imports Podemu.Utils

Namespace Game
    Public Class SpellEffect

        Public SpellID As Integer = 0
        Public Effect As Effect = 0
        Public Str As String = "1d5+0"
        Public Tours As Integer = 0
        Public Value As Integer = 0, Value2 As Integer, Value3 As Integer = 0
        Public Chance As Integer = 0

        Public Cibles As New SpellTarget

        Public ActualValue, MaxValue As Integer

        Public ReadOnly Property IsDegatsEffect() As Boolean
            Get
                Select Case Effect
                    Case 91, 92, 93, 94, 95, 96, 97, 98, 99, 100
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property

        Public Sub UseEffect(ByVal Player As Fighter, ByVal Cibles As List(Of Fighter), ByVal Cell As Integer, ByVal Trap As Boolean)
            Dim NewEffect As New EffectTrigger(Me, Player, Cibles, Cell, Effect)
            NewEffect.Trap = Trap
            NewEffect.UseEffect()
        End Sub

    End Class
End Namespace