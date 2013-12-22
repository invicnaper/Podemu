﻿Imports Podemu.Game
Imports Podemu.Utils

Namespace World

    Public Enum BuffType
        Damage
        Stats
        Attack
        BuffOnAttack
        Skin
        None
    End Enum

    Public Class EffectBuff

        Public Sub New(ByVal Trigger As EffectTrigger, ByVal Cible As Fighter, ByVal Type As BuffType, ByVal OverrideEffect As Effect)
            Me.Base = Trigger
            Me.Type = Type
            Turns = Trigger.Tours
            Trigger.Tours = 0
            Trigger.MyEffect = OverrideEffect

            Trigger.Cibles.Clear()
            If Type = BuffType.Attack Then
                Trigger.Player = Cible
            ElseIf Type = BuffType.BuffOnAttack Then
                Trigger.Player = Cible
                Trigger.Cibles.Add(Cible)
                MaxValue = Trigger.Base.Value2
            Else
                Trigger.Cibles.Add(Cible)
            End If
        End Sub

        Private Base As EffectTrigger
        Public Type As BuffType
        Public Turns As Integer = -1

        Public ActualValue As Integer = 0
        Public MaxValue As Integer = 0

        Public Sub Execute(ByVal All As Boolean)
            Turns -= 1
            If Type = BuffType.Damage Then
                Base.UseEffect()
            ElseIf Type = BuffType.Stats And Turns < 0 Then
                Base.BuffEffects(True)
            ElseIf Type = BuffType.Skin And Turns < 0 And Not All Then
                Base.ChangeSkin(True)
            End If
        End Sub

        Public Sub OnAttack(ByVal Cible As Fighter, ByVal Value As Integer)

            Dim NewBuff As EffectTrigger = Base.Copy

            If Type = BuffType.BuffOnAttack Or Type = BuffType.Attack Then
                NewBuff.MyEffect = Base.Value
                NewBuff.Value = Value
                NewBuff.Value2 = -1
                NewBuff.Value3 = -1
            End If

            If Type = BuffType.Attack Then

                NewBuff.Cibles.Clear()
                NewBuff.Cibles.Add(Cible)
                NewBuff.UseEffect()

            ElseIf Type = BuffType.BuffOnAttack Then

                If MaxValue > 0 Then
                    If (MaxValue - ActualValue) < Value Then
                        Value = (MaxValue - ActualValue)
                    End If
                    ActualValue += Value
                End If

                NewBuff.BuffEffects(False)

            End If

        End Sub

    End Class
End Namespace