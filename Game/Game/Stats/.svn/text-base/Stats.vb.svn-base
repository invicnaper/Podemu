Namespace Game
    Public Class Stats

        Public Base As New StatsBase
        Public Armor As New StatsArmor
        Public Damages As New StatsDamage

        Public Initiative, Prospection As New StatsRow

        Public PO, PA, PM As New StatsRow
        Public MaxInvocation As New StatsRow(1)
        Public Vie As New StatsRow

        Public Sub ResetBonus()

            Base.ResetBonus()
            Armor.ResetBonus()
            Damages.ResetBonus()

            Initiative.Boosts = 0
            Prospection.Boosts = 0

            PO.Boosts = 0
            PA.Boosts = 0
            PM.Boosts = 0
            MaxInvocation.Boosts = 0
            Vie.Boosts = 0

        End Sub

        Public Sub ResetItemBonus()

            Base.ResetItemBonus()
            Armor.ResetItemBonus()
            Damages.ResetItemBonus()

            Initiative.Items = 0
            Prospection.Items = 0

            PO.Items = 0
            PA.Items = 0
            PM.Items = 0
            MaxInvocation.Items = 0
            Vie.Items = 0

        End Sub

        Public Function GetDamages(ByVal iBase As Integer, ByVal Effect As Effect, ByVal Trap As Boolean) As Integer

            Select Case Effect

                Case Effect.DamageNeutre, Effect.VolNeutre, Effect.DamageTerre, Effect.VolTerre
                    Return Math.Floor(iBase * (100 + Base.Force.Total + Damages.BonusDegatsPercent.Total + _
                                              If(Trap, Damages.BonusDegatsPiegePercent.Total, 0)) / 100 + _
                                              Damages.BonusDegatsPhysique.Total + Damages.BonusDegats.Total + _
                                              If(Trap, Damages.BonusDegatsPiege.Total, 0))

                Case Effect.DamageFeu, Effect.VolFeu
                    Return Math.Floor(iBase * (100 + Base.Intelligence.Total + Damages.BonusDegatsPercent.Total + _
                                              If(Trap, Damages.BonusDegatsPiegePercent.Total, 0)) / 100 + _
                                              Damages.BonusDegatsMagique.Total + Damages.BonusDegats.Total + _
                                              If(Trap, Damages.BonusDegatsPiege.Total, 0))
                Case Effect.DamageAir, Effect.VolAir
                    Return Math.Floor(iBase * (100 + Base.Agilite.Total + Damages.BonusDegatsPercent.Total + _
                                              If(Trap, Damages.BonusDegatsPiegePercent.Total, 0)) / 100 + _
                                              Damages.BonusDegatsMagique.Total + Damages.BonusDegats.Total + _
                                              If(Trap, Damages.BonusDegatsPiege.Total, 0))
                Case Effect.DamageEau, Effect.VolEau
                    Return Math.Floor(iBase * (100 + Base.Chance.Total + Damages.BonusDegatsPercent.Total + _
                                              If(Trap, Damages.BonusDegatsPiegePercent.Total, 0)) / 100 + _
                                              Damages.BonusDegatsMagique.Total + Damages.BonusDegats.Total + _
                                              If(Trap, Damages.BonusDegatsPiege.Total, 0))

                Case Else
                    Return iBase

            End Select

        End Function

        Public Function ResistDamages(ByVal Damages As Integer, ByVal Effect As Effect, ByVal Pvp As Boolean) As Integer

            Select Case Effect

                Case Effect.DamageNeutre, Effect.VolNeutre
                    Return (Damages * (100 - Armor.Resistances.PercentNeutre.BridTotal - If(Pvp, Armor.ResistancesPvp.PercentNeutre.BridTotal, 0)) / 100) _
                        - Armor.Resistances.Neutre.Total - If(Pvp, Armor.ResistancesPvp.Neutre.Total, 0)

                Case Effect.DamageTerre, Effect.VolTerre
                    Return (Damages * (100 - Armor.Resistances.PercentTerre.BridTotal - If(Pvp, Armor.ResistancesPvp.PercentTerre.BridTotal, 0)) / 100) _
                        - Armor.Resistances.Terre.Total - If(Pvp, Armor.ResistancesPvp.Terre.Total, 0)

                Case Effect.DamageFeu, Effect.VolFeu
                    Return (Damages * (100 - Armor.Resistances.PercentFeu.BridTotal - If(Pvp, Armor.ResistancesPvp.PercentFeu.BridTotal, 0)) / 100) _
                        - Armor.Resistances.Feu.Total - If(Pvp, Armor.ResistancesPvp.Feu.Total, 0)

                Case Effect.DamageAir, Effect.VolAir
                    Return (Damages * (100 - Armor.Resistances.PercentAir.BridTotal - If(Pvp, Armor.ResistancesPvp.PercentAir.BridTotal, 0)) / 100) _
                        - Armor.Resistances.Air.Total - If(Pvp, Armor.ResistancesPvp.Air.Total, 0)

                Case Effect.DamageEau, Effect.VolEau
                    Return (Damages * (100 - Armor.Resistances.PercentEau.BridTotal - If(Pvp, Armor.ResistancesPvp.PercentEau.BridTotal, 0)) / 100) _
                        - Armor.Resistances.Eau.Total - If(Pvp, Armor.ResistancesPvp.Eau.Total, 0)

                Case Else
                    Return Damages

            End Select

        End Function

        Public Function GetArmor(ByVal Effect As Effect) As Integer

            Select Case Effect

                Case Effect.DamageNeutre, Effect.VolNeutre
                    Return Armor.ReductionNeutre.BaseTotal * _
                        Math.Max(1 + Base.Force.BaseTotal / 100, 1 + Base.Force.BaseTotal / 200 + Base.Intelligence.BaseTotal / 200)

                Case Effect.DamageTerre, Effect.VolTerre
                    Return (Armor.ReductionNeutre.BaseTotal + Armor.ReductionTerre.BaseTotal) * _
                        Math.Max(1 + Base.Force.BaseTotal / 100, 1 + Base.Force.BaseTotal / 200 + Base.Intelligence.BaseTotal / 200)

                Case Effect.DamageFeu, Effect.VolFeu
                    Return (Armor.ReductionNeutre.BaseTotal + Armor.ReductionFeu.BaseTotal) * _
                        Math.Max(1 + Base.Intelligence.BaseTotal / 100, 1 + Base.Intelligence.BaseTotal / 200 + Base.Intelligence.BaseTotal / 200)

                Case Effect.DamageEau, Effect.VolEau
                    Return (Armor.ReductionNeutre.BaseTotal + Armor.ReductionEau.BaseTotal) * _
                        Math.Max(1 + Base.Chance.BaseTotal / 100, 1 + Base.Chance.BaseTotal / 200 + Base.Intelligence.BaseTotal / 200)

                Case Effect.DamageAir, Effect.VolAir
                    Return (Armor.ReductionNeutre.BaseTotal + Armor.ReductionAir.BaseTotal) * _
                        Math.Max(1 + Base.Agilite.BaseTotal / 100, 1 + Base.Agilite.BaseTotal / 200 + Base.Intelligence.BaseTotal / 200)

            End Select

        End Function

        Public Function GetSoin(ByVal iBase As Integer) As Integer

            Return Math.Floor(iBase * (100 + Base.Intelligence.Total) / 100 + Damages.BonusSoins.Total)

        End Function

    End Class
End Namespace