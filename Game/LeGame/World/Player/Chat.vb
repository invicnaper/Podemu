﻿Imports Podemu.Game
Imports Podemu.Utils

Namespace World
    Public Class Chat

        Public Shared Sub SendPrivateMessage(ByVal Client As GameClient, ByVal Receiver As String, ByVal Message As String)

            Dim ReceiveClient As GameClient = Players.GetCharacter(Receiver)

            If Not ReceiveClient Is Nothing Then

                If Not ReceiveClient.Character.State.IsAway Then
                    '(Client.Infos.GmLevel > 3


                    ReceiveClient.Character.SendWatch("Chat", "de " & Client.Character.Name & " : " & Message.Split("|")(0))
                    ReceiveClient.Send("cMKF|" & Client.Character.ID & "|" & Client.Character.Name & "|" & Message)
                    Client.Send("cMKT|" & Client.Character.ID & "|" & ReceiveClient.Character.Name & "|" & Message)


                Else
                    Client.Send("Im114" & ReceiveClient.Character.Name)
                End If

            Else
                Client.Send("cMEf" & Receiver)
            End If

        End Sub

        Public Shared Sub SendMapMessage(ByVal Client As GameClient, ByVal Message As String)
            If ForbiddenWords.IfContainsForbiddenWord(Message) Then
                Client.SendNormalMessage("<b>Message impossible</b> , votre phrase contient un <b>mot interdit</b> par l'administrateur !")
                Exit Sub
            End If
            Client.Character.GetMap.Send("cMK|" & Client.Character.ID & "|" & Client.Character.Name & "|" & Message)

        End Sub

        Public Shared Sub SendBattleMessage(ByVal Client As GameClient, ByVal Message As String)

            Client.Character.State.GetFight.Send("cMK|" & Client.Character.ID & "|" & Client.Character.Name & "|" & Message)

        End Sub

        Public Shared Sub SendTeamMessage(ByVal Client As GameClient, ByVal Message As String)

            Client.Character.State.GetFight.Send("cMK#|" & Client.Character.ID & "|" & _
                    Client.Character.Name & "|" & Message, Client.Character.State.GetFighter.Team)

        End Sub

        Public Shared Sub SendRecruitmentMessage(ByVal Client As GameClient, ByVal Message As String)
            If ForbiddenWords.IfContainsForbiddenWord(Message) Then
                Client.SendNormalMessage("<b>Message impossible</b> , votre phrase contient un <b>mot interdit</b> par l'administrateur !")
                Exit Sub
            End If
            If Client.Spam.CanRecruitment Then
                Client.Spam.SetRecruitment()
                Players.Send("cMK?|" & Client.Character.ID & "|" & Client.Character.Name & "|" & Message)
            Else
                Client.Send("Im0115;" & Client.Spam.TimeRecruitment)
            End If

        End Sub

        Public Shared Sub SendSerianeMessage(ByVal Client As GameClient, ByVal Message As String)

            If Client.Character.Player.Alignment.Id = 3 Or Client.Infos.GmLevel > 0 Then
                For Each Player As GameClient In Players.GetPlayers
                    If Player.Infos.GmLevel > 0 Or Player.Character.Player.Alignment.Id = 3 Then
                        Player.Send("cMK!|" & Client.Character.ID & "|" & Client.Character.Name & "|" & Message)
                    End If
                Next
            Else
                Client.SendMessage("Vous n'êtes pas d'alignement Mercenaire !")
            End If

        End Sub

        Public Shared Sub SendTradeMessage(ByVal Client As GameClient, ByVal Message As String)
            If ForbiddenWords.IfContainsForbiddenWord(Message) Then
                Client.SendNormalMessage("<b>Message impossible</b> , votre phrase contient un <b>mot interdit</b> par l'administrateur !")
                Exit Sub
            End If
            If Client.Spam.CanCommerce Then
                Client.Spam.SetCommerce()
                Players.Send("cMK:|" & Client.Character.ID & "|" & Client.Character.Name & "|" & Message)
            Else
                Client.Send("Im0115;" & Client.Spam.TimeCommerce)
            End If

        End Sub

        Public Shared Sub SendPartyMessage(ByVal Client As GameClient, ByVal Message As String)

            If Client.Character.State.InParty Then
                Client.Character.State.GetParty.Send("cMK$|" & Client.Character.ID & "|" & Client.Character.Name & "|" & Message)
            Else
                Client.Send("cMK$|" & Client.Character.ID & "|" & Client.Character.Name & "|" & Message)
            End If

        End Sub

        Public Shared Sub SendAdminMessage(ByVal Client As GameClient, ByVal Message As String)

            If Client.Infos.GmLevel > 0 Then
                For Each Player As GameClient In Players.GetPlayers
                    If Player.Infos.GmLevel > 0 Then
                        Player.Send("cMK@|" & Client.Character.ID & "|" & Client.Character.Name & "|" & Message)
                    End If
                Next
            End If

        End Sub

        Public Shared Sub SendGuildMessage(ByVal Client As GameClient, ByVal Message As String)

            If Not Client.Character.GuildID = "0" Then
                For Each Player As GameClient In Players.GetPlayers
                    If Player.Character.GuildID = Client.Character.GuildID Then
                        Player.Send("cMK%|" & Client.Character.ID & "|" & Client.Character.Name & "|" & Message)
                    End If
                Next
            End If

        End Sub

        Public Shared Sub SendAdministratorMessage(ByVal Client As GameClient, ByVal Message As String)

            Players.SendMessage("(Staff) <b>" & Client.Character.Name & "</b> : " & Message)

        End Sub

        Public Shared Sub SendEventMessage(ByVal Message As String)

            Players.SendMessage("(<b>Event</b>) : " & Message)

        End Sub

        Public Shared Sub SendPubMessage(ByVal Message As String)

            Players.SendPubMessage("(<b>Pub</b>) : " & Message)

        End Sub
        Public Shared Sub SendPublMessage(ByVal Envoyeur As String, ByVal Message As String)

            Players.SendMessage("(<b>(PUB)</b> : " & Message)

        End Sub

        Public Shared Sub SendGlobalMessage(ByVal Envoyeur As String, ByVal Message As String)

            Players.SendMessage("(GloBalChat) <b><i>" & Envoyeur & "</i></b> : " & Message)

        End Sub
        Public Shared Sub SendAidmodoMessage(ByVal Message As String)

            Players.SendAideMessage("(<b>Aide</b>) : " & Message)

        End Sub
        Public Shared Sub SendAideMessage(ByVal Envoyeur As String, ByVal Message As String)

            Players.SendMessage("(AideChat) <b><i>" & Envoyeur & "</i></b> : " & Message)

        End Sub

        Public Shared Sub SendAdministratorMapMessage(ByVal Client As GameClient, ByVal Message As String)

            Client.Character.GetMap.SendMessage("(Map) <b>" & Client.Character.Name & "</b> : " & Message)

        End Sub

    End Class
End Namespace