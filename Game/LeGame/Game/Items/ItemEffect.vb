Imports Podemu.Utils
Imports Podemu.Utils.Basic

Namespace Game
    Public Class ItemEffect

        Public Item As Item
        Public EffectID As Effect = Effect.None
        Public Value1 As Integer = -1
        Public Value2 As Integer = -1
        Public Value3 As Integer = -1
        Public EffectStr As String = "0d0+0"

        Public ReadOnly Property IsWeaponEffect() As Boolean
            Get
                Select Case EffectID
                    Case 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property

        Public Function HoldEffect(Optional ByVal MaxMin As Integer = 0) As Boolean

            Select Case EffectID

                Case 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101

                Case 800 ' Pdvs des familiers
                    Return False

                Case Else
                    Value1 = GetRandomJet(EffectStr, MaxMin)
                    Value2 = -1

            End Select

            Return True

        End Function

        Public Overrides Function ToString() As String

            Dim Value1String As String = IIf(Value1 <= 0, "", DeciToHex(Value1))
            Dim Value2String As String = IIf(Value2 <= 0, "", DeciToHex(Value2))
            Dim Value3String As String = IIf(Value3 <= 0, "", DeciToHex(Value3))

            Return DeciToHex(EffectID) & "#" & Value1String & "#" & _
                Value2String & "#" & Value3String & "#" & EffectStr
        End Function

        Public Function Multiply(ByVal multiple As Integer) As ItemEffect
            Dim NewEffect As New ItemEffect()

            NewEffect.EffectID = EffectID
            NewEffect.Item = Item
            NewEffect.Value1 = Value1 * multiple
            NewEffect.Value2 = Value2 * multiple
            NewEffect.Value3 = Value3 * multiple

            Dim str As String = EffectStr

            If (str <> "") Then
                Dim min = Integer.Parse(str.Split("d")(0)) * multiple
                Dim max = Integer.Parse(str.Split("d")(1).Split("+")(0)) * multiple
                Dim plus = Integer.Parse(str.Split("d")(1).Split("+")(1)) * multiple

                NewEffect.EffectStr = min & "d" & max & "+" & plus
            Else
                NewEffect.EffectStr = ""
            End If

            Return NewEffect
        End Function

        Public Shared Function FromString(ByVal Item As Item, ByVal data As String) As ItemEffect

            Dim NewEffect As New ItemEffect
            Dim EffectData() As String = data.Split("#")

            NewEffect.Item = Item
            NewEffect.EffectID = Basic.HexToDeci(EffectData(0))

            If EffectData.Length > 1 Then
                NewEffect.Value1 = Basic.HexToDeci(EffectData(1))
            End If
            If EffectData.Length > 2 Then
                NewEffect.Value2 = Basic.HexToDeci(EffectData(2))
            End If
            If EffectData.Length > 3 Then
                NewEffect.Value3 = Basic.HexToDeci(EffectData(3))
            End If
            If EffectData.Length > 4 Then
                NewEffect.EffectStr = EffectData(4)
            End If

            Return NewEffect
        End Function

        Public Function Copy() As ItemEffect

            Dim NewItemEffect As New ItemEffect
            NewItemEffect.Item = Item
            NewItemEffect.EffectID = Me.EffectID
            NewItemEffect.EffectStr = Me.EffectStr
            NewItemEffect.Value1 = Me.Value1
            NewItemEffect.Value2 = Me.Value2
            NewItemEffect.Value3 = Me.Value3
            Return NewItemEffect

        End Function

        Public Sub UseEffect(ByVal Player As StatsPlayer)

            If IsWeaponEffect Then Exit Sub

            Select Case EffectID

                Case Effect.AddAgilite
                    Player.Stats.Base.Agilite.Items += Value1
                Case Effect.AddChance
                    Player.Stats.Base.Chance.Items += Value1
                Case Effect.AddForce
                    Player.Stats.Base.Force.Items += Value1
                Case Effect.AddIntelligence
                    Player.Stats.Base.Intelligence.Items += Value1
                Case Effect.AddVitalite
                    Player.Stats.Base.Vitalite.Items += Value1
                Case Effect.AddSagesse
                    Player.Stats.Base.Sagesse.Items += Value1
                Case Effect.AddLife
                    Player.Stats.Vie.Items += Value1
                Case Effect.AddPA, Effect.AddPABis
                    Player.Stats.PA.Items += Value1
                Case Effect.AddPM
                    Player.Stats.PM.Items += Value1
                Case Effect.AddPO
                    Player.Stats.PO.Items += Value1
                Case Effect.AddInvocationMax
                    Player.Stats.MaxInvocation.Items += Value1
                Case Effect.AddInitiative
                    Player.Stats.Initiative.Items += Value1
                Case Effect.AddProspection
                    Player.Stats.Prospection.Items += Value1

                Case Effect.SubAgilite
                    Player.Stats.Base.Agilite.Items -= Value1
                Case Effect.SubChance
                    Player.Stats.Base.Chance.Items -= Value1
                Case Effect.SubForce
                    Player.Stats.Base.Force.Items -= Value1
                Case Effect.SubIntelligence
                    Player.Stats.Base.Intelligence.Items -= Value1
                Case Effect.SubVitalite
                    Player.Stats.Base.Vitalite.Items -= Value1
                Case Effect.SubSagesse
                    Player.Stats.Base.Sagesse.Items -= Value1
                Case Effect.SubPA
                    Player.Stats.PA.Items -= Value1
                Case Effect.SubPM
                    Player.Stats.PM.Items -= Value1
                Case Effect.SubPO
                    Player.Stats.PO.Items -= Value1
                Case Effect.SubInitiative
                    Player.Stats.Initiative.Items -= Value1
                Case Effect.SubProspection
                    Player.Stats.Prospection.Items -= Value1

                Case Effect.AddReduceDamageAir
                    Player.Stats.Armor.Resistances.Air.Items += Value1
                Case Effect.AddReduceDamageEau
                    Player.Stats.Armor.Resistances.Eau.Items += Value1
                Case Effect.AddReduceDamageFeu
                    Player.Stats.Armor.Resistances.Feu.Items += Value1
                Case Effect.AddReduceDamageTerre
                    Player.Stats.Armor.Resistances.Terre.Items += Value1
                Case Effect.AddReduceDamageNeutre
                    Player.Stats.Armor.Resistances.Neutre.Items += Value1

                Case Effect.AddReduceDamagePvPAir
                    Player.Stats.Armor.ResistancesPvp.Air.Items += Value1
                Case Effect.AddReduceDamagePvPEau
                    Player.Stats.Armor.ResistancesPvp.Eau.Items += Value1
                Case Effect.AddReduceDamagePvPFeu
                    Player.Stats.Armor.ResistancesPvp.Feu.Items += Value1
                Case Effect.AddReduceDamagePvPTerre
                    Player.Stats.Armor.ResistancesPvp.Terre.Items += Value1
                Case Effect.AddReduceDamagePvPNeutre
                    Player.Stats.Armor.ResistancesPvp.Neutre.Items += Value1

                Case Effect.AddReduceDamagePourcentAir
                    Player.Stats.Armor.Resistances.PercentAir.Items += Value1
                Case Effect.AddReduceDamagePourcentEau
                    Player.Stats.Armor.Resistances.PercentEau.Items += Value1
                Case Effect.AddReduceDamagePourcentFeu
                    Player.Stats.Armor.Resistances.PercentFeu.Items += Value1
                Case Effect.AddReduceDamagePourcentTerre
                    Player.Stats.Armor.Resistances.PercentTerre.Items += Value1
                Case Effect.AddReduceDamagePourcentNeutre
                    Player.Stats.Armor.Resistances.PercentNeutre.Items += Value1

                Case Effect.AddReduceDamagePourcentPvPAir
                    Player.Stats.Armor.ResistancesPvp.PercentAir.Items += Value1
                Case Effect.AddReduceDamagePourcentPvPEau
                    Player.Stats.Armor.ResistancesPvp.PercentEau.Items += Value1
                Case Effect.AddReduceDamagePourcentPvPFeu
                    Player.Stats.Armor.ResistancesPvp.PercentFeu.Items += Value1
                Case Effect.AddReduceDamagePourcentPvPTerre
                    Player.Stats.Armor.ResistancesPvp.PercentTerre.Items += Value1
                Case Effect.AddReduceDamagePourcentPvpNeutre
                    Player.Stats.Armor.ResistancesPvp.PercentNeutre.Items += Value1

                Case Effect.AddReduceDamagePhysic
                    Player.Stats.Armor.ReductionPhysique.Items += Value1
                Case Effect.AddReduceDamageMagic
                    Player.Stats.Armor.ReductionMagique.Items += Value1
                Case Effect.AddEsquivePA
                    Player.Stats.Armor.EsquivePA.Items += Value1
                Case Effect.AddEsquivePM
                    Player.Stats.Armor.EsquivePM.Items += Value1

                Case Effect.SubReduceDamageAir
                    Player.Stats.Armor.Resistances.Air.Items -= Value1
                Case Effect.SubReduceDamageEau
                    Player.Stats.Armor.Resistances.Eau.Items -= Value1
                Case Effect.SubReduceDamageFeu
                    Player.Stats.Armor.Resistances.Feu.Items -= Value1
                Case Effect.SubReduceDamageTerre
                    Player.Stats.Armor.Resistances.Terre.Items -= Value1
                Case Effect.SubReduceDamageNeutre
                    Player.Stats.Armor.Resistances.Neutre.Items -= Value1

                Case Effect.SubReduceDamagePourcentAir
                    Player.Stats.Armor.Resistances.PercentAir.Items -= Value1
                Case Effect.SubReduceDamagePourcentEau
                    Player.Stats.Armor.Resistances.PercentEau.Items -= Value1
                Case Effect.SubReduceDamagePourcentFeu
                    Player.Stats.Armor.Resistances.PercentFeu.Items -= Value1
                Case Effect.SubReduceDamagePourcentTerre
                    Player.Stats.Armor.Resistances.PercentTerre.Items -= Value1
                Case Effect.SubReduceDamagePourcentNeutre
                    Player.Stats.Armor.Resistances.PercentNeutre.Items -= Value1

                Case Effect.SubReduceDamagePourcentPvPAir
                    Player.Stats.Armor.ResistancesPvp.PercentAir.Items -= Value1
                Case Effect.SubReduceDamagePourcentPvPEau
                    Player.Stats.Armor.ResistancesPvp.PercentEau.Items -= Value1
                Case Effect.SubReduceDamagePourcentPvPFeu
                    Player.Stats.Armor.ResistancesPvp.PercentFeu.Items -= Value1
                Case Effect.SubReduceDamagePourcentPvPTerre
                    Player.Stats.Armor.ResistancesPvp.PercentTerre.Items -= Value1
                Case Effect.SubReduceDamagePourcentPvpNeutre
                    Player.Stats.Armor.ResistancesPvp.PercentNeutre.Items -= Value1

                Case Effect.SubEsquivePA
                    Player.Stats.Armor.EsquivePA.Items -= Value1
                Case Effect.SubEsquivePM
                    Player.Stats.Armor.EsquivePM.Items -= Value1

                Case Effect.AddRenvoiDamage
                    Player.Stats.Damages.RenvoiDegats.Items += Value1
                Case Effect.AddDamageCritic
                    Player.Stats.Damages.BonusCoupCritique.Items += Value1
                Case Effect.AddEchecCritic
                    Player.Stats.Damages.BonusEchecCritique.Items += Value1
                Case Effect.AddDamage
                    Player.Stats.Damages.BonusDegats.Items += Value1
                Case Effect.AddDamagePercent
                    Player.Stats.Damages.BonusDegatsPercent.Items += Value1
                Case Effect.AddDamagePhysic
                    Player.Stats.Damages.BonusDegatsPhysique.Items += Value1
                Case Effect.AddDamageMagic
                    Player.Stats.Damages.BonusDegatsMagique.Items += Value1
                Case Effect.AddDamagePiege
                    Player.Stats.Damages.BonusDegatsPiege.Items += Value1
                Case Effect.AddDamagePiegePercent
                    Player.Stats.Damages.BonusDegatsPiegePercent.Items += Value1
                Case Effect.AddSoins
                    Player.Stats.Damages.BonusSoins.Items += Value1

                Case Effect.SubDamageCritic
                    Player.Stats.Damages.BonusCoupCritique.Items -= Value1
                Case Effect.SubDamage
                    Player.Stats.Damages.BonusDegats.Items -= Value1
                Case Effect.SubDamagePhysic
                    Player.Stats.Damages.BonusDegatsPhysique.Items -= Value1
                Case Effect.SubDamageMagic
                    Player.Stats.Damages.BonusDegatsMagique.Items -= Value1
                Case Effect.SubSoins
                    Player.Stats.Damages.BonusSoins.Items -= Value1

            End Select

        End Sub

        Public Sub UseUsableEffect(ByVal Client As GameClient, ByVal Player As StatsPlayer)

            Select Case EffectID

                Case Effect.AddLife
                    Dim Value1 As Integer = GetRandomJet(EffectStr)
                    If Value1 > (Player.MaximumLife - Player.Life) Then Value1 = (Player.MaximumLife - Player.Life)
                    Player.Life += Value1
                    Client.Send("Im01;" & Value1)

                Case Effect.AddCharactAgilite
                    Player.Stats.Base.Agilite.Base += Value1
                    Client.Send("Im012;" & Value1)
                Case Effect.AddCharactChance
                    Player.Stats.Base.Chance.Base += Value1
                    Client.Send("Im011;" & Value1)
                Case Effect.AddCharactForce
                    Player.Stats.Base.Force.Base += Value1
                    Client.Send("Im010;" & Value1)
                Case Effect.AddCharactIntelligence
                    Player.Stats.Base.Intelligence.Base += Value1
                    Client.Send("Im014;" & Value1)
                Case Effect.AddCharactVitalite
                    Player.Stats.Base.Vitalite.Base += Value1
                    Client.Send("Im013;" & Value1)
                Case Effect.AddSagesse, Effect.AddCharactSagesse
                    Player.Stats.Base.Sagesse.Base += Value1
                    Client.Send("Im09;" & Value1)
                Case Effect.AddCharactPoint
                    Player.CharactPoint += Value1
                    Client.Send("Im015;" & Value1)
                Case Effect.AddSpellPoint
                    Player.SpellPoint += Value1
                    Client.Send("Im016;" & Value1)

                Case Effect.AddSpell
                    If Value3 > 0 AndAlso SpellsHandler.SpellExist(Value3) Then
                        Client.Character.Spells.AddSpell(Value3, 1)
                        Client.Send("Im03;" & Value3)
                        Client.Send("SL" & Client.Character.Spells.GetAllSpellList)
                    End If

            End Select

        End Sub

        Public Function ConvertToSpellEffect() As SpellEffect

            Dim NewEffect As New SpellEffect
            NewEffect.Effect = EffectID
            NewEffect.Str = EffectStr
            NewEffect.Value = Value1
            NewEffect.Value2 = Value2
            NewEffect.Value3 = Value3
            NewEffect.Cibles.Update(19)
            Return NewEffect

        End Function

    End Class
End Namespace