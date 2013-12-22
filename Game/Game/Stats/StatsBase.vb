Namespace Game
    Public Class StatsBase

        Public Force, Vitalite, Sagesse, Agilite, Chance, Intelligence As New StatsRow

        Public Sub ResetBonus()
            Force.Boosts = 0
            Vitalite.Boosts = 0
            Sagesse.Boosts = 0
            Agilite.Boosts = 0
            Chance.Boosts = 0
            Intelligence.Boosts = 0
        End Sub

        Public Sub ResetItemBonus()
            Force.Items = 0
            Vitalite.Items = 0
            Sagesse.Items = 0
            Agilite.Items = 0
            Chance.Items = 0
            Intelligence.Items = 0
        End Sub

        Public Sub ResetDon()

        End Sub

    End Class
End Namespace