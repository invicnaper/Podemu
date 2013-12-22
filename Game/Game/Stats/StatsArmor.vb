Namespace Game
    Public Class StatsArmor

        Public ReductionNeutre As New StatsRow
        Public ReductionAir As New StatsRow
        Public ReductionEau As New StatsRow
        Public ReductionFeu As New StatsRow
        Public ReductionTerre As New StatsRow

        Public ReductionPhysique As New StatsRow
        Public ReductionMagique As New StatsRow

        Public EsquivePA As New StatsRow
        Public EsquivePM As New StatsRow

        Public Resistances As New StatsResistance
        Public ResistancesPvp As New StatsResistance

        Public Sub ResetBonus()

            ReductionNeutre.Boosts = 0
            ReductionAir.Boosts = 0
            ReductionEau.Boosts = 0
            ReductionFeu.Boosts = 0
            ReductionTerre.Boosts = 0

            ReductionPhysique.Boosts = 0
            ReductionMagique.Boosts = 0

            EsquivePA.Boosts = 0
            EsquivePM.Boosts = 0

            Resistances.ResetBonus()
            ResistancesPvp.ResetBonus()

        End Sub

        Public Sub ResetItemBonus()

            ReductionNeutre.Items = 0
            ReductionAir.Items = 0
            ReductionEau.Items = 0
            ReductionFeu.Items = 0
            ReductionTerre.Items = 0

            ReductionPhysique.Items = 0
            ReductionMagique.Items = 0

            EsquivePA.Items = 0
            EsquivePM.Items = 0

            Resistances.ResetItemBonus()
            ResistancesPvp.ResetItemBonus()

        End Sub

    End Class
End Namespace