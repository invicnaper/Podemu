Imports Podemu.Utils
Imports Podemu.World
Imports Podemu.Utils.Basic

Namespace Game
    Partial Public Class SpellEffect

        Public Function GetScore(ByVal TheClient As Fighter, ByVal ListOfCible As List(Of Fighter), ByVal Cell As Integer) As Integer

            Select Case Effect

                Case 77, 84 ' vol PA/PM
                    Return GetStealPointScore(TheClient, ListOfCible)

                Case 91, 92, 93, 94, 95 ' Vol
                    Return GetStealLifeScore(TheClient, ListOfCible)

                Case 96, 97, 98, 99, 100 ' Dégats
                    Return GetDamageScore(TheClient, ListOfCible)

                Case 108 ' Soin
                    Return GetHealthScore(TheClient, ListOfCible)

                Case 168, 168 ' perte PA/PM
                    Return GetLoosePointScore(TheClient, ListOfCible)

                Case 181 'invoc
                    Return GetInvocationPointScore(TheClient)

                Case Else
                    If ListOfCible.Count <> 0 AndAlso (ListOfCible.FirstOrDefault(Function(c) c.Team = TheClient.Team) Is Nothing) Then
                        Return 10 + Basic.Rand(1, 20)
                    End If

            End Select

            Return 0

        End Function

        Public Function GetBaseDamageScore(ByVal TheClient As Fighter, ByVal Player As Fighter) As Integer

            Dim s = TheClient.Stats.Stats
            Dim PercentLife As Integer = Math.Floor((Player.Stats.Life / Player.Stats.MaximumLife) * 100)
            Dim Armor As Integer = s.GetArmor(Effect)
            Dim MinJet As Integer = Basic.GetMinJet(Str)
            Dim MaxJet As Integer = Basic.GetMaxJet(Str)
            Dim MinDamage As Integer = s.ResistDamages(s.GetDamages(MinJet, Effect, False), Effect, False) - Armor
            Dim MaxDamage As Integer = s.ResistDamages(s.GetDamages(MaxJet, Effect, False), Effect, False) - Armor

            Dim result As Integer = Math.Floor(((MinDamage + MaxDamage) / 2) * (100 - PercentLife + 1) * (Tours + 1))

            Return If(TheClient.Team <> Player.Team, result, result * -1)

        End Function

        Public Function GetStealLifeScore(ByVal TheClient As Fighter, ByVal ListOfCible As List(Of Fighter)) As Integer

            Dim Score As Integer = 0
            Dim PercentLife As Integer = Math.Floor((TheClient.Stats.Life / TheClient.Stats.MaximumLife) * 100)

            For Each Player As Fighter In ListOfCible

                Score += GetBaseDamageScore(TheClient, Player) * (100 - PercentLife + 1)

            Next

            Return Score

        End Function

        Public Function GetDamageScore(ByVal TheClient As Fighter, ByVal ListOfCible As List(Of Fighter)) As Integer

            Dim Score As Integer = 0
            Dim PercentLife As Integer = Math.Floor((TheClient.Stats.Life / TheClient.Stats.MaximumLife) * 100)

            For Each Player As Fighter In ListOfCible

                Score += GetBaseDamageScore(TheClient, Player)

            Next

            Return Score

        End Function

        Private Function GetHealthScore(ByVal TheClient As Fighter, ByVal ListOfCible As List(Of Fighter)) As Integer

            Dim Score As Integer = 0
            Dim s = TheClient.Stats.Stats

            Dim MinJet As Integer = Basic.GetMinJet(Str)
            Dim MaxJet As Integer = Basic.GetMaxJet(Str)
            Dim MinDamage As Integer = s.GetSoin(MinJet)
            Dim MaxDamage As Integer = s.GetSoin(MaxJet)

            Dim result As Integer = Math.Round((MinDamage + MaxDamage) / 2) * (Tours + 1)

            For Each Player As Fighter In ListOfCible

                Dim PercentLife As Integer = Math.Floor((Player.Stats.Life / Player.Stats.MaximumLife) * 100)

                If Player.Team = TheClient.Team Then
                    Score += result * (100 - PercentLife + 1)
                Else
                    Score -= result * (100 - PercentLife + 1)
                End If

            Next

            Return Score

        End Function

        Private Function GetStealPointScore(ByVal TheClient As Fighter, ByVal ListOfCible As List(Of Fighter)) As Integer

            Dim Score As Integer = 0
            Dim s = TheClient.Stats.Stats

            Dim MinJet As Integer = Basic.GetMinJet(Str)
            Dim MaxJet As Integer = Basic.GetMaxJet(Str)
            Dim MinDamage As Integer = s.GetSoin(MinJet)
            Dim MaxDamage As Integer = s.GetSoin(MaxJet)

            Dim result As Integer = Math.Round((MinDamage + MaxDamage) / 2) * (Tours + 1)

            Dim EsquiveLanceur As Integer = s.Base.Sagesse.Total / 4 + 1

            For Each player In ListOfCible

                Dim EsquiveCible As Integer = If(Effect = Effect.VolPM, player.Stats.EsquivePM, player.Stats.EsquivePA) + 1
                Dim PourcentagePointRestant As Double = If(Effect = Effect.VolPM, player.PM, player.PA) / If(Effect = Effect.VolPM, player.Stats.MaxPM, player.Stats.MaxPA)
                Dim Chance As Double = (1 / 2) * (EsquiveLanceur / EsquiveCible) * PourcentagePointRestant

                If player.Team = TheClient.Team Then
                    Score += result * Chance * 4
                Else
                    Score -= result * Chance * 4
                End If

            Next

            Return Score

        End Function

        Private Function GetLoosePointScore(ByVal TheClient As Fighter, ByVal ListOfCible As List(Of Fighter)) As Integer

            Dim Score As Integer = 0
            Dim s = TheClient.Stats.Stats

            Dim MinJet As Integer = Basic.GetMinJet(Str)
            Dim MaxJet As Integer = Basic.GetMaxJet(Str)
            Dim MinDamage As Integer = s.GetSoin(MinJet)
            Dim MaxDamage As Integer = s.GetSoin(MaxJet)

            Dim result As Integer = Math.Round((MinDamage + MaxDamage) / 2) * (Tours + 1)

            Dim EsquiveLanceur As Integer = s.Base.Sagesse.Total / 4 + 1

            For Each player In ListOfCible

                Dim EsquiveCible As Integer = If(Effect = Effect.VolPM, player.Stats.EsquivePM, player.Stats.EsquivePA) + 1
                Dim PourcentagePointRestant As Double = If(Effect = Effect.VolPM, player.PM, player.PA) / If(Effect = Effect.VolPM, player.Stats.MaxPM, player.Stats.MaxPA)
                Dim Chance As Double = (1 / 2) * (EsquiveLanceur / EsquiveCible) * PourcentagePointRestant

                If player.Team = TheClient.Team Then
                    Score += result * Chance * 2
                Else
                    Score -= result * Chance * 2
                End If

            Next

            Return Score

        End Function

        Private Function GetInvocationPointScore(ByVal TheClient As Fighter) As Integer

            Dim Template = MonstersHandler.GetTemplate(Value)

            Dim MonsterGrade = Template.LevelList.FirstOrDefault(Function(l) l.Grade = Value2)

            Return 1000 + 10 * (MonsterGrade.Level + MonsterGrade.VieMaximum)

        End Function

    End Class
End Namespace