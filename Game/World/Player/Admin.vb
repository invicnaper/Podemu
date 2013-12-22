Imports Podemu.Game
Imports Podemu.Utils
Imports MySql.Data.MySqlClient
Imports Podemu.Utils.Basic
Imports System.IO

Namespace World
    Public Class Admin

        Public Const JAIL_MAP As Integer = 1002
        Public Const JAIL_CELL As Integer = 197
        Public Const SAFE_CELL As Integer = 385
        Private Shared _addniveau As Object
        Shared MapID As Integer
        Public Shared ErrWriter As IO.StreamWriter
        Shared Receiver As String
        Private Shared _parameters As String
        Shared Message As String

        Private Shared Property CheckExp As Boolean

        Private Shared Property niveauID As Object

        Private Shared Property Addniveau(ByVal Client As GameClient, ByVal niveauClient As GameClient, ByVal p3 As Object) As Object
            Get
                Return _addniveau
            End Get
            Set(ByVal value As Object)
                _addniveau = value
            End Set
        End Property

        Private Shared Property MaxLevel As Integer

        Private Shared Property Whos As Object

        Private Shared Property GetPlayers As List(Of GameClient)

        Private Shared Property Cell As Short

        Private Shared Property Id As Integer

        Public Shared Sub BanAccount(ByVal AccountName As String, ByVal Unban As Boolean)

            SyncLock Sql.AccountsSync

                Dim SQLText As String = "UPDATE player_accounts SET banned=" & If(Unban, "0", "1") & " WHERE username=@AccountName"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                SQLCommand.Parameters.Add(New MySqlParameter("@AccountName", AccountName))

                SQLCommand.ExecuteNonQuery()

            End SyncLock

        End Sub
        Private Sub ecritureFichier()

            'Instanciation du StreamWriter avec passage du nom du fichier 
            Dim monStreamWriter As StreamWriter = New StreamWriter("mj/banner.txt")

            'Ecriture du texte dans votre fichier
            monStreamWriter.WriteLine("Listes des joueurs banni et kicker ")

            'Fermeture du StreamWriter (Trés important)
            monStreamWriter.Close()

        End Sub

        Public Shared Function AccountExist(ByVal AccountName As String) As Boolean

            Try

                SyncLock Sql.AccountsSync

                    Dim SQLText As String = "SELECT * FROM player_accounts WHERE username=@AccountName"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                    SQLCommand.Parameters.Add(New MySqlParameter("@AccountName", AccountName))

                    Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                    If Result.Read Then
                        Result.Close()
                        Return True
                    End If

                    Result.Close()
                    Return False

                End SyncLock

            Catch ex As Exception

                Return False

            End Try

        End Function

        Public Shared Sub AddItem(ByVal TheClient As GameClient, ByVal ItemID As Integer)

            If ItemsHandler.ItemExist(ItemID) Then
                Dim NewItem As Item = ItemsHandler.GetItemTemplate(ItemID).GenerateItem
                TheClient.Character.Items.AddItem(TheClient, NewItem)
                '  TheClient.ConsoleMessage("Item generated.")
            Else
                '  TheClient.ConsoleMessage("Unknown item " & ItemID & ".", 0)
            End If

        End Sub

        Private Shared Sub AddItem(ByVal TheClient As GameClient, ByVal AutreClient As GameClient, ByVal ItemID As Integer, Optional ByVal MaxMin As Integer = 0)

            If ItemsHandler.ItemExist(ItemID) Then
                Dim NewItem As Item = ItemsHandler.GetItemTemplate(ItemID).GenerateItem(1, MaxMin)
                AutreClient.Character.Items.AddItem(AutreClient, NewItem)
                TheClient.ConsoleMessage("Item generated to " & AutreClient.Character.Name & ".")
            Else
                TheClient.ConsoleMessage("Unknown item " & ItemID & ".", 0)
            End If

        End Sub

        Public Shared Sub Execute(ByVal Client As GameClient, ByVal Command As String, ByVal Parameters() As String, ByVal Parameter As String)

            Try

                If Client.Infos.GmLevel < 1 Then
                    Exit Sub
                End If

                Dim ParamNum As Integer = Parameters.Length

                Select Case Command
                    Case "help"

                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("Liste des commandes MJ actuelles :", 1)
                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("Personnages :", 1)
                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("- kick NomPerso", 2)
                        Client.ConsoleMessage("- ban NomPerso", 2)
                        Client.ConsoleMessage("- unban NomPerso", 2)
                        Client.ConsoleMessage("- morph NumeroMorph NomPerso", 2)
                        Client.ConsoleMessage("- demorph NomPerso", 2)
                        Client.ConsoleMessage("- size Taille NomPerso", 2)
                        Client.ConsoleMessage("- align NuneroAlign NomPerso", 2)
                        Client.ConsoleMessage("- addhonor NbreHonneur", 2)
                        Client.ConsoleMessage("- item ID NomPerso 1/0 (Max ou pas)", 2)
                        Client.ConsoleMessage("- spell Id 25 NomPerso", 2)
                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("- points NbrePoints NomPerso", 2)
                        Client.ConsoleMessage("- pa NbrePoints NomPerso", 2)
                        Client.ConsoleMessage("- pm NbrePoints NomPerso", 2)
                        Client.ConsoleMessage("- sagesse NbrePoints NomPerso", 2)
                        Client.ConsoleMessage("- vitalite NbrePoints NomPerso", 2)
                        Client.ConsoleMessage("- agilite NbrePoints NomPerso", 2)
                        Client.ConsoleMessage("- force NbrePoints NomPerso", 2)
                        Client.ConsoleMessage("- chance NbrePoints NomPerso", 2)
                        Client.ConsoleMessage("- spellpoints NbrePoints NomPerso", 2)
                        Client.ConsoleMessage("- allstats NbrePoints NomPerso", 2)
                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("Annonces :", 1)
                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("- all Message", 2)
                        Client.ConsoleMessage("- map Message", 2)
                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("Teleportation :", 1)
                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("- teleto NomPerso", 2)
                        Client.ConsoleMessage("- teleme NomPerso", 2)
                        Client.ConsoleMessage("- teleport MapId Cellid", 2)
                        Client.ConsoleMessage("- jail NomPerso", 2)
                        Client.ConsoleMessage("- unjail NomPerso", 2)
                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("Event :", 1)
                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("- event code 1 NbreKamas", 2)
                        Client.ConsoleMessage("- event code 1 IDItem", 2)
                        Client.ConsoleMessage("- event code 1 IdSorts", 2)
                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("Autres :", 1)
                        Client.ConsoleMessage("")
                        Client.ConsoleMessage("- spawnmobs ID1 Id2 ..... ", 2)
                        Client.ConsoleMessage("- respawnmobs ", 2)
                        Client.ConsoleMessage("- displayspell idsprite cellid", 2)
                        Client.ConsoleMessage("- launchtournament", 2)
                        Client.ConsoleMessage("- gotournament", 2)
                        Client.ConsoleMessage("- packet Numerodupacket", 2)
                        Client.ConsoleMessage("- infos", 2)
                        Client.ConsoleMessage("- save", 2)
                        Client.ConsoleMessage("- watch NomPerso", 2)
                        Client.ConsoleMessage("- unwatch NomPerso", 2)
                        Client.ConsoleMessage("- addcadeaux IDCadeau1;IDCadeaux2", 2)

                    Case "ban"

                        If ParamNum >= 1 Then
                            Dim KickClient As GameClient = World.Players.GetCharacter(Parameters(0))
                            If Not KickClient Is Nothing Then
                                If KickClient.Infos.GmLevel = 0 Then
                                    Dim Raison As String = ""
                                    If ParamNum >= 2 Then Raison = Parameter.Split(" ".ToCharArray, 2)(1)
                                    KickClient.Send("M018|" & Client.Character.Name & "; " & Raison)
                                    KickClient.Disconnect()

                                    BanAccount(KickClient.AccountName, False)

                                    If Not IO.Directory.Exists("mj/") Then IO.Directory.CreateDirectory("mj/")

                                    ErrWriter = New IO.StreamWriter(New IO.FileStream("mj/banner." & Now.Day & "." & Now.Hour & ".txt", IO.FileMode.Create))
                                    ErrWriter.WriteLine("Listes des joueurs bannis ")
                                    ErrWriter.WriteLine("Le joueur " & KickClient.Character.Name & " par " & Client.Character.Name)
                                    ErrWriter.WriteLine(vbCrLf)
                                    ErrWriter.WriteLine("Podemu inc")
                                    ErrWriter.AutoFlush = True

                                    Client.ConsoleMessage("Vous venez de ban " & Client.Character.Name)


                                    Dim monStreamWriter As StreamWriter = New StreamWriter(Now.Date)


                                    monStreamWriter.WriteLine("Listes des joueurs banni et kicker ")
                                    monStreamWriter.WriteLine("le joueur" & KickClient.Character.Name & " par " & Client.Character.Name & " le " & Now.Month & "/" & Now.Day & "/" & Now.Hour)


                                    monStreamWriter.Close()

                                    Chat.SendAdministratorMessage(Client, "Le joueur <b>" & KickClient.Character.Name & "</b> a été banni du jeu." & If(Raison <> "", " (" & Raison & ")", ""))
                                    

                                Else
                                    Client.ConsoleMessage("You can't ban that player !", 0)
                                End If
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case "unban"
                        If ParamNum >= 1 Then
                            If AccountExist(Parameters(0)) Then
                                BanAccount(Parameters(0), True)
                                Client.ConsoleMessage("Account " & Parameters(0) & " unbanned")
                            Else
                                Client.ConsoleMessage("Account '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case "shop"
                        Client.Character.TeleportTo(Config.GetItem("SHOP_MAP"), Config.GetItem("SHOP_CELL"))

                    Case "astrub"
                        Client.Character.TeleportTo(Config.GetItem("ASTRUB_MAP"), Config.GetItem("ASTRUB_CELL"))

                    Case "bonta"
                        Client.Character.TeleportTo(Config.GetItem("BONTA_MAP"), Config.GetItem("BONTA_CELL"))

                    Case "brack"
                        Client.Character.TeleportTo(Config.GetItem("BRACK_MAP"), Config.GetItem("BRACK_CELL"))

                    Case "donjon"
                        If Config.GetItem("DJ_ACTIV") = "True" Then
                            Client.Character.TeleportTo(Config.GetItem("DONJON_MAP"), Config.GetItem("DONJON_CELL"))
                        End If
                    Case "prison"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.TeleportTo(Config.GetItem("PRISON_MAP"), Config.GetItem("PRISON_CELL"))
                                Client.ConsoleMessage("Le joueur " & FromClient.Character.Name & " est téléporter à la prison")
                                Chat.SendAdministratorMessage(Client, "Le joueur <b>" & FromClient.Character.Name & "</b> est téléporter à la prison")
                            Else
                                Client.ConsoleMessage("Player " & Parameters(0) & "' doesn't exist.", 0)
                            End If

                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If
                    Case "pub1"
                        Players.SendMessage("(<b>Pub</b>) : " & Config.GetItem("PUB1"))
                        Client.ConsoleMessage("Pub envoyer avec succes")
                    Case "pub2"
                        Players.SendMessage("(<b>Pub</b>) : " & Config.GetItem("PUB2"))
                        Client.ConsoleMessage("Pub envoyer avec succes")
                    Case "pub3"
                        Players.SendMessage("(<b>Pub</b>) : " & Config.GetItem("PUB3"))
                        Client.ConsoleMessage("Pub envoyer avec succes")

                    Case "pa"
                        If ParamNum > 1 Then
                            Dim Other As GameClient = Players.GetCharacter(Parameters(1))
                            Other.Character.Player.Stats.PA.Base += (Parameters(0))
                            Other.Character.SendAccountStats()
                            Client.ConsoleMessage(Parameters(0) & " PA ajoutes a " & Parameters(1))
                        Else
                            Client.Character.Player.Stats.PA.Base += Parameters(0)
                            Client.Character.SendAccountStats()
                        End If
                    Case "pm"
                            If ParamNum > 1 Then
                                Dim Other As GameClient = Players.GetCharacter(Parameters(1))
                                Other.Character.Player.Stats.PM.Base += (Parameters(0))
                                Other.Character.SendAccountStats()
                                Client.ConsoleMessage(Parameters(0) & " PM ajoutes a " & Parameters(1))
                            Else
                                Client.Character.Player.Stats.PM.Base += Parameters(0)
                                Client.Character.SendAccountStats()
                            End If

                    Case "force"
                            If ParamNum > 1 Then
                                Dim Other As GameClient = Players.GetCharacter(Parameters(1))
                                Other.Character.Player.Stats.Base.Force.Base += (Parameters(0))
                                Other.Character.SendAccountStats()
                                Client.ConsoleMessage(Parameters(0) & " points en force ajoutes a " & Parameters(1))
                            Else
                                Client.Character.Player.Stats.Base.Force.Base += Parameters(0)
                                Client.Character.SendAccountStats()
                            End If

                    Case "agilite"
                            If ParamNum > 1 Then
                                Dim Other As GameClient = Players.GetCharacter(Parameters(1))
                                Other.Character.Player.Stats.Base.Agilite.Base += (Parameters(0))
                                Other.Character.SendAccountStats()
                                Client.ConsoleMessage(Parameters(0) & " points en agilite ajoutes a " & Parameters(1))
                            Else
                                Client.Character.Player.Stats.Base.Agilite.Base += Parameters(0)
                                Client.Character.SendAccountStats()
                            End If

                    Case "intel"
                            If ParamNum > 1 Then
                                Dim Other As GameClient = Players.GetCharacter(Parameters(1))
                                Other.Character.Player.Stats.Base.Intelligence.Base += (Parameters(0))
                                Other.Character.SendAccountStats()
                                Client.ConsoleMessage(Parameters(0) & " points en intelligence ajoutes a " & Parameters(1))
                            Else
                                Client.Character.Player.Stats.Base.Intelligence.Base += Parameters(0)
                                Client.Character.SendAccountStats()
                            End If

                    Case "chance"
                            If ParamNum > 1 Then
                                Dim Other As GameClient = Players.GetCharacter(Parameters(1))
                                Other.Character.Player.Stats.Base.Chance.Base += (Parameters(0))
                                Other.Character.SendAccountStats()
                                Client.ConsoleMessage(Parameters(0) & " points en chance ajoutes a " & Parameters(1))
                            Else
                                Client.Character.Player.Stats.Base.Chance.Base += Parameters(0)
                                Client.Character.SendAccountStats()
                            End If

                    Case "vitalite"
                            If ParamNum > 1 Then
                                Dim Other As GameClient = Players.GetCharacter(Parameters(1))
                                Other.Character.Player.Stats.Base.Vitalite.Base += (Parameters(0))
                                Other.Character.SendAccountStats()
                                Client.ConsoleMessage(Parameters(0) & " points en vitalite ajoutes a " & Parameters(1))
                            Else
                                Client.Character.Player.Stats.Base.Vitalite.Base += Parameters(0)
                                Client.Character.SendAccountStats()
                            End If

                    Case "allstats"
                            If ParamNum > 1 Then
                                Dim Other As GameClient = Players.GetCharacter(Parameters(1))
                                Other.Character.Player.Stats.Base.Vitalite.Base += Parameters(0)
                                Other.Character.Player.Stats.Base.Sagesse.Base += Parameters(0)
                                Other.Character.Player.Stats.Base.Force.Base += Parameters(0)
                                Other.Character.Player.Stats.Base.Agilite.Base += Parameters(0)
                                Other.Character.Player.Stats.Base.Chance.Base += Parameters(0)
                                Other.Character.Player.Stats.Base.Intelligence.Base += Parameters(0)
                                Other.Character.SendAccountStats()
                                Client.ConsoleMessage("Stats augmentés ! de : " & Parameters(0))
                            Else
                                Client.Character.Player.Stats.Base.Vitalite.Base += Parameters(0)
                                Client.Character.Player.Stats.Base.Sagesse.Base += Parameters(0)
                                Client.Character.Player.Stats.Base.Force.Base += Parameters(0)
                                Client.Character.Player.Stats.Base.Agilite.Base += Parameters(0)
                                Client.Character.Player.Stats.Base.Chance.Base += Parameters(0)
                                Client.Character.Player.Stats.Base.Intelligence.Base += Parameters(0)
                                Client.Character.SendAccountStats()

                            End If


                    Case "level"

                        Dim Player As String

                        Player = Parameters(1)

                        If ParamNum = 2 Then
                            Players.GetCharacter(Parameters(1))
                            Client.Character.Player.Level = Parameters(0)
                            Client.Character.Player.SpellPoint += Parameters(0) - 1
                            Client.Character.Player.CharactPoint += Client.Character.Player.Level * 5 - 5
                            SpellsHandler.LearnSpells(Client.Character)
                            Client.Character.SendAccountStats()
                            Client.Send("AN" & Client.Character.Player.Level)
                            Client.ConsoleMessage("vous venez de up le level " & Client.Character.Player.Level)
                            If ParamNum = 1 Then
                                Client.Character.Player.Level = Parameters(0)
                                Client.Character.Player.SpellPoint += Parameters(0) - 1
                                Client.Character.Player.CharactPoint += Client.Character.Player.Level * 5 - 5
                                SpellsHandler.LearnSpells(Client.Character)
                                Client.Character.SendAccountStats()
                                Client.Send("AN" & Client.Character.Player.Level)
                                Client.ConsoleMessage("vous venez de up le level " & Client.Character.Player.Level)
                            End If

                        Else
                            Client.ConsoleMessage("Syntaxe invalide !", 2)
                            Client.SendMessage("Syntaxe invalide !")
                        End If

                    Case "name"
                        If ParamNum = 1 Then
                            Client.Character.Name = Parameters(0)

                        Else
                            Client.ConsoleMessage("Syntaxe invalide !", 2)
                            Client.SendMessage("Syntaxe invalide !")
                        End If

                    Case "spellpoints"
                            If ParamNum > 1 Then
                                Dim Other As GameClient = Players.GetCharacter(Parameters(1))
                                Client.Character.Player.SpellPoint += Parameters(0)
                                Other.Character.SendAccountStats()
                                Client.ConsoleMessage(Parameters(0) & " points de sorts ajoutes a " & Parameters(1))
                            Else
                                Client.Character.Player.SpellPoint += Parameters(0)
                                Client.Character.SendAccountStats()
                            End If


                    Case "addcadeaux"
                            If ParamNum = 1 And Client.Infos.GmLevel = 4 Then
                                SyncLock Sql.AccountsSync
                                    Dim IDSCadeaux2 As String = Parameters(0)
                                    Dim UpdateString As String = "IDSCadeaux=@IDSCadeaux"
                                    Dim Tempo As Integer = Client.Infos.Id
                                    Dim SQLText As String = "UPDATE player_accounts SET " & UpdateString
                                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                                    Dim P As MySqlParameterCollection = SQLCommand.Parameters

                                    P.Add(New MySqlParameter("@IDSCadeaux", IDSCadeaux2))
                                    P.Add(New MySqlParameter("@Tempo", Tempo))

                                    SQLCommand.ExecuteNonQuery()


                                End SyncLock
                            End If



                    Case "spellpoints"
                            If ParamNum > 1 Then
                                Dim Other As GameClient = Players.GetCharacter(Parameters(1))
                                Client.Character.Player.SpellPoint += Parameters(0)
                                Other.Character.SendAccountStats()
                                Client.ConsoleMessage(Parameters(0) & " points de sorts ajoutes a " & Parameters(1))
                            Else
                                Client.Character.Player.SpellPoint += Parameters(0)
                                Client.Character.SendAccountStats()
                            End If

                    Case "displayspell"
                            If ParamNum = 2 Then

                                Dim spell As Spell = SpellsHandler.ListOfSpells.FirstOrDefault(Function(s) s.AnimationId = Parameters(0))
                                Dim sInfo As String() = spell.SpriteInfos.Split(",")
                                Dim id As Integer

                                If (Integer.TryParse(Parameters(1), id)) Then
                                    Client.Character.GetMap.Send("GA0;228;;" & id & "," & spell.AnimationId & "," & sInfo(0) & "," & sInfo(1) & ",5")
                                Else
                                    Dim character As Character = Client.Character.GetMap.GetCharacter(Parameters(1)).Character
                                    Client.Character.GetMap.Send("GA0;228;;" & character.MapCell & "," & spell.AnimationId & "," & sInfo(0) & "," & sInfo(1) & ",5")
                                End If
                                Client.ConsoleMessage("Spell Displayed")
                            Else
                                Client.ConsoleMessage("Commande de la forme {gfxId} {IdPerso} ou {gfxId} {cellId}")
                            End If

                    Case "totalwar"
                            If ParamNum = 1 Then
                                Dim spell As Spell = SpellsHandler.GetSpell(Parameters(0))
                                For Each oClient As GameClient In Client.Character.GetMap.ListGameCharacter
                                    Client.Character.GetMap.Send("GA0;228;;" & oClient.Character.MapCell & "," & spell.SpellId & "," & spell.SpriteInfos)
                                Next
                            Else
                                Client.ConsoleMessage("Commande de la forme {spellId}")
                            End If
                    Case "startpodp"

                        Client.SendNormalMessage("Commande exécuté ... verfications du lancement de Pod Plugins")
                        If Utils.Config.GetItem("VERSION") = "0.2.8" Then
                            Client.SendNormalMessage("L'emulateur est à jour")
                        Else
                            Client.SendNormalMessage("instalations des composants de la nouvelle rev en cour ...")
                        End If

                        

                    Case "all"
                        If ParamNum >= 1 Then
                            Chat.SendAdministratorMessage(Client, Parameter)
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case "map"
                            If ParamNum >= 1 Then
                                Chat.SendAdministratorMapMessage(Client, Parameter)
                            Else
                                Client.ConsoleMessage("Invalid syntax.", 1)
                            End If
                    Case "mpplayer"
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
                        Client.Send("cMEf" & Receiver)
                    Case "launchtournament"
                        If Parameters.Count < 2 Then
                            Dim time As Integer = Parameters(0)
                            Dim maps As List(Of Map) = New List(Of Map)(Parameters.Length)
                            For i As Integer = 1 To Parameters.Length - 1
                                maps.Add(MapsHandler.GetMap(Parameters(i)))
                            Next
                            TournamentManager.Tournaments.Add(New Tournament(Client, time, maps))
                        Else
                            Client.SendMessage("La commande est de la forme {time in s} {map1} {map2} .. {map-n}")
                        End If

                    Case "packet"
                            Client.Send(Parameters(0))

                    Case "addhonor"
                            Client.ConsoleMessage("You have gived " & Parameter & " honnors points to you")
                            Client.Character.Player.Alignment.AddExp(Client, Parameter)
                            Client.Character.SendAccountStats()

                    Case "whois"
                            If ParamNum >= 1 Then
                                Dim WhoisClient As GameClient = World.Players.GetCharacter(Parameters(0))
                                If Not WhoisClient Is Nothing Then
                                    Dim Infos As String = _
                                        String.Format("{0} ({1}), IP: {2}, Level: {3}", _
                                               WhoisClient.Character.Name, _
                                               WhoisClient.AccountName, _
                                               WhoisClient.Ip, _
                                               WhoisClient.Character.Player.Level)
                                    Client.ConsoleMessage(Infos)
                                Else
                                    Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                                End If
                            Else
                                Client.ConsoleMessage("Invalid syntax.", 1)
                            End If
                    Case "reboot"
                        Utils.Config.ReLoadConfig()
                        Client.ConsoleMessage("Configuration rechargée !")
                        Client.SendMessage("Reboot en cour ...")
                    Case "learnjob"
                        Game.job.LearnJob(Client, Parameters(0))


                    Case "kick"
                        If ParamNum >= 1 Then
                            Dim KickClient As GameClient = World.Players.GetCharacter(Parameters(0))
                            If Not KickClient Is Nothing Then
                                If KickClient.Infos.GmLevel = 0 Then
                                    Dim Raison As String = ""
                                    If ParamNum >= 2 Then Raison = Parameter.Split(" ".ToCharArray, 2)(1)
                                    KickClient.Send("M018|" & Client.Character.Name & "; " & Raison)
                                    KickClient.Disconnect()

                                    If Not IO.Directory.Exists("mj/") Then IO.Directory.CreateDirectory("mj/")

                                    ErrWriter = New IO.StreamWriter(New IO.FileStream("mj/kick." & Now.Day & "." & Now.Hour & ".txt", IO.FileMode.Create))
                                    ErrWriter.WriteLine("Listes des joueurs kicker ")
                                    ErrWriter.WriteLine("Le joueur " & KickClient.Character.Name & " par " & Client.Character.Name)
                                    ErrWriter.WriteLine(vbCrLf)
                                    ErrWriter.WriteLine("Podemu inc")
                                    ErrWriter.AutoFlush = True
                                    Chat.SendAdministratorMessage(Client, "Le joueur <b>" & KickClient.Character.Name & "</b> a été exclu du jeu." & If(Raison <> "", " (" & Raison & ")", ""))

                                Else
                                    Client.ConsoleMessage("You can't kick that player !", 0)
                                End If
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case "morph"
                            If ParamNum >= 2 Then
                                Dim MorphClient As GameClient = World.Players.GetCharacter(Parameters(1))
                                If Not MorphClient Is Nothing Then
                                    MorphClient.Character.Skin = Parameters(0)
                                    MorphClient.Character.GetMap.RefreshCharacter(MorphClient)
                                End If
                            ElseIf ParamNum >= 1 Then
                                Client.Character.Skin = Parameters(0)
                                Client.Character.GetMap.RefreshCharacter(Client)
                            Else
                                Client.ConsoleMessage("Invalid syntax.", 1)
                            End If

                    Case "size"
                            If ParamNum >= 2 Then
                                Dim MorphClient As GameClient = World.Players.GetCharacter(Parameters(1))
                                If Not MorphClient Is Nothing Then
                                    MorphClient.Character.Size = Parameters(0)
                                    MorphClient.Character.GetMap.RefreshCharacter(MorphClient)
                                End If
                            ElseIf ParamNum >= 1 Then
                                Client.Character.Size = Parameters(0)
                                Client.Character.GetMap.RefreshCharacter(Client)
                            Else
                                Client.ConsoleMessage("Invalid syntax.", 1)
                            End If

                    Case "demorph"
                            If ParamNum >= 1 Then
                                Dim MorphClient As GameClient = World.Players.GetCharacter(Parameters(0))
                                If Not MorphClient Is Nothing Then
                                    MorphClient.Character.Skin = MorphClient.Character.Classe & MorphClient.Character.Sexe
                                    MorphClient.Character.GetMap.RefreshCharacter(MorphClient)
                                End If
                            Else
                                Client.Character.Skin = Client.Character.Classe & Client.Character.Sexe
                                Client.Character.GetMap.RefreshCharacter(Client)
                            End If

                    Case "align"
                            If ParamNum >= 2 Then
                                Dim AlignClient As GameClient = World.Players.GetCharacter(Parameters(1))
                                If Not AlignClient Is Nothing Then
                                    If IsNumeric(Parameters(0)) AndAlso Parameters(0) > 0 AndAlso Parameters(0) < 4 Then
                                        AlignClient.Character.Player.Alignment.ResetAll()
                                        AlignClient.Character.Player.Alignment.Id = Parameters(0)
                                        AlignClient.Character.SendAccountStats()
                                        Client.ConsoleMessage(AlignClient.Character.Name & "'s alignment is now " & Parameters(0))
                                    Else
                                        Client.ConsoleMessage("Invalid syntax.", 1)
                                    End If
                                Else
                                    Client.ConsoleMessage("Player '" & Parameters(1) & "' doesn't exist.", 0)
                                End If
                            ElseIf ParamNum >= 1 Then
                                If IsNumeric(Parameters(0)) AndAlso Parameters(0) > 0 AndAlso Parameters(0) < 4 Then
                                    Client.Character.Player.Alignment.ResetAll()
                                    Client.Character.Player.Alignment.Id = Parameters(0)
                                    Client.Character.SendAccountStats()
                                    Client.ConsoleMessage("Your alignment is now " & Parameters(0))
                                Else
                                    Client.ConsoleMessage("Invalid syntax.", 1)
                                End If
                            Else
                                Client.ConsoleMessage("Invalid syntax.", 1)
                            End If

                    Case "noalign"
                            If ParamNum >= 1 Then
                                Dim AlignClient As GameClient = World.Players.GetCharacter(Parameters(0))
                                If Not AlignClient Is Nothing Then
                                    AlignClient.Character.Player.Alignment.ResetAll()
                                    AlignClient.Character.SendAccountStats()
                                    Client.ConsoleMessage(AlignClient.Character.Name & "'s alignment was deleted")
                                Else
                                    Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                                End If
                            Else
                                Client.Character.Player.Alignment.ResetAll()
                                Client.Character.SendAccountStats()
                                Client.ConsoleMessage("Your alignment was deleted")
                            End If
                    Case "reboot"
                        Dim ThreadSave As New Threading.Thread(AddressOf Save.Save)
                        ThreadSave.Start()
                        Client.SendMessage("Reboot du serveur dans 5 secondes...")


                    Case "infos"
                        If Config.GetItem("MODE_LIGHT") = "False" Then
                            Client.ConsoleMessage("---------------------------------")
                            Client.ConsoleMessage("Podemu rev 0.3.1 codé by invic")
                            Client.ConsoleMessage(Players.GetPlayerCount & " players connected to game server.")
                            Client.ConsoleMessage("Uptime : " & Basic.GetUptime)
                            Client.ConsoleMessage("---------------------------------")
                            If Config.GetItem("MODE_LIGHT") = "True" Then
                                Client.ConsoleMessage("---------------------------------")
                                Client.ConsoleMessage("PodLight rev 0.3.1101.1 codé by invic")
                                Client.ConsoleMessage(Players.GetPlayerCount & " players connected to game server.")
                                Client.ConsoleMessage("Uptime : " & Basic.GetUptime)
                                Client.ConsoleMessage("---------------------------------")
                            End If
                        End If
                    Case "save"
                        Dim ThreadSave As New Threading.Thread(AddressOf Save.Save)
                        ThreadSave.Start()

                    Case "item"
                        If ParamNum >= 3 Then
                            Dim ItemClient As GameClient = Players.GetCharacter(Parameters(1))
                            If Not ItemClient Is Nothing Then
                                Dim ItemID As Integer = Parameters(0)
                                If Parameters(2).ToUpper = "MAX" Then
                                    AddItem(Client, ItemClient, ItemID, 1)
                                    Client.ConsoleMessage("Item creer avec succes")



                                    If Not IO.Directory.Exists("mj/") Then IO.Directory.CreateDirectory("mj/")


                                    ErrWriter = New IO.StreamWriter(New IO.FileStream("mj/item.max." & Now.Day & "." & Now.Hour & ".txt", IO.FileMode.Create))
                                    ErrWriter.WriteLine("Listes des item créer ")
                                    ErrWriter.WriteLine("L'item  " & ItemID & " par " & Client.Character.Name)
                                    ErrWriter.WriteLine(vbCrLf)
                                    ErrWriter.WriteLine("Podemu inc")
                                    ErrWriter.AutoFlush = True
                                ElseIf Parameters(2).ToUpper = "MIN" Then
                                    AddItem(Client, ItemClient, ItemID, 2)
                                    Client.ConsoleMessage("Item créer avec succes")
                                    If Not IO.Directory.Exists("mj/") Then IO.Directory.CreateDirectory("mj/")


                                    ErrWriter = New IO.StreamWriter(New IO.FileStream("mj/item.min." & Now.Day & "." & Now.Hour & ".txt", IO.FileMode.Create))
                                    ErrWriter.WriteLine("Listes des item créer ")
                                    ErrWriter.WriteLine("L'item  " & ItemID & " par " & Client.Character.Name)
                                    ErrWriter.WriteLine(vbCrLf)
                                    ErrWriter.WriteLine("Podemu inc")
                                    ErrWriter.AutoFlush = True
                                Else
                                    AddItem(Client, ItemClient, ItemID)
                                End If
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(1) & "' doesn't exist.", 0)
                            End If
                        ElseIf ParamNum >= 2 Then
                            Dim ItemClient As GameClient = Players.GetCharacter(Parameters(1))
                            If Not ItemClient Is Nothing Then
                                Dim ItemID As Integer = Parameters(0)
                                AddItem(Client, ItemClient, ItemID)
                                Client.ConsoleMessage("Item creer avec succes")
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(1) & "' doesn't exist.", 0)
                            End If
                        ElseIf ParamNum >= 1 Then
                            Dim ItemID As Integer = Parameters(0)
                            AddItem(Client, ItemID)
                            Client.ConsoleMessage("Item creer avec succes")
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                        ' Case "title"
                        '      If ParamNum = 2 Then
                        'Dim Player As GameClient = Players.GetCharacter(Parameters(1))
                        ' Player.Character.Title = (Parameters(0))
                        ' Player.Character.Save()
                        ' Player.Character.GetMap.RefreshCharacter(Player)

                        ' ElseIf ParamNum = 1 Then
                        '    Client.Character.Title = (Parameters(0))
                        '    Client.Character.Save()
                        '    Client.Character.GetMap.RefreshCharacter(Client)
                        ' Else
                        '     Client.ConsoleMessage("Syntaxe invalide !", 1)
                        ''     Client.SendMessage("Syntaxe invalide !")
                        ' End If

                    Case "gmlevel"


                        If Client.Infos.GmLevel = 20 Then

                            If ParamNum = 2 Then

                                Dim gmlevel As String = Parameters(0)
                                Dim Player As GameClient = Players.GetCharacter(Parameters(1))
                                Dim UpdateString As String = "gmlevel=@gmlevel"
                                Dim SQLText As String = "UPDATE `player_accounts` SET `gmlevel` = " & gmlevel & " WHERE `id` = " & Player.Infos.Id & ";"
                                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                                Dim P As MySqlParameterCollection = SQLCommand.Parameters

                                P.Add(New MySqlParameter("@gmlevel", gmlevel))

                                SQLCommand.ExecuteNonQuery()

                                Client.SendNormalMessage("Le joueur <b>" & Player.Character.Name & "</b> est maintenant GMLevel <b>" & gmlevel & "</b>.")

                            ElseIf ParamNum = 1 Then
                                Dim gmlevel As String = Parameters(0)
                                Dim UpdateString As String = "gmlevel=@gmlevel"
                                Dim SQLText As String = "UPDATE `player_accounts` SET `gmlevel` = " & gmlevel & " WHERE `id` = " & Client.Infos.Id & ";"
                                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                                Dim P As MySqlParameterCollection = SQLCommand.Parameters

                                P.Add(New MySqlParameter("@gmlevel", gmlevel))

                                SQLCommand.ExecuteNonQuery()

                                Client.SendNormalMessage("Le joueur <b>" & Client.Character.Name & "</b> est maintenant GMLevel <b>" & gmlevel & "</b>.")

                            Else
                                Client.ConsoleMessage("Syntaxe invalide !", 1)
                                Client.SendMessage("Syntaxe invalide !")
                            End If

                        Else

                            Client.SendMessage("Ton GMLevel n'est pas assé elevé pour utiliser cette commande !")
                            Client.ConsoleMessage("Ton GMLevel n'est pas assé elevé pour utiliser cette commande !", 1)
                        End If
                    Case "gomap"

                        If ParamNum = 4 Then
                            Dim MapX As Integer = 0
                            Dim MapY As Integer = 0
                            Dim CellID As Integer = 0

                            Try
                                MapX = Integer.Parse(Parameters(0))
                                MapY = Integer.Parse(Parameters(1))
                                CellID = Integer.Parse(Parameters(2))
                            Catch ex As Exception
                            End Try

                            Dim Map As Map = MapsHandler.GetMapByPos(MapX, MapY)

                            Dim Other As GameClient = Players.GetCharacter(Parameters(3))

                            If Other.Character.State.InBattle = True Then
                                Client.SendMessage("Le joueur est en combat.")
                            End If

                            Other.Character.TeleportTo(Map.Id, CellID)
                            Client.ConsoleMessage("Le joueur <b>" & Other.Character.Name & "</b> a ete teleporte en <b>[" & MapX & "," & MapY & "]</b>.")
                            Client.SendNormalMessage("Le joueur <b>" & Other.Character.Name & "</b> a été téléporté en <b>[" & MapX & "," & MapY & "]</b>.")

                        ElseIf ParamNum = 3 Then
                            Dim MapX As Integer = 0
                            Dim MapY As Integer = 0
                            Dim CellID As Integer = 0

                            Try
                                MapX = Integer.Parse(Parameters(0))
                                MapY = Integer.Parse(Parameters(1))
                                CellID = Integer.Parse(Parameters(2))
                            Catch ex As Exception
                            End Try

                            Dim Map As Map = MapsHandler.GetMapByPos(MapX, MapY)

                            If Client.Character.State.InBattle = True Then
                                Client.SendMessage("Le joueur est en combat.")
                            End If

                            Client.Character.TeleportTo(Map.Id, CellID)
                            Client.ConsoleMessage("Le joueur <b>" & Client.Character.Name & "</b> a ete teleporte en <b>[" & MapX & "," & MapY & "]</b>.")
                            Client.SendNormalMessage("Le joueur <b>" & Client.Character.Name & "</b> a été téléporté en <b>[" & MapX & "," & MapY & "]</b>.")

                        Else
                            Client.ConsoleMessage("Syntaxe invalide !", 1)
                            Client.SendMessage("Syntaxe invalide !")
                        End If
                    Case "free"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.TeleportTo(Config.GetItem("START_MAP"), Config.GetItem("START_CELL"))
                                Client.ConsoleMessage("Le joueur " & FromClient.Character.Name & " est téléporter à la map start")
                            Else
                                Client.ConsoleMessage("Player " & Parameters(0) & "' doesn't exist.", 0)
                            End If

                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If
                    Case "spell"
                        If ParamNum >= 3 Then
                            Dim ToAddClient As GameClient = Players.GetCharacter(Parameters(2))
                            If Not ToAddClient Is Nothing Then
                                ToAddClient.Character.Spells.AddSpell(Parameters(0), Parameters(1))
                                Client.ConsoleMessage("Spell added to " & ToAddClient.Character.Name & ".")
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(2) & "' doesn't exist.", 0)
                            End If
                        ElseIf ParamNum >= 2 Then
                            Client.Character.Spells.AddSpell(Parameters(0), Parameters(1))
                            Client.ConsoleMessage("Spell added.")
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case "watch"
                        If ParamNum >= 1 Then

                            Dim ToWatchClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not ToWatchClient Is Nothing Then
                                If Not ToWatchClient.Infos.GmLevel > 0 Then
                                    Client.ConsoleMessage("Player " & ToWatchClient.Character.Name & " is now watched.")
                                    ToWatchClient.Character.WatchedBy = Client
                                Else
                                    Client.ConsoleMessage("Player '" & Parameters(0) & "' can't be watched.", 0)
                                End If
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case "unwatch"
                        If ParamNum >= 1 Then

                            Dim ToWatchClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not ToWatchClient Is Nothing Then
                                Client.ConsoleMessage("Player " & ToWatchClient.Character.Name & " is no longer watched.")
                                ToWatchClient.Character.WatchedBy = Nothing
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If

                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If
                    Case "testmb"
                        ' Client.Character.MobsSuiveurs.AddMobSuiveur(Client, MyObject.Args)
                    Case "kamas"
                        Client.Character.Player.Kamas += Config.GetItem("KAMAS_ADMIN")
                        Client.Character.SendAccountStats()
                        Client.SendNormalMessage("Vous avez recus " & Config.GetItem("KAMAS_ADMIN") & " kamas")

                    Case "teleto"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.TeleportTo(FromClient.Character.MapId, FromClient.Character.MapCell)
                                Client.ConsoleMessage("Teleported to " & FromClient.Character.Name & " position.")
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If

                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case "mj"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.Name = "[MJ]" & Client.Character.Name & ""
                                Client.Character.Save()
                                Client.SendNormalMessage("Changement du Nom avec succés votre nouveau nom est <b>" & FromClient.Character.Name & "</b> Merci de change votre perso Pour le voir.")
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case "admin"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.Name = "[ADMIN]" & Client.Character.Name & ""
                                Client.Character.Save()
                                Client.SendNormalMessage("Changement du Nom avec succés votre nouveau nom est <b>" & FromClient.Character.Name & "</b> Merci de change votre perso Pour le voir.")
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If
                    Case "graphist"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.Name = "[GRAPHIST]" & Client.Character.Name & ""
                                Client.Character.Save()
                                Client.SendNormalMessage("Changement du Nom avec succés votre nouveau nom est <b>" & FromClient.Character.Name & "</b> Merci de change votre perso Pour le voir.")
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If
                    Case "mp"
                        If ParamNum = 1 Then ' Admin tape un nom :D
                            Players.GetCharacter(Parameters(0))
                            Dim ReceiveClient As GameClient = Players.GetCharacter(Receiver)

                            If Not ReceiveClient Is Nothing Then

                                If Not ReceiveClient.Character.State.IsAway Then
                                    '(Client.Infos.GmLevel > 3


                                    ReceiveClient.Character.SendWatch("Chat", "de [Admin] " & Client.Character.Name & " : " & Message.Split("|")(0))
                                    ReceiveClient.Send("cMKF|" & Client.Character.ID & "|" & Client.Character.Name & "|" & Message)
                                    Client.Send("cMKT|" & Client.Character.ID & "|" & ReceiveClient.Character.Name & "|" & Message)


                                Else
                                    Client.Send("Im114" & ReceiveClient.Character.Name)
                                End If

                            Else
                                Client.Send("cMEf" & Receiver)
                            End If
                        End If

                    Case "change"
                        If ParamNum = 1 Then 'Si l'Admin rentre un paramètre (nom d'un personnage)
                            Players.GetCharacter(Parameters(0))
                            If Client.Character.Sexe = 0 Then 'Si le Exe du personnage est mâle
                                Client.Character.Sexe = 1 'Transformé en Femelle
                            ElseIf Client.Character.Sexe = 1 Then 'Inversement
                                Client.Character.Sexe = 0
                            End If
                        End If

                        If ParamNum = 0 Then 'Si l'Admin ne rentre pas de paramètre, cela change le sexe de son personnage
                            If Client.Character.Sexe = 0 Then
                                Client.Character.Sexe = 1
                            ElseIf Client.Character.Sexe = 1 Then
                                Client.Character.Sexe = 0
                            End If

                            Client.Character.Skin = Client.Character.Classe & Client.Character.Sexe
                            Client.Character.SkinStatic = False
                            Client.Character.GetMap.RefreshCharacter(Client)
                            Client.ConsoleMessage("Genre du personnage change !")
                            Client.SendNormalMessage("Genre du personnage changé !")

                        Else
                            Client.ConsoleMessage("Syntaxe invalide.")
                            Client.SendMessage("Syntaxe invalide.")
                        End If
                    Case "dev"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.Name = "[DEV]" & Client.Character.Name & ""
                                Client.Character.Save()
                                Client.SendNormalMessage("Changement du Nom avec succés votre nouveau nom est <b>" & FromClient.Character.Name & "</b> Merci de change votre perso Pour le voir.")
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If
                    Case "codeur"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.Name = "[CODEUR]" & Client.Character.Name & ""
                                Client.Character.Save()
                                Client.SendNormalMessage("Changement du Nom avec succés votre nouveau nom est <b>" & FromClient.Character.Name & "</b> Merci de change votre perso Pour le voir.")
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If


                    Case "animateur"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.Name = "[Animateur]" & Client.Character.Name & ""
                                Client.Character.Save()
                                Client.SendNormalMessage("Changement du Nom avec succés votre nouveau nom est <b>" & FromClient.Character.Name & "</b> Merci de change votre perso Pour le voir.")
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If


                    Case "co-createur"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.Name = "[CO-CREATEUR]" & Client.Character.Name & ""
                                Client.Character.Save()
                                Client.SendNormalMessage("Changement du Nom avec succés votre nouveau nom est <b>" & FromClient.Character.Name & "</b> Merci de change votre perso Pour le voir.")
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If


                    Case "createur"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.Name = "[CREATEUR]" & Client.Character.Name & ""
                                Client.Character.Save()
                                Client.SendNormalMessage("Changement du Nom avec succés votre nouveau nom est <b>" & FromClient.Character.Name & "</b> Merci de change votre perso Pour le voir.")
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case "vip"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.Name = "[VIP]" & Client.Character.Name & ""
                                Client.Character.Save()
                                Client.SendNormalMessage("Changement du Nom avec succés votre nouveau nom est <b>" & FromClient.Character.Name & "</b> Merci de change votre perso Pour le voir.")
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case Config.GetItem("CMD_NAME")
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                Client.Character.Name = "" & Config.GetItem("TITL_NAME") & Client.Character.Name & ""
                                Client.SendNormalMessage("Changement du Nom avec succés votre nouveau nom est <b>" & FromClient.Character.Name & "</b> Merci de change votre perso Pour le voir.")
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case "teleme"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                If Not FromClient.Character.State.Occuped Then
                                    FromClient.Character.TeleportTo(Client.Character.MapId, Client.Character.MapCell)
                                    Client.ConsoleMessage("Teleported " & FromClient.Character.Name & " to you.")
                                Else
                                    Client.ConsoleMessage("Player '" & Parameters(0) & "' is busy.", 0)
                                End If
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If

                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If
                    Case "xp"
                        Client.ConsoleMessage("You have gived " & Parameter & " xp for you")
                        Client.Character.Player.niveau.AddExp(Client, Parameter)
                        Client.Character.SendAccountStats()
                        Client.ConsoleMessage("You have gived " & Parameter & " honnors points to you")
                        Client.Character.Player.Alignment.AddExp(Client, Parameter)
                        Client.Character.SendAccountStats()

                    Case "addnpc"
                        If ParamNum >= 1 Then
                            Dim NewNPC As New Npc

                            NewNPC.TemplateID = Parameters(0)
                            NewNPC.PendingMap = Client.Character.GetMap.Id
                            NewNPC.CellID = Client.Character.MapCell
                            NewNPC.Direction = Client.Character.MapDir

                            If World.MapsHandler.Exist(NewNPC.PendingMap) Then
                                NewNPC.ID = World.MapsHandler.GetMap(NewNPC.PendingMap).NextID
                                World.MapsHandler.GetMap(NewNPC.PendingMap).NpcList.Add(NewNPC)
                                Client.Send("GM" & Client.Character.GetMap.NpcsPattern)
                                Client.ConsoleMessage("npc added !")
                            Else
                                Client.ConsoleMessage("Error this npc doesn't exist", 1)
                            End If
                        End If

                    Case "addnpcfix"

                        Dim NewNPC As New Npc

                        NewNPC.TemplateID = Parameters(0)
                        NewNPC.PendingMap = Client.Character.MapId
                        NewNPC.CellID = Client.Character.MapCell
                        NewNPC.Direction = Client.Character.MapDir

                        If ParamNum = 1 Then

                            NewNPC.ID = World.MapsHandler.GetMap(NewNPC.PendingMap).NextID
                            World.MapsHandler.GetMap(NewNPC.PendingMap).NpcList.Add(NewNPC)
                            Client.Send("GM" & Client.Character.GetMap.NpcsPattern)
                            Client.ConsoleMessage("PNJ ajoute !")
                            Client.ConsoleMessage("ATTENTION : Il se peut que le PNJ soit invisible ! Evitez d'orienter un PNJ de face, de dos ou de profil.", 3)
                            Client.SendNormalMessage("PNJ ajouté.")

                            SyncLock Sql.CharactersSync

                                Try

                                    Dim CreateString As String = "@mapid, @templateid, @cell, @dir"

                                    Dim SQLText As String = "INSERT INTO npcs_maps VALUES (" & CreateString & ")"
                                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)
                                    Dim P As MySqlParameterCollection = SQLCommand.Parameters

                                    P.Add(New MySqlParameter("@mapid", NewNPC.PendingMap))
                                    P.Add(New MySqlParameter("@templateid", NewNPC.TemplateID))
                                    P.Add(New MySqlParameter("@cell", NewNPC.CellID))
                                    P.Add(New MySqlParameter("@dir", NewNPC.Direction))
                                    SQLCommand.ExecuteNonQuery()

                                Catch ex As Exception
                                    Utils.MyConsole.Err("Can't create npc '@" & NewNPC.TemplateID & "@' : " & ex.Message)
                                End Try
                            End SyncLock

                        Else
                            Client.ConsoleMessage("Syntaxe invalide.", 2)
                        End If

                    Case "addquest"
                        If ParamNum >= 1 Then
                            'Client.Character.Quests.AddQuest(Client, Parameters(0))
                        End If

                    Case "teleport"
                        If ParamNum >= 3 Then

                            Dim MapId As Integer = Parameters(0)
                            Dim CellId As Integer = Parameters(1)
                            Client.Character.TeleportTo(MapId, CellId)
                            Client.ConsoleMessage("Vous venez de téléporter " & Client.Character.Name & " à " & MapId, CellId)

                        ElseIf ParamNum >= 2 Then

                            Dim MapId As Integer = Parameters(0)
                            Dim CellId As Integer = Parameters(1)
                            Client.Character.TeleportTo(MapId, CellId)
                            Client.ConsoleMessage("Vous venez de téléporter " & Client.Character.Name & " à " & MapId, CellId)
                        ElseIf ParamNum >= 1 Then

                            Dim MapId As Integer = Parameters(0)
                            Client.ConsoleMessage("Vous venez de téléporter " & Client.Character.Name & " à " & MapId)
                            Client.Character.TeleportTo(MapId, Client.Character.MapCell)

                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If

                    Case "whos"
                        If ParamNum >= 1 Then
                            Dim Whois As GameClient = World.Players.GetCharacter(Parameters(0))
                            If Not Whois Is Nothing Then
                                Dim Infos As String = _
                                    String.Format("{0} ({1}), IP: {2}, Level: {3}", _
                                           Whois.Character.Name, _
                                           Whois.AccountName, _
                                           Whois.Ip, _
                                           Whois.Character.Player.Level)
                                Client.ConsoleMessage(Infos)
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If
                        Else
                            Client.ConsoleMessage("Invalid syntax.", 1)
                        End If
                    Case "jail"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                If Not FromClient.Character.State.Occuped Then
                                    FromClient.Character.TeleportTo(JAIL_MAP, JAIL_CELL)
                                    Client.ConsoleMessage("Teleported " & FromClient.Character.Name & " to jail.")
                                Else
                                    Client.ConsoleMessage("Player '" & Parameters(0) & "' is busy.", 0)
                                End If
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If

                        Else

                            Client.ConsoleMessage("You are teleported to jail.")
                            Client.Character.TeleportTo(JAIL_MAP, SAFE_CELL)

                        End If

                    Case "guilde"
                        If Client.Character.State.InBattle Then
                            Client.SendNormalMessage("Action impossible en combat !")
                            Exit Select
                        End If
                        Client.Send("gn")

                    Case "unjail"
                        If ParamNum >= 1 Then

                            Dim FromClient As GameClient = Players.GetCharacter(Parameters(0))
                            If Not FromClient Is Nothing Then
                                If Not FromClient.Character.State.Occuped Then
                                    If FromClient.Character.MapId = JAIL_MAP Then
                                        FromClient.Character.TeleportTo(Config.GetItem("START_MAP"), Config.GetItem("START_CELL"))
                                        Client.ConsoleMessage("Teleported " & FromClient.Character.Name & " to safe area.")
                                    Else
                                        Client.ConsoleMessage("Player '" & Parameters(0) & "' isn't in jail.", 0)
                                    End If
                                Else
                                    Client.ConsoleMessage("Player '" & Parameters(0) & "' is busy.", 0)
                                End If
                            Else
                                Client.ConsoleMessage("Player '" & Parameters(0) & "' doesn't exist.", 0)
                            End If

                        Else

                            Client.ConsoleMessage("You are teleported to a safe area.")
                            Client.Character.TeleportTo(Config.GetItem("START_MAP"), Config.GetItem("START_CELL"))

                        End If

                    Case Else
                        Client.ConsoleMessage("Unknown command.", 1)

                End Select

            Catch ex As Exception
                MyConsole.Err("Exception on @" & Client.Character.Name & "@'s admin cmd '@" & Command & "@' : " & ex.Message)
                MyConsole.Err(ex.ToString)
                Client.ConsoleMessage("Invalid syntax", 1)
            End Try

        End Sub

        Private Shared Sub LaunchSpellHorsCombat(ByVal Parameters As String, ByVal Parameters1 As String, ByVal Client As GameClient)
            Throw New NotImplementedException
        End Sub

        Private Shared Sub addnpcfix(ByVal Client As GameClient, ByVal Parameters As String)
            Throw New NotImplementedException
        End Sub

        Shared Sub AddItem(ByVal Client As GameClient, ByVal S As String, ByVal p3 As Integer)
            Throw New NotImplementedException
        End Sub

    End Class
End Namespace