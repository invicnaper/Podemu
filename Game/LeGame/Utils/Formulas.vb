Imports Podemu.Game
Imports Podemu.World

Namespace Utils
    Public Class Formulas

        Private Shared GroupRates() As Integer = {0, 1, 1.1, 1.5, 2.3, 3.1, 3.6, 4.2, 4.7}
        Private Shared _randomepriceHDV As Integer

        Shared Property RandomepriceHDV(item As Item) As Integer
            Get
                Return _randomepriceHDV
            End Get
            Set(value As Integer)
                _randomepriceHDV = value
            End Set
        End Property

        Public Shared Function ChanceTacle(ByVal Tacle As Fighter, ByVal Tacleur As Fighter) As Integer

            Dim Agi As Integer = Tacle.Stats.Stats.Base.Agilite.Total
            Dim AgiEnnemi As Integer = Tacleur.Stats.Stats.Base.Agilite.Total

            'Plus équilibré
            'Dim Chance = Math.Atan(Math.Tan(3 * Math.PI / 10) * Math.Abs(Agi - AgiEnnemi) / 100 / Math.PI + 0.5)

            Dim Chance As Integer = ((Agi + 25) ^ 2 / ((Agi + 25) ^ 2 + (AgiEnnemi + 25) ^ 2)) * 100
            If Chance < 0 Then Chance = 0
            If Chance > 100 Then Chance = 100
            Return Chance

        End Function

        Public Shared Function PointLost(ByVal Lanceur As Fighter, ByVal Receiver As Fighter, ByVal Points As Integer, ByVal PM As Boolean) As Integer

            Dim EsquiveLanceur As Integer = Lanceur.Stats.Stats.Base.Sagesse.Total / 4 + 1
            Dim EsquiveCible As Integer = If(PM, Receiver.Stats.EsquivePM, Receiver.Stats.EsquivePA) + 1

            Dim LostPoint As Integer = 0

            For i As Integer = 1 To Points

                Dim PointActuel As Integer = If(PM, Receiver.Stats.MaxPM, Receiver.Stats.MaxPA) - LostPoint
                Dim PourcentagePointRestant As Double = PointActuel / If(PM, Receiver.Stats.MaxPM, Receiver.Stats.MaxPA)
                Dim Chance As Double = (1 / 2) * (EsquiveLanceur / EsquiveCible) * PourcentagePointRestant
                Dim PercentChance As Integer = Chance * 100
                If PercentChance > 100 Then PercentChance = 100
                If PercentChance < 0 Then PercentChance = 0

                If Basic.Rand(0, 99) < PercentChance Then LostPoint += 1

            Next

            Return LostPoint

        End Function

        Public Shared Function PvpExp(ByVal Levels() As Integer, ByVal NumPlayer As Integer, ByVal MaxLevel As Integer, ByVal Level As Integer, ByVal Sagesse As Integer) As Long

            If Config.GetItem("RATES_PVP") <> "0" Then

                Dim TotalLevel As Double = 0
                For Each Lvl As Integer In Levels
                    TotalLevel += Lvl ^ 2
                Next

                Dim Exp As Double = Math.Sqrt(TotalLevel) ^ 3
                Exp /= NumPlayer

                Dim Rent As Integer = MaxLevel - Level
                If Rent > 5 Then Exp /= (Rent / 5)

                Exp *= (1 + Sagesse * 0.01)
                Exp *= Config.GetItem("RATES_PVP")

                Return Math.Ceiling(Exp)

            Else

                Return 0

            End If

        End Function

        Public Shared Function PvmExp(ByVal Mobs() As Fighter, ByVal Players() As Fighter, ByVal Player As Fighter, ByVal MaxLevel As Integer) As Long

            If Config.GetItem("RATES_PVM") <> "0" Then

                Dim BaseExp As Double = 0
                For Each Mob As Fighter In Mobs
                    BaseExp += ExperienceTable.AtLevel(If(Mob.Stats.Level + 1 > ExperienceTable.MaxLevel, ExperienceTable.MaxLevel, Mob.Stats.Level + 1)).Character / Player.Stats.Level
                Next

                Dim Exp As Double = Math.Sqrt(BaseExp)

                If Players.Length > 1 Then
                    Dim GroupNumber As Integer = 0
                    Dim Quota As Integer = Math.Floor(MaxLevel / 3)
                    For Each iPlay As Fighter In Players
                        If iPlay.Stats.Level >= Quota Then GroupNumber += 1
                    Next
                    Exp *= GroupRates(GroupNumber)
                    Exp /= Players.Length
                End If

                Exp *= (1 + Player.Stats.Stats.Base.Sagesse.TotalWithoutBoosts * 0.01)
                Exp *= Config.GetItem("RATES_PVM")

                Return Math.Ceiling(Exp)

            Else

                Return 0

            End If

        End Function


        Public Shared Function MountExp(ByVal Player As Fighter, ByRef PlayerXp As Long, ByVal Mount As Mount) As Long

            If Config.GetItem("RATES_MOUNT") <> "0" Then

                Dim diff As Integer = Math.Abs(Player.Stats.Level - Mount.Level)
                Dim coeff As Double = 0

                If diff <= 10 Then
                    coeff = 0.1
                ElseIf diff <= 20 Then
                    coeff = 0.08
                ElseIf diff <= 30 Then
                    coeff = 0.06
                ElseIf diff <= 40 Then
                    coeff = 0.04
                ElseIf diff <= 50 Then
                    coeff = 0.03
                ElseIf diff <= 60 Then
                    coeff = 0.02
                ElseIf diff <= 70 Then
                    coeff = 0.015
                Else
                    coeff = 0.01
                End If

                Dim mountXp As Double = PlayerXp * ((Mount.Percent + 20) / 100) * coeff

                PlayerXp = Math.Ceiling(PlayerXp * (Mount.Percent / 100))

                mountXp *= Config.GetItem("RATES_MOUNT")

                Return Math.Ceiling(mountXp)

            Else

                Return 0

            End If

        End Function


        Public Shared Function HonorExp(ByVal Level As Integer, ByVal LevelsWinner As Integer, ByVal LevelsLoosers As Integer) As Integer

            Dim Base As Double = Math.Sqrt(Level) * 10
            Dim Coef As Double = LevelsLoosers / LevelsWinner

            Dim Honor As Integer = Math.Floor(Base * Coef)

            Return Honor

        End Function

        Public Shared Function HonorLooseExp(ByVal Level As Integer, ByVal LevelsWinner As Integer, ByVal LevelsLoosers As Integer) As Integer

            Dim Base As Double = Math.Sqrt(Level) * 10
            Dim Coef As Double = LevelsLoosers / LevelsWinner

            Dim Honor As Integer = Math.Floor(Base * Coef)

            Return Honor

        End Function

    End Class
End Namespace