Namespace World
    Public Class FighterBuffs

        Private Buffs As New List(Of EffectBuff)

        Public Sub Add(ByVal Effect As EffectTrigger, ByVal Cible As Fighter, ByVal Type As BuffType, Optional ByVal OverrideEffect As Game.Effect = Game.Effect.None)

            Dim PacketBuff As String = "GIE" & Effect.MyEffect
            PacketBuff &= ";" & Cible.Id
            PacketBuff &= ";" & Effect.Value
            PacketBuff &= ";" & If(Effect.Value2 <> -1, Effect.Value2.ToString, "")
            PacketBuff &= ";" & If(Effect.Value3 <> -1, Effect.Value3.ToString, "")
            PacketBuff &= ";" & If(Effect.Base.Chance <> -1, Effect.Base.Chance.ToString, "")
            PacketBuff &= ";" & If(Effect.Base.Tours > 1, Effect.Base.Tours, 1)
            PacketBuff &= ";" & Effect.Base.SpellID

            Effect.Player.Fight.Send(PacketBuff)

            Buffs.Add(New EffectBuff(Effect, Cible, Type, If(OverrideEffect = Game.Effect.None, Effect.MyEffect, OverrideEffect)))

        End Sub

        Public Sub Execute()
            Dim CopyBuffs As New List(Of EffectBuff)
            CopyBuffs.AddRange(Buffs)

            For Each Buff As EffectBuff In CopyBuffs
                Buff.Execute(False)
                If Buff.Turns < 0 Then
                    Buffs.Remove(Buff)
                End If
            Next
        End Sub

        Public Sub ExecuteOnAttack(ByVal Attacker As Fighter, ByVal Value As Integer)

            Dim CopyBuffs As New List(Of EffectBuff)
            CopyBuffs.AddRange(Buffs)

            For Each Buff As EffectBuff In CopyBuffs
                Buff.OnAttack(Attacker, Value)
            Next

        End Sub

        Public Sub DeleteAll()
            Dim CopyBuffs As New List(Of EffectBuff)
            CopyBuffs.AddRange(Buffs)

            For Each Buff As EffectBuff In CopyBuffs
                Buff.Turns = 0
                Buff.Execute(True)
                Buffs.Remove(Buff)
            Next
        End Sub

    End Class
End Namespace