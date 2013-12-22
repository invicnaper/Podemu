Imports Podemu.Utils
Imports Podemu.Game
Imports System.Text
Imports Podemu.World
Imports MySql.Data.MySqlClient
Imports Podemu.World.Players
Imports System.Threading

Namespace World
    Public Class Commands
        Dim mount As Mount
        Shared Cells As String
        Shared MapID As Object
        Shared Packet As Object
        Public Mounts As New List(Of Mount)
        Private Shared _existName As Boolean
        Shared CreateGuild As Integer
        Shared NewGuild As Object
        Shared Receiver As String
        Private Shared _parameters As String

        Private Shared Property GetPlayers As List(Of GameClient)

        Private Shared Property ExistName(ByVal Data As String) As Boolean
            Get
                Return _existName
            End Get
            Set(ByVal value As Boolean)
                _existName = value
            End Set
        End Property

        Private Shared Property ListOfGuild As Object

        Private Shared Property ListOfPlayersGuild As Object

        Private Shared Property GetNewID As Integer

        Private Shared Property Parameters(ByVal p1 As Integer) As String
            Get
                Return _parameters
            End Get
            Set(ByVal value As String)
                _parameters = value
            End Set
        End Property
        Private Shared Sub SendAddMountToShed(ByVal client As GameClient, ByVal Mount As Mount)
            client.Send("Ee+" & Mount.MountInfo)
        End Sub

        Public Shared Sub Respawn(ByVal Client As Game.GameClient)

            If Client.Spam.LastRespawn < Environment.TickCount Then
                If Not Client.Character.State.Occuped Then
                    If Not Client.Character.MapId = Admin.JAIL_MAP Then
                        Client.Spam.LastRespawn = Environment.TickCount + 300000
                        Client.Character.TeleportTo(Config.GetItem("START_MAP"), Config.GetItem("START_CELL"))
                    End If
                Else
                    Client.SendMessage("Impossible de te téléporter: Ton personnage est occupé!<br />Essai de te reconnecter.")
                End If
            Else
                Client.SendMessage("Tu ne peux pas encore utiliser cette commande !")
            End If

        End Sub
        Public Shared Sub SendPrivateMessage(ByVal Client As GameClient, ByVal Receiver As String, ByVal Message As String)
                Dim ReceiveClient As GameClient = Players.GetCharacter(Receiver)

                If Not ReceiveClient Is Nothing Then

                    If Not ReceiveClient.Character.State.IsAway Then
                    ReceiveClient.Character.SendWatch("Chat", "de " & Config.GetItem("NOM_ADMIN") & Message.Split("|")(0))
                    ReceiveClient.Send("cMKF|" & Client.Character.ID & "|" & Config.GetItem("NOM_ADMIN") & "|" & Message)
                        Client.Send("cMKT|" & Client.Character.ID & "|" & ReceiveClient.Character.Name & "|" & Message)
                    Else
                        Client.Send("Im114" & ReceiveClient.Character.Name)
                    End If

                Else
                    Client.Send("cMEf" & Receiver)
                End If
        End Sub
        Public Shared Sub Seriane(ByVal Client As Game.GameClient, ByVal Param As String)

            If Client.Character.Player.Alignment.Id = 3 Then

                If Not Client.Character.State.Occuped Then

                    Select Case Param

                        Case "M"
                            Client.Character.Skin = 8006

                        Case "F"
                            Client.Character.Skin = 8007

                        Case "OFF"
                            Client.Character.Skin = Client.Character.Classe & Client.Character.Sexe

                        Case Else
                            Client.SendMessage("Paramètre invalide : M, F et OFF sont acceptés.")
                            Exit Sub

                    End Select

                    Client.Character.GetMap.RefreshCharacter(Client)

                End If

            Else
                Client.SendMessage("Vous n'êtes pas d'alignement Mercenaire !")
            End If

        End Sub


        Public Shared Sub GetMount(ByVal Client As GameClient, ByVal Data() As String)
            Dim ancestors(14) As MountAncestor
            Dim Mount As New Mount(Client.Character.Name, Data(1), False, Data(2), Rnd(), 0, 0, 0, ancestors)
            Mount.Equip(Client)
        End Sub


        Public Shared Function Parse(ByVal Client As Game.GameClient, ByVal Message As String) As Boolean

            Dim Params() As String = Message.Split()
            Dim Cmd As String = Params(0).ToLower

            Select Case Cmd

                Case "right"
                    Dim player = Players.GetCharacter(CStr(Params(1)))
                    Dim cr = player.Infos.GmLevel
                    player.Infos.GmLevel = Params(2)

                    If cr < Params(2) Then
                        player.Send("BAIO" & Client.Character.Name)
                    ElseIf cr > Params(2) Then
                        player.Send("BAIC" & Client.Character.Name)
                    End If

                Case "start"
                    Respawn(Client)

                Case "createguild"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.Send("gn")
                        Client.SendNormalMessage("Panel ouvert !")
                    End If
                    If Config.GetItem("MODE_LIGHT") = "True" Then
                        Client.SendMessage("Le mode light est True donc y a pas de commandes pour joueurs")
                    End If

                Case "achat"
                    If Params.Length >= 2 Then
                        If IsNumeric(Params(1)) Then
                            Dim Value As Integer = Params(1)
                            Game.StoreManager.BuyItem(Client, Value)
                        End If
                    Else
                        Client.SendMessage("Il faut préciser le numéro de l'objet à acheter !")
                    End If

                Case "restat"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.Character.Restat()
                        Client.SendNormalMessage("Vos <b>caracteristiques</b> ont ete remises a zero !")
                    End If

                Case "demorph"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        If Not Client.Character.State.Occuped Then
                            Client.Character.Skin = Client.Character.Classe & Client.Character.Sexe
                            Client.Character.GetMap.RefreshCharacter(Client)
                        End If
                    End If
                Case "shop"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.Character.TeleportTo(Config.GetItem("SHOP_MAP"), Config.GetItem("SHOP_CELL"))
                    End If
                Case "astrub"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.Character.TeleportTo(Config.GetItem("ASTRUB_MAP"), Config.GetItem("ASTRUB_CELL"))
                    End If
                Case "bonta"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.Character.TeleportTo(Config.GetItem("BONTA_MAP"), Config.GetItem("BONTA_CELL"))
                    End If
                Case "brack"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.Character.TeleportTo(Config.GetItem("BRACK_MAP"), Config.GetItem("BRACK_CELL"))
                    End If
                Case "donjon"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        If Config.GetItem("DJ_ACTIV") = "True" Then
                            Client.Character.TeleportTo(Config.GetItem("DONJON_MAP"), Config.GetItem("DONJON_CELL"))
                        End If
                        If Config.GetItem("DJ_ACTIV") = "False" Then
                            Client.SendNormalMessage("Commande désactiver")
                        End If
                    End If
                Case "pvm"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        If Config.GetItem("PVM_ACTIV") = "False" Then
                            Client.Character.TeleportTo(Config.GetItem("PVM_MAP"), Config.GetItem("PVM_CELL"))
                        End If
                        If Config.GetItem("PVM_ACTIV") = "False" Then
                            Client.SendNormalMessage("Commande désactiver")
                        End If
                    End If
                Case "pvp"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        If Config.GetItem("PVP_ACTIV") = "True" Then
                            Client.Character.TeleportTo(Config.GetItem("PVP_MAP"), Config.GetItem("PVP_CELL"))
                        End If
                        If Config.GetItem("PVP_ACTIV") = "False" Then
                            Client.SendNormalMessage("Commande désactiver")
                        End If
                    End If
                Case "mount"
                    If Params.Length < 2 Then
                        Client.SendMessage("Format : {type} {capacity}")
                    Else
                        GetMount(Client, Params)
                    End If

                Case "point"
                    Client.SendMessage("Vous avez actuellement <b>" & Client.Infos.Points & "</b> " & Config.GetItem("POINTS_NAME") & "<a href="" http://navosoft.net/Groufs"" >(en acheter des points)</a>" & " visiter : " & Config.GetItem("LINK_STOR") & " Pour achter vos points")

                Case Config.GetItem("CMD1_NAM")

                    Config.GetItem("CMD_CODE")

                Case Config.GetItem("CMD_NAM")
                    If Config.GetItem("CMD_ACTIV") = True Then
                        If Config.GetItem("TYPE") = 1 Then
                            Client.Character.TeleportTo(Config.GetItem("NEW_MAP"), Config.GetItem("NEW_CELL"))
                        End If
                        If Config.GetItem("TYPE") = 2 Then
                            Client.SendNormalMessage(Config.GetItem("MSG"))
                        End If
                    End If
                    If Config.GetItem("CMD_ACTIV") = False Then
                        Client.SendNormalMessage("Commande désactiver")
                    End If


                Case "help"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.SendMessage("Liste des <b>Commandes</b> disponible :")
                        Client.SendMessage("<b>.start</b> - Retourne a la map de depart")
                        Client.SendMessage("<b>.point</b> - Permet de savoir votre nombre de points boutiques")
                        Client.SendMessage("<b>.traque</b> - Permet d'avoir une traque")
                        Client.SendMessage("<b>.search</b> - Permet d'allez a votre traque")
                        Client.SendMessage("<b>.infos</b> - Permet de savoir les informations serveur")
                        Client.SendMessage("<b>.fullpdv</b> - Remonter sa vie au max")
                        Client.SendMessage("<b>.gchat</b> - Chat avec les autres serveurs")
                        Client.SendMessage("<b>.restat</b> - Permet de se restat")
                        Client.SendMessage("<b>.brakmarien</b> - Devenir Brackmarien")
                        Client.SendMessage("<b>.bontarien</b> - Devenir bontarien")
                        Client.SendMessage("<b>.neutre</b> - Devenir neutre")
                        Client.SendMessage("<b>.Seriane</b> - Devenir Seriane")
                        Client.SendMessage("<b>.shop</b> - go à la map shop")
                        Client.SendMessage("<b>.astrub</b> - go à astrub")
                        Client.SendMessage("<b>.bonta</b> - go à bonta")
                        Client.SendMessage("<b>.brack</b> - go à brack")
                        Client.SendMessage("<b>.donjon</b> - go donjon 1")
                        Client.SendMessage("<b>.pvp</b> - go pvp")
                        Client.SendMessage("<b>.pvm</b> - go pvm")
                        Client.SendMessage("<b>.guilde</b> - créer une guild")
                        Client.SendMessage("<b>PodEmu</b> - by Invic")
                    End If
                    If Config.GetItem("MODE_LIGHT") = "True" Then
                        Client.SendMessage("Le mode light est True donc y a pas de commandes pour joueurs")
                    End If
                Case "fm"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        MagicalForgeManager.ChangeElement(Client.Character.Items.GetObjectOnPos(1), Client)
                    End If
                Case "getobject"
                    Dim MyItem As Game.Item = Client.Character.Items.GetObjectOnPos(Params(1))
                    Client.SendNormalMessage("ObjectOnPos : " & MyItem.GetTemplate.Name)
                Case "seriane"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        If Utils.Config.GetItem("ACTIVE_SERIANE") Then
                            Client.Character.Player.Alignment.ResetAll()
                            Client.Character.Player.Alignment.Id = 3
                            Client.Character.SendAccountStats()
                            Client.SendNormalMessage("Vous etes désormais d'alignement <b>sériane</b>")
                        Else
                            Client.SendNormalMessage("<b>Alignement sériane</b> non disponible via commande")
                        End If
                    End If
                Case "light"
                    If Config.GetItem("MODE_LIGHT") = "True" Then
                        Client.SendNormalMessage("Mode light est un mode pour les serveurs Anka'lik . vous débutez niveau 1 avec 0 kamas")
                        If Config.GetItem("MODE_LIGHT") = "False" Then
                            Client.SendNormalMessage("Mode light désactiver")
                        End If
                    End If
                Case "tournament"
                    If TournamentManager.GetOpenedTournament() Is Nothing Then
                        Client.SendNormalMessage("Aucun tournois n'est disponible en ce moment !")
                        Exit Select
                    End If
                    If Client.Character.State.InTournament Then
                        Client.SendNormalMessage("Vous participer deja a un <b>tournoi</b> !")
                    Else
                        TournamentManager.GetOpenedTournament.Participate(Client)
                        Client.SendNormalMessage("Vous participer desormais au <b>tournois</b> !")
                    End If
                Case "description"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.SendMessage("Les informations sur le serveur :")
                        Client.SendMessage("<b>-Nom : </b>" & Config.GetItem("NAME_SERVER"))
                        Client.SendMessage("<b>-xp : </b>" & Config.GetItem("RATES_PVM"))
                        Client.SendMessage("<b>-pvp : </b>" & Config.GetItem("RATES_PVP"))
                        Client.SendMessage("<b>-vote : </b>" & Config.GetItem("VOTE"))
                        Client.SendMessage("<b>-admin : </b>" & Config.GetItem("NAME_ADMIN"))
                    End If
                Case "prism"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Dim MyPrism As New Prism
                        PrismManager.OnDeposit(Client, MyPrism)
                    End If
                Case "search"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        TrackingsManager._Search(Client)
                    End If
                Case "global"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        If Config.GetItem("GLOBALCHAT_ENABLED") = "FALSE" Then
                            Players.SendMessage("(GloBalChat) <b><i>" & Client.Character.Name & "</i></b> : " & Message)
                        End If
                        If Config.GetItem("GLOBALCHAT_ENABLED") = "TRUE" Then
                            Client.SendNormalMessage("Globalchat désactiver.")
                        End If
                    End If
                Case "aide"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        If Config.GetItem("AIDECHAT_ACTIV") = "TRUE" Then
                            Players.SendMessage("(AideChat) <b><i>" & Client.Character.Name & "</i></b> : " & Message)
                        End If
                        If Config.GetItem("AIDECHAT_ACTIV") = "False" Then
                            Client.SendNormalMessage("Aide désactiver.")
                        End If
                    End If
                Case "enclo"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.Send("ECK16")
                        Client.Send("DV")
                    End If
               
                Case "mpadmin"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        'Invic ;)

                        If Config.GetItem("MP_ADMIN") = "True" Then
                            If Client.Spam.CanRecruitment Then
                                Client.Spam.SetRecruitment()
                                Players.SendMessage("(AideChat) : Player <b> " & Client.Character.Name & "</b> cherche un admin ")
                                Client.SendNormalMessage("Votre demande est ajouter.")
                            Else
                                Client.Send("Im0115;" & Client.Spam.TimeRecruitment)
                            End If
                        End If
                        If Config.GetItem("MP_ADMIN") = "False" Then
                            Client.SendNormalMessage("La commande mpadmin est désactiver par l admin")
                        End If
                    End If
                Case "addmount"
                    Dim mount As Mount
                    Client.Character.Mounts.Add(mount)
                    SendAddMountToShed(Client, mount)
                    Client.Send(Mount.OwnerName)
                Case "infos"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.SendMessage("Podemu 0.3.0 : </b>by Invic</b>")
                        Client.SendMessage("Joueurs en ligne : <b>" & Players.GetPlayerCount & "</b>")
                        Client.SendMessage("Uptime : <b>" & Basic.GetUptime & "</b>")
                    End If
                    If Config.GetItem("MODE_LIGHT") = "True" Then
                        Client.SendMessage("PodLight 0.3.0101.1 : by Invic")
                        Client.SendMessage("Joueurs en ligne : <b>" & Players.GetPlayerCount & "</b>")
                        Client.SendMessage("Uptime : <b>" & Basic.GetUptime & "</b>")
                    End If
                Case "neutre"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        If Utils.Config.GetItem("ACTIVE_NEUTRE") Then
                            Client.Character.Player.Alignment.ResetAll()
                            Client.Character.Player.Alignment.Id = 0
                            Client.Character.SendAccountStats()
                            Client.SendNormalMessage("Vous etes désormais d'alignement <b>neutre</b>")
                        Else
                            Client.SendNormalMessage("<b>Passage neutre</b> non disponible via commande")
                        End If
                    End If
                Case "mesave"
                    Client.Character.Save()
                    Client.SendNormalMessage("Personnage <b>" & Client.Character.Name & "</b> sauvegardé !")

                Case "change"

                    If Client.Infos.Points > Utils.Config.GetItem("PTS_SEXCHANGE") Then

                        If Client.Character.Sexe = 0 Then
                            Client.Character.Sexe = 1
                        ElseIf Client.Character.Sexe = 1 Then
                            Client.Character.Sexe = 0
                        End If
                    Else
                        Client.SendNormalMessage("Pour changer votre sex , il vous faut <b>" & Utils.Config.GetItem("PTS_SEXCHANGE") & "</b> Points , et vous avez que <b>" & Client.Infos.Points & "</b> Points")
                    End If
                    Client.Character.Skin = Client.Character.Classe & Client.Character.Sexe
                    Client.Character.SkinStatic = False
                    Client.Character.GetMap.RefreshCharacter(Client)

                Case "abonnement"
                    If Client.Infos.SubscriptionDate > Now.ToString Then
                        Client.SendMessage("Votre Abonnement finira le : " & Client.Infos.SubscriptionDate.ToString)
                    Else
                        Client.SendMessage("Votre etat d'abonnement : <b>Beta test</b>")
                    End If
                    Client.SendMessage("Pour acheter une période d'abonnement , taper les commandes suivantes : ")
                    Client.SendMessage(".abos - (Abonnement 7 jours) payement : <b>" & Utils.Config.GetItem("COUT_7JOURS") & "</b> Jetons")
                    Client.SendMessage(".abom - (Abonnement 1 mois) payement : <b>" & Utils.Config.GetItem("COUT_1MOIS") & "</b> Jetons")
                    Client.SendMessage(".aboan - (Abonnement 1 an) payement : <b>" & Utils.Config.GetItem("COUT_1AN") & "</b> Jetons")
                Case "testh"

                    SyncLock Sql.AccountsSync
                        Dim subscriptionDate As String = Now.Year & "-" & Now.Month & "-" & Now.Day & " " & Now.Hour & ":" & Now.Minute & ":" & Now.Second
                        Dim SQLText As String = "UPDATE player_accounts SET subscriptionDate=" & subscriptionDate & " WHERE id=" & Client.Infos.Id
                        Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                        SQLCommand.Parameters.Add(New MySqlParameter("@subscriptionDate", subscriptionDate))

                        SQLCommand.ExecuteNonQuery()
                    End SyncLock



                Case "abos"
                    Client.Character.GetMap.RefreshCharacter(Client)


                    If Client.Infos.Points >= Utils.Config.GetItem("COUT_7JOURS") Then
                        If Client.Infos.SubscriptionDate < Now.ToString Then

                            Client.Infos.SubscriptionDate = (Now.Day + 7) & "/" & Now.Month & "/" & Now.Year & " " & Now.Hour & ":" & Now.Minute & ":" & Now.Second


                            Dim newpts = (Client.Infos.Points - (Utils.Config.GetItem("COUT_7JOURS")))

                            Client.SendNormalMessage("Vous venez d'acheter une periode d'abonnement de 7jours pour <b>" & Utils.Config.GetItem("COUT_7JOURS") & "</b> Points , il vous reste <b>" & newpts & "</b> Points")
                            Client.SendNormalMessage("Votre période d'abonnement finira le " & Client.Infos.SubscriptionDate)
                            Client.Character.Save()
                            'Pour les points
                            SyncLock (Sql.AccountsSync)
                                Dim points As String = newpts
                                Dim SQLText As String = "UPDATE player_accounts SET points=" & newpts & " WHERE id=" & Client.Infos.Id
                                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                                SQLCommand.Parameters.Add(New MySqlParameter("@points", newpts))

                                SQLCommand.ExecuteNonQuery()
                            End SyncLock
                            Client.Send("M018|" & " le jeu , pour valider votre achat" & vbCrLf & "Vous venez d'achter un lot d'abonnement , vous recevez un cadeau pour votre achat , merci de se reconecter!")
                            Client.Disconnect()

                            'Abonnement

                            SyncLock Sql.AccountsSync
                                Dim subscriptionDate As String = Client.Infos.SubscriptionDate.ToString
                                Dim SQLText As String = "UPDATE player_accounts SET subscriptionDate=" & subscriptionDate & " WHERE id=" & Client.Infos.Id
                                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                                SQLCommand.Parameters.Add(New MySqlParameter("@subscriptionDate", subscriptionDate))

                                SQLCommand.ExecuteNonQuery()
                            End SyncLock

                        Else
                            Client.Infos.SubscriptionDate = (Client.Infos.SubscriptionDate.Day + 7) & "/" & Client.Infos.SubscriptionDate.Month & "/" & Client.Infos.SubscriptionDate.Year & " " & Client.Infos.SubscriptionDate.Hour & ":" & Client.Infos.SubscriptionDate.Minute & ":" & Client.Infos.SubscriptionDate.Second

                            Dim newpts = (Client.Infos.Points - (Utils.Config.GetItem("COUT_7JOURS")))
                            '
                            Client.SendMessage(Client.Infos.SubscriptionDate)
                            Client.SendNormalMessage("Vous venez d'acheter une periode d'abonnement de 7jours pour <b>" & Utils.Config.GetItem("COUT_7JOURS") & "</b> Points , il vous reste <b>" & newpts & "</b> Points")
                            Client.SendNormalMessage("Votre période d'abonnement finira le " & Client.Infos.SubscriptionDate)
                            Client.Character.Save()

                            'Pour les points
                            SyncLock (Sql.AccountsSync)
                                Dim points As String = newpts
                                Dim SQLText As String = "UPDATE player_accounts SET points=" & newpts & " WHERE id=" & Client.Infos.Id
                                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                                SQLCommand.Parameters.Add(New MySqlParameter("@points", newpts))

                                SQLCommand.ExecuteNonQuery()
                            End SyncLock
                            Client.Send("M018|" & " le jeu , pour valider votre achat" & vbCrLf & "Vous venez d'achter un lot d'abonnement , vous recevez un cadeau pour votre achat , merci de se reconecter!")
                            Client.Disconnect()
                            'Abonnement

                            SyncLock Sql.AccountsSync
                                Dim subscriptionDate As String = Client.Infos.SubscriptionDate.ToString
                                Dim SQLText As String = "UPDATE player_accounts SET subscriptionDate=" & subscriptionDate & " WHERE id=" & Client.Infos.Id
                                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                                SQLCommand.Parameters.Add(New MySqlParameter("@subscriptionDate", subscriptionDate))

                                SQLCommand.ExecuteNonQuery()
                            End SyncLock
                             
                        End If


                    Else
                        Client.SendNormalMessage("Pour acheter l'abonnement de une semaine , il vous faut <b>" & Utils.Config.GetItem("COUT_7JOURS") & "</b> points , et vous n'avez que <b>" & Client.Infos.Points & "</b> points")
                    End If
                Case "fullpdv"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        If Client.Character.State.InBattle Then
                            Client.SendNormalMessage("Action impossible en combat !")
                            Exit Select
                        End If
                        Client.SendNormalMessage("Votre nombre de points de vie a ete remis au maximum !")
                        Client.Character.Player.Life = Client.Character.Player.MaximumLife
                        Client.Character.SendAccountStats()
                    End If
                Case "guilde"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        If Client.Character.State.InBattle Then
                            Client.SendNormalMessage("Action impossible en combat !")
                            Exit Select
                        End If
                        Client.Send("gn")
                    End If

                Case "traque"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        If Utils.Config.GetItem("ACTIVE_CMD_TRAQUE") Then
                            TrackingsManager._Get(Client)
                        Else
                            Client.SendNormalMessage("<b>.traque</b> désactiver sur demande de l'admin")
                        End If
                    End If

                Case "banque"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.Character.State.IsInBank = True
                        Dim Prix As Integer = BankParser.ListOfItems.Count
                        If Client.Character.Player.Kamas >= Prix Then
                            Client.Character.Player.Kamas -= Prix
                            Client.Character.SendAccountStats()
                            Client.SendNormalMessage("Vous venez de perdre " & Prix & " kamas pour accéder à votre coffre")
                            BankParser.BeginTrade(Client)
                        Else
                            Client.SendNormalMessage("Vous n'avez pas assez de kamas pour accéder à votre coffre !")
                        End If
                    End If
                Case "bontarien"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.SendNormalMessage("Vous venez de passer à l'alignement Bontarien")
                        Client.Character.Player.Alignment.ResetAll()
                        Client.Character.Player.Alignment.Id = 1
                        Client.Character.SendAccountStats()
                    End If
                Case "brakmarien"
                    If Config.GetItem("MODE_LIGHT") = "False" Then
                        Client.SendNormalMessage("Vous venez de passer à l'alignement brackmarien")
                        Client.Character.Player.Alignment.ResetAll()
                        Client.Character.Player.Alignment.Id = 2
                        Client.Character.SendAccountStats()
                    End If

            End Select

            Return True

        End Function


    End Class
End Namespace