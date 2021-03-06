﻿Imports Podemu.Game.Storages
Imports Podemu.Game.Interactives
Imports MySql.Data.MySqlClient
Imports System.Text
Imports Podemu.Utils
Imports Podemu.Utils.Basic
Imports Podemu.World

Namespace Game
    Public Class GameParser

        Private Client As GameClient

        Public Sub New(ByVal eClient As GameClient)
            Client = eClient
        End Sub

        Public Sub Unpack(ByVal Packet As String)

            SyncLock Client.Lock

                Try

                    Select Case Client.State.State

                        Case GameState.WaitingPacket.Ticket
                            ParseTicket(Packet)

                        Case GameState.WaitingPacket.Character
                            ParseCharacter(Packet)

                        Case GameState.WaitingPacket.Create
                            ParseCreate(Packet)

                        Case GameState.WaitingPacket.Basic
                            ParseBasic(Packet)

                    End Select

                Catch ex As Exception
                    Utils.MyConsole.Err(ex.ToString)
                    Debug.Print(ex.ToString)
                End Try

            End SyncLock

        End Sub

        Private Sub UnknownPacket(ByVal Packet As String, Optional ByVal Fatal As Boolean = False)

            If Fatal Then Client.State.State = GameState.WaitingPacket.None
            Client.Send("BN")

        End Sub
        Private Shared WithEvents TimerActions As New Timers.Timer
        Public Shared Numero As Integer = 1

        Private Sub LoadCharacters()

            For i As Integer = 0 To Client.Infos.CharacterNumber - 1

                If CharactersManager.CharactersList.ContainsKey(Client.Infos.CharactersList(ServerId)(i)) Then

                    Dim LoadChar As Character = CharactersManager.CharactersList(Client.Infos.CharactersList(ServerId)(i))

                    If Not Client.CharacterList.ContainsKey(LoadChar.ID) Then
                        Client.CharacterList.Add(LoadChar.ID, LoadChar)
                    End If

                Else
                    DeleteCharacterFromAccount(Client.Infos.CharactersList(ServerId)(i))
                End If

            Next

        End Sub

        Private Sub Reconnect(ByVal Character As Character)
            If Config.GetItem("ACTIVE_RECONNECT") = "True" Then


                Dim Battle = Character.State.GetFight
                Client.Send("ArO")
                SendCharacterList()
                SelectCharacter(Character.ID, True)
                Client.State.Created = False
                CreateGame()

                Threading.Thread.Sleep(1000)
                Battle.Join(Client, False)
                Client.Character.State.GetFighter.DecoTurn = 0
                Battle.SendMessage("Le joeure <b> " & Character.Name & "</b> vient de se reconnecter")
            Else
                Client.SendNormalMessage("Le mode reconnexion est désactiver, vous nou pouvez pas continuer ce combat")
            End If

        End Sub

        Private Sub ParseTicket(ByVal Packet As String)

            If Packet.Substring(0, 2) = "AT" Then
                Dim Ticket As String = Packet.Substring(2)

                If World.LoginKeys.TicketExist(Ticket) Then
                    Dim Key As LoginKey = World.LoginKeys.GetKey(Ticket)
                    If Not Key.IsOutdated Then

                        Client.AccountName = Key.AccountName
                        Client.Infos = Key.SqlInfos

                        LoginKeys.KeyList.Remove(Key)

                        If Players.GetPlayerCount() > 0 Then
                            Dim AlreadyAccountPlayers = Players.GetPlayers().Where(Function(p) p.AccountName = Client.AccountName)
                            For Each player In AlreadyAccountPlayers
                                If player IsNot Client Then
                                    player.Disconnect()
                                End If
                            Next
                        End If

                        LoadCharacters()

                        Client.State.State = GameState.WaitingPacket.Character
                        Client.Send("ATK0")

                        If Not Server.RealmLink.RealmSocket Is Nothing Then
                            Server.RealmLink.RealmSocket.Send("ANANAS " & Client.AccountName)
                        End If

                        For Each Character In Client.CharacterList
                            If Character.Value.State.InBattle Then
                                Reconnect(Character.Value)
                                Exit Sub
                            End If
                        Next

                    Else
                        Client.State.State = GameState.WaitingPacket.None
                        Client.Send("ATE")
                    End If
                Else
                    Client.State.State = GameState.WaitingPacket.None
                    Client.Send("ATE")
                End If

            Else
                UnknownPacket(Packet, True)
            End If

        End Sub

        Public Sub SendCharacterList()
            'Hamza :D
            Dim SubscriptionTime As Long
            If Config.GetItem(Of Boolean)("ENABLE_SUBSCRIPTION") Then
                SubscriptionTime = Client.Infos.SubscriptionTime
                If SubscriptionTime < 0 Then SubscriptionTime = 0
            Else
                SubscriptionTime = -1
            End If

            Dim ListCharacter As New StringBuilder(String.Concat("ALK", SubscriptionTime, "|", Client.Infos.CharacterNumber))

            If Client.Infos.CharacterNumber > 0 Then

                For Each Character As Character In Client.CharacterList.Values
                    ListCharacter.Append("|" & Character.PatternOnCharChoose)
                Next

            End If

            Client.Send(ListCharacter.ToString())

        End Sub

        Private Sub SendRandomName()

            Client.Send("APK" & Name.GetRandomName)

        End Sub

        Private Sub SelectCharacter(ByVal CharId As Integer, ByVal Reco As Boolean)

            If Client.CharacterList.ContainsKey(CharId) Then

                Dim SelectedChar As Character = Client.CharacterList(CharId)

                If Config.GetItem(Of Boolean)("ACTIVE_HEROIC") AndAlso SelectedChar.IsDead Then
                    Client.Send("ASE")
                    Exit Sub
                End If

                If SelectedChar.State.IsMerchant Then
                    Dim mMap = SelectedChar.GetMap
                    mMap.GetMerchand(SelectedChar.ID).EndTradeWithAll()
                    mMap.MerchantList.Remove(SelectedChar)
                    mMap.RemoveEntity(SelectedChar.ID)
                    SelectedChar = CharactersManager.ToCharacter(SelectedChar)
                End If

                If Not Reco Then SelectedChar.State = New CharacterState(SelectedChar)

                SelectedChar.SetClient(Client)
                Client.Character = SelectedChar
                Client.State.State = GameState.WaitingPacket.Create
                Client.Send("ASK" & SelectedChar.PatternOnCharSelect)

            Else
                Client.Send("ASE")
            End If

        End Sub

        Private Sub DeleteCharacterFromAccount(ByVal CharacterName As String)

            Dim CharacterNumber As Integer = Client.Infos.CharacterTotalNumber

            Dim NewString As New StringBuilder

            If CharacterNumber <= 0 Then Exit Sub

            If (CharacterNumber >= 2) Then

                For Each Serv As Integer In Client.Infos.CharactersList.Keys
                    For Each Character As String In Client.Infos.CharactersList(Serv)
                        If Serv <> ServerId OrElse Character <> CharacterName Then NewString.Append(Character & "," & Serv & "|")
                    Next
                Next
                If NewString.Length > 0 Then NewString = New StringBuilder(Mid(NewString.ToString(), 1, NewString.Length - 1))

            End If

            SyncLock Sql.AccountsSync

                Dim SQLText As String = "UPDATE player_accounts SET characters=@NewCharacters WHERE username=@AccountName"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                SQLCommand.Parameters.Add(New MySqlParameter("@AccountName", Client.AccountName))
                SQLCommand.Parameters.Add(New MySqlParameter("@NewCharacters", NewString))
                SQLCommand.ExecuteNonQuery()

            End SyncLock

        End Sub

        Private Sub DeleteCharacterFromDatabase(ByVal CharacterName As String)

            SyncLock Sql.CharactersSync

                Dim SQLText As String = "DELETE FROM player_characters WHERE nom=@CharacterName"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)
                SQLCommand.Parameters.Add(New MySqlParameter("@CharacterName", CharacterName))

                SQLCommand.ExecuteNonQuery()

                CharactersManager.CharactersList.Remove(CharacterName)

            End SyncLock

        End Sub

        Private Sub DeleteCharacter(ByVal CharId As Integer)

            Client.Send("BN")

            If Client.CharacterList.ContainsKey(CharId) Then

                Dim CharacterName As String = Client.CharacterList(CharId).Name
                DeleteCharacterFromAccount(CharacterName)
                DeleteCharacterFromDatabase(CharacterName)

                Client.Infos.DelCharacter(CharacterName)
                Client.CharacterList.Remove(CharId)
                SendCharacterList()

                GameStats.AddToStats(GameStats.GameRightStat.CharactersWon, -1)

            End If

        End Sub

        Private Sub AddCharacterToAccount(ByVal CharacterName As String)

            Dim NewCharacters As New StringBuilder

            For Each Serv As Integer In Client.Infos.CharactersList.Keys
                For Each Character As String In Client.Infos.CharactersList(Serv)
                    NewCharacters.Append(Character & "," & Serv & "|")
                Next
            Next

            NewCharacters.Append(CharacterName & "," & ServerId)

            SyncLock Sql.AccountsSync

                Dim SQLText As String = "UPDATE player_accounts SET characters=@NewCharacters WHERE username=@AccountName"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                SQLCommand.Parameters.Add(New MySqlParameter("@AccountName", Client.AccountName))
                SQLCommand.Parameters.Add(New MySqlParameter("@NewCharacters", NewCharacters))
                SQLCommand.ExecuteNonQuery()

            End SyncLock

        End Sub

        Private Sub CreateChararacter(ByVal ExtraData As String)

            Dim DataSplit() As String = ExtraData.Split("|")

            If (DataSplit.Length = 6) Then

                If Not Character.Exist(DataSplit(0)) Then
                    If Name.IsCorrectName(DataSplit(0)) Then

                        Client.Character = New Character

                        With Client.Character

                            .Name = Name.MakeCorrectName(DataSplit(0))
                            .ID = Character.GetActualID()
                            .Classe = DataSplit(1)
                            .Sexe = DataSplit(2)
                            .Skin = .Classe & .Sexe
                            .Size = 100
                            If Config.GetItem("MODE_LIGHT") = "False" Then
                                .Player.Level = Config.GetItem("START_LEVEL")
                            End If
                            If Config.GetItem("MODE_LIGHT") = "True" Then
                                .Player.Level = "1"
                            End If

                            If .Classe < 1 Or .Classe > 12 Or .Sexe < 0 Or .Sexe > 1 Then
                                Client.Send("AAE")
                                Exit Sub
                            End If

                            .Color(0) = DataSplit(3)
                            .Color(1) = DataSplit(4)
                            .Color(2) = DataSplit(5)
                            If Config.GetItem("MODE_LIGHT") = "False" Then
                                .Player.Kamas = CLng(Config.GetItem("START_KAMAS"))
                            End If
                            If Config.GetItem("MODE_LIGHT") = "True" Then
                                .Player.Kamas = "0"
                            End If
                            .Player.CharactPoint = (Config.GetItem("START_LEVEL") - 1) * 5
                            .Player.SpellPoint = Config.GetItem("START_LEVEL") - 1
                            .Player.Exp = ExperienceTable.AtLevel(Config.GetItem("START_LEVEL")).Character

                            .Player.Stats.Base.Force.Base = Config.GetItem("START_STATS")
                            .Player.Stats.Base.Vitalite.Base = Config.GetItem("START_STATS")
                            .Player.Stats.Base.Sagesse.Base = Config.GetItem("START_STATS")
                            .Player.Stats.Base.Agilite.Base = Config.GetItem("START_STATS")
                            .Player.Stats.Base.Chance.Base = Config.GetItem("START_STATS")
                            .Player.Stats.Base.Intelligence.Base = Config.GetItem("START_STATS")

                            .Player.Life = .Player.MaximumLife
                            .Player.Energy = 10000

                            .MapId = Config.GetItem("START_MAP")
                            .MapCell = Config.GetItem("START_CELL")
                            .MapDir = Config.GetItem("START_DIR")

                            .Items.ClearItems()
                            .Spells.ClearSpells()

                            SpellsHandler.LearnSpells(Client.Character)

                            AddCharacterToAccount(.Name)

                            .Create()

                        End With

                    CharactersManager.CharactersList.Add(Client.Character.Name, Client.Character)

                    Client.Send("AAK")
                    Client.Infos.AddCharacter(Client.Character.Name)

                    Client.CharacterList.Add(Client.Character.ID, Client.Character)
                    SendCharacterList()

                    GameStats.AddToStats(GameStats.GameRightStat.CharactersWon, 1)

                Else
                    Client.Send("AAE")
                End If

            Else
                Client.Send("AAEa")
            End If

            Else
            Client.Send("AAE")
            End If

        End Sub

        Private Sub ParseCharacter(ByVal Packet As String)

            If Packet.Substring(0, 1) = "A" Then

                Select Case Packet.Substring(1, 1)

                    Case "i", "k", "f"
                        Client.Send("BN")

                    Case "A"
                        CreateChararacter(Packet.Substring(2))

                    Case "D"
                        DeleteCharacter(Packet.Substring(2).Replace("|", ""))

                    Case "L"
                        SendCharacterList()

                    Case "g"
                        SendGiftsList()

                    Case "G"
                        AssignGift(Packet.Substring(2))

                    Case "R"
                        ResetCharacter(Packet.Substring(2))

                    Case "P"
                        SendRandomName()

                    Case "S"
                        SelectCharacter(Packet.Substring(2), False)

                    Case "V"
                        Client.Send("AV0")

                    Case Else
                        UnknownPacket(Packet, True)

                End Select

            Else
                UnknownPacket(Packet, True)
            End If

        End Sub

        Private Sub ResetCharacter(ByVal CharacterId As Integer)

            If Client.CharacterList.ContainsKey(CharacterId) Then

                Dim Character As Character = Client.CharacterList(CharacterId)

                If Character.DeathCount >= Config.GetItem(Of Integer)("MAX_DEATH") Then
                    Exit Sub
                End If

                Character.Skin = Character.Classe & Character.Sexe
                Character.Size = 100
                Character.Player.Level = Config.GetItem("START_LEVEL")

                If Character.Classe < 1 Or Character.Classe > 12 Or Character.Sexe < 0 Or Character.Sexe > 1 Then
                    Client.Send("ARE")
                    Exit Sub
                End If

                Character.Player.Kamas = CLng(Config.GetItem("START_KAMAS"))
                Character.Player.CharactPoint = (Config.GetItem("START_LEVEL") - 1) * 5
                Character.Player.SpellPoint = Config.GetItem("START_LEVEL") - 1
                Character.Player.Exp = ExperienceTable.AtLevel(Config.GetItem("START_LEVEL")).Character

                Character.Player.Stats.Base.Force.Base = Config.GetItem("START_STATS")
                Character.Player.Stats.Base.Vitalite.Base = Config.GetItem("START_STATS")
                Character.Player.Stats.Base.Sagesse.Base = Config.GetItem("START_STATS")
                Character.Player.Stats.Base.Agilite.Base = Config.GetItem("START_STATS")
                Character.Player.Stats.Base.Chance.Base = Config.GetItem("START_STATS")
                Character.Player.Stats.Base.Intelligence.Base = Config.GetItem("START_STATS")

                Character.Player.Life = Character.Player.MaximumLife
                Character.Player.Energy = 10000

                Character.Restriction.Restriction = 0

                Character.MapId = Config.GetItem("START_MAP")
                Character.MapCell = Config.GetItem("START_CELL")
                Character.MapDir = Config.GetItem("START_DIR")

                Character.SaveMap = 0
                Character.SaveCell = 0

                Character.Mount = Nothing

                Character.IsDead = False

                Character.MerchantBag.ClearItems()
                Character.Items.ClearItems()
                Character.Spells.ClearSpells()
                Character.Zaaps.Clear()

                SpellsHandler.LearnSpells(Character)

                SendCharacterList()
            Else
                Client.Send("ARE")
            End If


        End Sub

        Private Sub SendGiftsList()

            For Each Gift As GiftTemplate In Client.Infos.Gifts

                Client.Send("Ag" & String.Join("|", "1", Gift.ID, Gift.Name, Gift.Description, Gift.GfxUrl, String.Join(";", Gift.Items.Select(Function(i) i.ToString()))))

            Next

        End Sub

        Private Sub AssignGift(ByVal data As String)
            Dim params() As String = data.Split("|")
            Dim chId As Integer = params(1)
            Dim character As Character = Client.CharacterList.FirstOrDefault(Function(c) c.Value.ID = chId).Value
            Dim gift As GiftTemplate = GiftsHandler.GetTemplate(CInt(params(0)))

            If character IsNot Nothing AndAlso gift IsNot Nothing Then
                For Each Item As Item In gift.Items
                    character.Items.AddItemOffline(Item)
                Next
                Client.Infos.Gifts.Remove(gift)
                Server.RealmLink.RealmSocket.DelGift(Client, gift.ID)
                Client.Send("AG0")
                Exit Sub
            End If
            Client.Send("AGE")
        End Sub

        Private Function GetActualTime() As Integer
            Return (Now.Hour * 3600000 + Now.Minute * 60000 + Now.Second * 1000 + Now.Millisecond)
        End Function

        Private Sub CreateGame()

            If Client.Character IsNot Nothing Then
                If Client.Character.State.GetFighter IsNot Nothing Then
                    'Attente fin fight
                    If Client.Character.State.GetFighter.WaitingResult Then
                        While Client.Character.State.GetFighter.WaitingResult
                            Threading.Thread.Sleep(20)
                        End While
                    End If
                End If
            End If

            Client.Send("GCK|1|" & Client.Character.Name)
            Client.Send("AR" & Client.Character.Rights)

            If Not Client.State.Created Then
                Client.State.Created = True
                Client.Send("cC+" & Client.Character.Channels)
                World.Players.SendWelcomeMessage(Client)
                If Config.GetItem("PUB_ACTIV") = "True" Then
                    Client.SendMessage("<b>(PUB): </b>" & Config.GetItem("PUB1") & ".")
                End If
                If Config.GetItem("PUB_ACTIV") = "True" Then
                    If TimerActions.Interval = Utils.Config.GetItem("TEMPS_PUB") Then
                        Client.SendMessage("<b>(PUB): </b>" & Config.GetItem("PUB2") & ".")
                    End If
                End If
                If Config.GetItem("ACTIVE_HEROIC") = "True" Then
                    Client.SendMessage("Mode Heroic")
                End If
                If Config.GetItem("ENABLE_SUBSCRIPTION") = "False" Then
                    Client.SendMessage("Mode Beta Test")
                End If
                If Config.GetItem("MODE_LIGHT") = "True" Then
                    Client.SendMessage("Mode Light")
                End If

                Client.Character.Items.RefreshItems(Client, True)
                Client.Send("SLo+")
                Client.Send(Client.Character.Spells.GetAllSpellList)
                If Config.GetItem("ENABLE_NIGHT") = True Then Client.Send("BT" & GetActualTime())
            Else
                Client.Character.SendAccountStats()
                Client.Character.SendPods()
            End If

            Client.State.State = GameState.WaitingPacket.Basic

            World.Players.LoadMap(Client, Client.Character.MapId, Client.Character.MapCell, False)
            Client.Send(Client.Character.EmoteListPattern)

            If (Client.Character.Mount IsNot Nothing) Then
                Client.Character.Mount.Equip(Client)
            End If

            If Not Client.Character.GuildID = 0 And Not Client.Character.GuildID = Nothing Then
                Client.Send(Client.Character.PatternSendGuild)
                World.GuildHandler.ListOfPlayers.Add(Client)
            End If

        End Sub

        Private Sub ParseCreate(ByVal Packet As String)

            Select Case Packet(0)

                Case "A"

                    Select Case Packet(1)

                        Case "f"

                        Case Else
                            UnknownPacket(Packet, True)

                    End Select

                Case "B"

                    Select Case Packet(1)

                        Case "D"
                            SendDate()

                        Case Else
                            UnknownPacket(Packet, True)

                    End Select

                Case "G"

                    Select Case Packet(1)


                        Case "C"
                            CreateGame()

                        Case "K"

                        Case Else
                            UnknownPacket(Packet, True)

                    End Select

                Case Else
                    UnknownPacket(Packet, True)

            End Select

        End Sub

        Private Sub SendGameInformations()

            If Not Client.Character.State.InBattle Then Client.Character.GetMap.AddCharacter(Client)

            If World.PaddockManager.ExistOnMap(Client.Character.MapId) Then
                For Each Paddock As Paddock In World.PaddockManager.PaddocksOnMap(Client.Character.MapId)
                    Client.Send("Rp" & Paddock.PaddockDisplayInfo)
                Next
            End If

            Client.Send("GDK")

        End Sub

        Private Sub SendDate()
            Client.Send("BD" & (Now.Year - 1370).ToString & "|" & (Now.Month - 1) & "|" & Now.Day)
        End Sub

        Private Sub DeleteItem(ByVal ExtraData As String)

            If Client.Character.State.InBattle AndAlso Client.Character.State.GetFight.State <> Fight.FightState.Starting Then
                Client.Send("BN")
                Exit Sub
            End If

            If Client.Character.State.Occuped AndAlso Not Client.Character.State.InBattle Then
                Client.Send("BN")
                Exit Sub
            End If

            Dim Data() As String = ExtraData.Split("|")
            Dim ID As Integer = Data(0)
            Dim Quantity As Integer = Data(1)
            Client.Character.Items.DeleteItem(Client, ID, Quantity)

        End Sub

        Private Sub MoveItem(ByVal ExtraData As String)

            If Client.Character.State.InBattle AndAlso Client.Character.State.GetFight.State <> Fight.FightState.Starting Then
                Client.Send("BN")
                Exit Sub
            End If

            If Client.Character.State.Occuped AndAlso Not Client.Character.State.InBattle Then
                Client.Send("BN")
                Exit Sub
            End If

            Dim Data() As String = ExtraData.Split("|")

            Dim ID As Integer = Data(0)
            Dim Position As Integer = Data(1)
            Dim Quantity As Integer = 1
            If Data.Length > 2 Then Quantity = Data(2)

            Client.Character.Items.MoveItem(Client, ID, Position, Quantity)

        End Sub

        Private Sub UseItem(ByVal ExtraData As String)

            If Client.Character.State.InBattle AndAlso Client.Character.State.GetFight.State <> Fight.FightState.Starting Then
                Client.Send("BN")
                Exit Sub
            End If

            If Client.Character.State.Occuped AndAlso Not Client.Character.State.InBattle Then
                Client.Send("BN")
                Exit Sub
            End If

            Dim Data() As String = ExtraData.Split("|")
            Dim ID As Integer = Data(0)
            Dim TargetId As String = Data(1)
            Client.Character.Items.UseItem(Client, ID, TargetId)

        End Sub

        Private Sub UpgradeSpell(ByVal SpellID As String)

            If Not IsNumeric(SpellID) Then Exit Sub
            Dim iSpellID As Integer = CInt(SpellID)

            Dim ActualLevel As Integer = Client.Character.Spells.GetSpellLevel(iSpellID)

            If ActualLevel = 0 OrElse Client.Character.Player.SpellPoint < ActualLevel Then
                Client.Send("SUE")
                Exit Sub
            End If

            Client.Character.Player.SpellPoint -= ActualLevel
            Client.Character.Spells.UpSpell(SpellID)

            Client.Send("SUK" & SpellID & "~" & ActualLevel + 1)
            Client.Character.SendAccountStats()

        End Sub

        Private Sub AccountBoost(ByVal CaractID As Integer)

            With Client.Character.Player

                Dim Classe As Integer = Client.Character.Classe
                Dim PointCount As Integer = 0

                If CaractID = 11 Then ' Vitalité

                    If .CharactPoint >= 1 Then

                        If Classe = 11 Then
                            .Stats.Base.Vitalite.Base += 2 * Utils.Config.GetItem("STATS_RATE")
                            .Life += 2 * Utils.Config.GetItem("STATS_RATE")
                        Else
                            .Stats.Base.Vitalite.Base += 1 * Utils.Config.GetItem("STATS_RATE")
                            .Life += 1 * Utils.Config.GetItem("STATS_RATE")
                        End If

                        .CharactPoint -= 1
                        Client.Character.SendAccountStats()

                    End If

                End If

                If CaractID = 12 Then ' Sagesse

                    If .CharactPoint >= 3 Then

                        .Stats.Base.Sagesse.Base += 1 * Utils.Config.GetItem("STATS_RATE")
                        .CharactPoint -= 3
                        Client.Character.SendAccountStats()

                    End If

                End If

                If CaractID = 10 Then ' Force

                    If Classe = 1 Or Classe = 7 Then
                        If .Stats.Base.Force.Base < 51 Then PointCount = 2
                        If .Stats.Base.Force.Base > 50 Then PointCount = 3
                        If .Stats.Base.Force.Base > 150 Then PointCount = 4
                        If .Stats.Base.Force.Base > 250 Then PointCount = 5
                    End If

                    If Classe = 2 Or Classe = 5 Then
                        If .Stats.Base.Force.Base < 51 Then PointCount = 2
                        If .Stats.Base.Force.Base > 50 Then PointCount = 3
                        If .Stats.Base.Force.Base > 150 Then PointCount = 4
                        If .Stats.Base.Force.Base > 250 Then PointCount = 5
                    End If

                    If Classe = 3 Or Classe = 9 Then
                        If .Stats.Base.Force.Base < 51 Then PointCount = 1
                        If .Stats.Base.Force.Base > 50 Then PointCount = 2
                        If .Stats.Base.Force.Base > 150 Then PointCount = 3
                        If .Stats.Base.Force.Base > 250 Then PointCount = 4
                        If .Stats.Base.Force.Base > 350 Then PointCount = 5

                    End If

                    If Classe = 4 Or Classe = 6 Or Classe = 8 Or Classe = 10 Then
                        If .Stats.Base.Force.Base < 101 Then PointCount = 1
                        If .Stats.Base.Force.Base > 100 Then PointCount = 2
                        If .Stats.Base.Force.Base > 200 Then PointCount = 3
                        If .Stats.Base.Force.Base > 300 Then PointCount = 4
                        If .Stats.Base.Force.Base > 400 Then PointCount = 5
                    End If

                    If Classe = 11 Then
                        PointCount = 3
                    End If

                    If Classe = 12 Then
                        If .Stats.Base.Force.Base < 51 Then PointCount = 1
                        If .Stats.Base.Force.Base > 50 Then PointCount = 2
                        If .Stats.Base.Force.Base > 200 Then PointCount = 3
                    End If

                    If .CharactPoint >= PointCount Then
                        .Stats.Base.Force.Base += 1 * Utils.Config.GetItem("STATS_RATE")
                        .CharactPoint -= PointCount
                        Client.Character.SendAccountStats()
                        Client.Character.SendPods()
                    Else
                        Client.Send("ABE")
                    End If

                End If

                If CaractID = 15 Then ' Intelligence

                    If Classe = 1 Or Classe = 2 Or Classe = 5 Or Classe = 7 Or Classe = 10 Then
                        If .Stats.Base.Intelligence.Base < 101 Then PointCount = 1
                        If .Stats.Base.Intelligence.Base > 100 Then PointCount = 2
                        If .Stats.Base.Intelligence.Base > 200 Then PointCount = 3
                        If .Stats.Base.Intelligence.Base > 300 Then PointCount = 4
                        If .Stats.Base.Intelligence.Base > 400 Then PointCount = 5
                    End If

                    If Classe = 3 Then
                        If .Stats.Base.Intelligence.Base < 21 Then PointCount = 1
                        If .Stats.Base.Intelligence.Base > 20 Then PointCount = 2
                        If .Stats.Base.Intelligence.Base > 60 Then PointCount = 3
                        If .Stats.Base.Intelligence.Base > 100 Then PointCount = 4
                        If .Stats.Base.Intelligence.Base > 140 Then PointCount = 5
                    End If

                    If Classe = 4 Then
                        If .Stats.Base.Intelligence.Base < 51 Then PointCount = 2
                        If .Stats.Base.Intelligence.Base > 50 Then PointCount = 3
                        If .Stats.Base.Intelligence.Base > 150 Then PointCount = 4
                        If .Stats.Base.Intelligence.Base > 250 Then PointCount = 5
                    End If

                    If Classe = 6 Or Classe = 8 Then
                        If .Stats.Base.Intelligence.Base < 21 Then PointCount = 1
                        If .Stats.Base.Intelligence.Base > 20 Then PointCount = 2
                        If .Stats.Base.Intelligence.Base > 40 Then PointCount = 3
                        If .Stats.Base.Intelligence.Base > 60 Then PointCount = 4
                        If .Stats.Base.Intelligence.Base > 80 Then PointCount = 5
                    End If

                    If Classe = 9 Then
                        If .Stats.Base.Intelligence.Base < 51 Then PointCount = 1
                        If .Stats.Base.Intelligence.Base > 50 Then PointCount = 2
                        If .Stats.Base.Intelligence.Base > 150 Then PointCount = 3
                        If .Stats.Base.Intelligence.Base > 250 Then PointCount = 4
                        If .Stats.Base.Intelligence.Base > 350 Then PointCount = 5
                    End If

                    If Classe = 11 Then
                        PointCount = 3
                    End If

                    If Classe = 12 Then
                        If .Stats.Base.Intelligence.Base < 51 Then PointCount = 1
                        If .Stats.Base.Intelligence.Base > 50 Then PointCount = 2
                        If .Stats.Base.Intelligence.Base > 200 Then PointCount = 3
                    End If

                    If .CharactPoint >= PointCount Then
                        .Stats.Base.Intelligence.Base += 1 * Utils.Config.GetItem("STATS_RATE")
                        .CharactPoint -= PointCount
                        Client.Character.SendAccountStats()
                    Else
                        Client.Send("ABE")
                    End If

                End If

                If CaractID = 13 Then ' Chance

                    If Classe = 1 Or Classe = 4 Or Classe = 5 Or Classe = 6 Or Classe = 7 Or Classe = 8 Or Classe = 9 Then
                        If .Stats.Base.Chance.Base < 21 Then PointCount = 1
                        If .Stats.Base.Chance.Base > 20 Then PointCount = 2
                        If .Stats.Base.Chance.Base > 40 Then PointCount = 3
                        If .Stats.Base.Chance.Base > 60 Then PointCount = 4
                        If .Stats.Base.Chance.Base > 80 Then PointCount = 5
                    End If

                    If Classe = 2 Or Classe = 10 Then
                        If .Stats.Base.Chance.Base < 101 Then PointCount = 1
                        If .Stats.Base.Chance.Base > 100 Then PointCount = 2
                        If .Stats.Base.Chance.Base > 200 Then PointCount = 3
                        If .Stats.Base.Chance.Base > 300 Then PointCount = 4
                        If .Stats.Base.Chance.Base > 400 Then PointCount = 5
                    End If

                    If Classe = 3 Then
                        If .Stats.Base.Chance.Base < 101 Then PointCount = 1
                        If .Stats.Base.Chance.Base > 100 Then PointCount = 2
                        If .Stats.Base.Chance.Base > 150 Then PointCount = 3
                        If .Stats.Base.Chance.Base > 230 Then PointCount = 4
                        If .Stats.Base.Chance.Base > 330 Then PointCount = 5
                    End If

                    If Classe = 11 Then
                        PointCount = 3
                    End If

                    If Classe = 12 Then
                        If .Stats.Base.Chance.Base < 51 Then PointCount = 1
                        If .Stats.Base.Chance.Base > 50 Then PointCount = 2
                        If .Stats.Base.Chance.Base > 200 Then PointCount = 3
                    End If

                    If .CharactPoint >= PointCount Then
                        .Stats.Base.Chance.Base += 1 * Utils.Config.GetItem("STATS_RATE")
                        .CharactPoint -= PointCount
                        Client.Character.SendAccountStats()
                    Else
                        Client.Send("ABE")
                    End If

                End If

                If CaractID = 14 Then ' Agilité

                    If Classe = 1 Or Classe = 2 Or Classe = 3 Or Classe = 5 Or Classe = 7 Or Classe = 8 Then
                        If .Stats.Base.Agilite.Base < 21 Then PointCount = 1
                        If .Stats.Base.Agilite.Base > 20 Then PointCount = 2
                        If .Stats.Base.Agilite.Base > 40 Then PointCount = 3
                        If .Stats.Base.Agilite.Base > 60 Then PointCount = 4
                        If .Stats.Base.Agilite.Base > 80 Then PointCount = 5
                    End If

                    If Classe = 4 Then
                        If .Stats.Base.Agilite.Base < 101 Then PointCount = 1
                        If .Stats.Base.Agilite.Base > 100 Then PointCount = 2
                        If .Stats.Base.Agilite.Base > 200 Then PointCount = 3
                        If .Stats.Base.Agilite.Base > 300 Then PointCount = 4
                        If .Stats.Base.Agilite.Base > 400 Then PointCount = 5
                    End If

                    If Classe = 6 Or Classe = 9 Then
                        If .Stats.Base.Agilite.Base < 51 Then PointCount = 1
                        If .Stats.Base.Agilite.Base > 50 Then PointCount = 2
                        If .Stats.Base.Agilite.Base > 100 Then PointCount = 3
                        If .Stats.Base.Agilite.Base > 150 Then PointCount = 4
                        If .Stats.Base.Agilite.Base > 200 Then PointCount = 5
                    End If

                    If Classe = 10 Then
                        If .Stats.Base.Agilite.Base < 21 Then PointCount = 1
                        If .Stats.Base.Agilite.Base > 20 Then PointCount = 2
                        If .Stats.Base.Agilite.Base > 40 Then PointCount = 3
                        If .Stats.Base.Agilite.Base > 60 Then PointCount = 4
                        If .Stats.Base.Agilite.Base > 80 Then PointCount = 5
                    End If

                    If Classe = 11 Then
                        PointCount = 3
                    End If

                    If Classe = 12 Then
                        If .Stats.Base.Agilite.Base < 51 Then PointCount = 1
                        If .Stats.Base.Agilite.Base > 50 Then PointCount = 2
                        If .Stats.Base.Agilite.Base > 200 Then PointCount = 3
                    End If

                    If .CharactPoint >= PointCount Then
                        .Stats.Base.Agilite.Base += 1 * Utils.Config.GetItem("STATS_RATE")
                        .CharactPoint -= PointCount
                        Client.Character.SendAccountStats()
                    Else
                        Client.Send("ABE")
                    End If

                End If

            End With

        End Sub

        Private Sub MoveSpell(ByVal ExtraData As String)

            Client.Send("BN")

            Dim Data() As String = ExtraData.Split("|")
            If Data.Length <> 2 Then Exit Sub

            Dim SpellID As Integer = Data(0)
            Dim SpellPosition As Integer = Data(1)

            Client.Character.Spells.ChangePosition(SpellID, SpellPosition)

        End Sub

        Private Sub ChangeCanal(ByVal Canal As String)
            Client.Send(Canal)
        End Sub

        Private Sub ParseMessage(ByVal ExtraData As String)

            Dim Data() As String = ExtraData.Split("|".ToCharArray, 2)
            Dim Canal As String = Utils.Encoding.AsciiDecoder(Data(0))
            Dim Message As String = Data(1)

            If Message.Contains("<") Or Message.Contains(">") Then
                Exit Sub
            End If

            For Each Word As String In Config.Censured
                If Message.ToLower.Contains(Word.ToLower) Then
                    Client.Send("BN")
                    Exit Sub
                End If
            Next

            If Message.StartsWith(".") Then
                If World.Commands.Parse(Client, Message.Substring(1).Replace("|", String.Empty)) Then Exit Sub
            End If

            Client.Character.SendWatch("Chat", "vers " & Canal & " : " & Message.Split("|")(0))

            Select Case Canal

                Case "*"
                    If Client.Character.State.InBattle Then
                        Chat.SendBattleMessage(Client, Message)
                    Else
                        Chat.SendMapMessage(Client, Message)
                    End If

                Case "$"
                    Chat.SendPartyMessage(Client, Message)

                Case "%"
                    Chat.SendGuildMessage(Client, Message)

                Case "#"
                    If Client.Character.State.InBattle Then
                        Chat.SendTeamMessage(Client, Message)
                    Else
                        Client.Send("BN")
                    End If

                Case "?"
                    Chat.SendRecruitmentMessage(Client, Message)

                Case "!"
                    Chat.SendSerianeMessage(Client, Message)

                Case ":"
                    Chat.SendTradeMessage(Client, Message)

                Case "@"
                    Chat.SendAdminMessage(Client, Message)

                Case "¤"

                Case Else
                    If Canal.Length > 1 Then
                        Chat.SendPrivateMessage(Client, Canal, Message)
                    End If

            End Select

        End Sub

        Private Sub InviteParty(ByVal CharacterName As String)

            Dim Invited As GameClient = World.Players.GetCharacter(CharacterName)
            If Not Invited Is Nothing Then
                If (Not Invited.Character.State.Occuped) And (Not Invited.Character.State.InParty) Then
                    If (Not Client.Character.State.InParty) OrElse (Client.Character.State.GetParty.CharacterList.Count < 8) Then
                        Client.Character.State.PartyInvite = Invited.Character.ID
                        Invited.Character.State.PartyInvited = Client.Character.ID
                        Client.Send("PIK" & Client.Character.Name & "|" & Invited.Character.Name)
                        Invited.Send("PIK" & Client.Character.Name & "|" & Invited.Character.Name)
                    Else
                        Client.Send("PIEf" & CharacterName)
                    End If
                Else
                    Client.Send("PIEa" & CharacterName)
                End If
            Else
                Client.Send("PIEn" & CharacterName)
            End If

        End Sub

        Private Sub AcceptParty()

            If Client.Character.State.PartyInvited <> -1 Then
                Dim Invite As GameClient = World.Players.GetCharacter(Client.Character.State.PartyInvited)
                If Not Invite Is Nothing Then
                    If Invite.Character.State.PartyInvite = Client.Character.ID Then
                        Invite.Character.State.PartyInvite = -1
                        Client.Character.State.PartyInvited = -1
                        If Invite.Character.State.InParty Then
                            Invite.Character.State.GetParty.AddCharacter(Client)
                        Else
                            Dim Party As New Party(Invite, Client)
                        End If
                        Invite.Send("PR")
                    Else
                        Client.Send("BN")
                    End If
                Else
                    Client.Character.State.PartyInvited = -1
                    Client.Send("BN")
                End If
            Else
                Client.Send("BN")
            End If

        End Sub

        Private Sub DeclineParty()

            If Client.Character.State.PartyInvited <> -1 Then
                Dim Invite As GameClient = World.Players.GetCharacter(Client.Character.State.PartyInvited)
                If Not Invite Is Nothing Then
                    Invite.Character.State.PartyInvite = -1
                    Client.Character.State.PartyInvited = -1
                    Invite.Send("PR")
                Else
                    Client.Character.State.PartyInvited = -1
                    Client.Send("BN")
                End If
            Else
                Client.Send("BN")
            End If

        End Sub

        Private Sub LeaveParty(ByVal Id As Integer)

            If Client.Character.State.InParty Then
                If Id <> -1 Then
                    If Client.Character.State.GetParty.Owner = Client.Character.ID Then
                        Dim Delete As GameClient = Client.Character.State.GetParty.GetCharacter(Id)
                        If Not Delete Is Nothing Then
                            Client.Character.State.GetParty.DelCharacter(Delete, Client.Character.ID)
                        Else
                            Client.Send("BN")
                        End If
                    Else
                        Client.Send("BN")
                    End If
                Else
                    Client.Character.State.GetParty.DelCharacter(Client)
                End If
            Else
                Client.Send("BN")
            End If

        End Sub

        Private Sub ScreenInfos(ByVal ExtraData As String)

            Dim Infos() As String = ExtraData.Split(";")
            Dim Width As Integer = Infos(0)
            Dim Height As Integer = Infos(1)
            Dim Mode As String = If(Infos(2) = "1", "Fullscreen", "Normal")

        End Sub

        Private Sub ExchangeRequest(ByVal ExtraData As String)

            If Client.Character.State.Occuped OrElse Not Client.Character.CheckRegister Then Exit Sub

            Dim Data() As String = ExtraData.Split("|")

            Select Case Data(0)

                Case 0 ' NPC

                    Dim ExchangeNPC As Npc = Client.Character.GetMap.GetNpc(Data(1))
                    If ExchangeNPC IsNot Nothing Then
                        ExchangeNPC.BeginTrade(Client)
                    End If

                Case 1 ' Joueur

                    Dim ExchangePlayer As GameClient = Client.Character.GetMap.GetCharacter(Integer.Parse(Data(1)))
                    If ExchangePlayer IsNot Nothing AndAlso Not ExchangePlayer.Character.State.Occuped Then
                        Dim Exchange As New Exchange(Client, ExchangePlayer)
                    End If


                Case 4 'Marchand

                    Dim Merchant As Merchant = Client.Character.GetMap.GetMerchand(Data(1))
                    If Merchant IsNot Nothing Then
                        Merchant.BeginTrade(Client)
                    End If

                Case 6 ' Sac Marchand

                    Dim Bag As MerchantBag = Client.Character.MerchantBag
                    If Bag IsNot Nothing Then
                        Bag.BeginTrade(Client)
                    End If

            End Select

        End Sub

        Private Sub ExchangeBuy(ByVal ExtraData As String)

            Dim Data() As String = ExtraData.Split("|")
            Dim ItemID As Integer = Data(0)
            Dim Quantity As Integer = Data(1)

            If Client.Character.State.IsTrading Then

                Select Case Client.Character.State.TradeType

                    Case Trading.Npc

                        Dim ExchangeNPC As Npc = Client.Character.State.GetTraderAs(Of Npc)()
                        If ExchangeNPC IsNot Nothing Then
                            ExchangeNPC.Buy(Client, ItemID, Quantity)
                        Else
                            Client.Send("OBE")
                        End If


                    Case Trading.Merchant

                        Dim Merchant As Merchant = Client.Character.State.GetTraderAs(Of Merchant)()
                        If Merchant IsNot Nothing Then
                            Merchant.Buy(Client, ItemID, Quantity)
                        Else
                            Client.Send("OBE")
                        End If

                End Select

            End If

        End Sub

        Private Sub ExchangeSell(ByVal ExtraData As String)

            Dim Data() As String = ExtraData.Split("|")
            Dim ItemID As Integer = Data(0)
            Dim Quantity As Integer = Data(1)

            If Client.Character.State.IsTrading Then

                Select Case Client.Character.State.TradeType

                    Case Trading.Npc

                        Dim ExchangeNPC As Npc = Client.Character.State.GetTraderAs(Of Npc)()
                        If ExchangeNPC IsNot Nothing Then
                            ExchangeNPC.Sell(Client, ItemID, Quantity)
                        Else
                            Client.Send("OSE")
                        End If

                End Select

            End If

        End Sub

        Private Sub ExchangeAccept()

            Dim Exchange = Client.Character.State.GetTraderAs(Of Exchange)()
            If (Exchange IsNot Nothing) Then
                Exchange.Begin()
            Else
                Client.Send("BN")
            End If
        End Sub

        Private Sub ExchangeLeave()

            If Client.Character.State.IsTrading Then

                Select Case Client.Character.State.TradeType

                    Case Trading.Exchange
                        Dim Tradder = Client.Character.State.GetTraderAs(Of Exchange)()
                        If (Tradder IsNot Nothing) Then
                            Tradder.Leave()
                        Else
                            Client.Send("EVE")
                        End If

                    Case Trading.Npc

                        Dim Tradder = Client.Character.State.GetTraderAs(Of Npc)()
                        If (Tradder IsNot Nothing) Then
                            Tradder.EndTrade(Client)
                        Else
                            Client.Send("EVE")
                        End If

                    Case Trading.Merchant

                        Dim Tradder = Client.Character.State.GetTraderAs(Of Merchant)()
                        If (Tradder IsNot Nothing) Then
                            Tradder.EndTrade(Client)
                        Else
                            Client.Send("EVE")
                        End If

                    Case Trading.Paddock

                        Dim Tradder = Client.Character.State.GetTraderAs(Of Paddock)()
                        If (Tradder IsNot Nothing) Then
                            Tradder.EndTrade(Client)
                        Else
                            Client.Send("EVE")
                        End If

                    Case Trading.MerchantBag

                        Dim Tradder = Client.Character.State.GetTraderAs(Of MerchantBag)()
                        If (Tradder IsNot Nothing) Then
                            Tradder.EndTrade(Client)
                        Else
                            Client.Send("EVE")
                        End If


                    Case Trading.Storage

                        Dim Storage = Client.Character.State.GetTraderAs(Of Storage)()
                        If (Storage IsNot Nothing) Then
                            Storage.EndTrade(Client)
                        Else
                            Client.Send("EVE")
                        End If

                End Select

            Else
                Client.Send("EVE")
            End If

        End Sub

        Private Sub ExchangeMoveGold(ByVal Kamas As Long)

            Select Case Client.Character.State.TradeType

                Case Trading.Exchange

                    Dim Exchange = Client.Character.State.GetTraderAs(Of Exchange)()
                    If (Exchange IsNot Nothing) Then
                        Exchange.MoveKamas(Client, Kamas)
                    Else
                        Client.Send("EME")
                    End If

                Case Trading.Storage

                    Dim Storage = Client.Character.State.GetTraderAs(Of Storage)()
                    If (Storage IsNot Nothing) Then
                        Storage.SetKamas(Client, Kamas)
                    Else
                        Client.Send("EME")
                    End If

            End Select



        End Sub

        Private Sub ExchangeMoveItem(ByVal ExtraData As String)

            If Client.Character.State.IsTrading Then

                Dim Add As Boolean = (ExtraData.Substring(0, 1) = "+")
                Dim Data() As String = ExtraData.Substring(1).Split("|")
                Dim ItemId As Integer = Data(0)
                Dim Quantity As Integer = Data(1)
                Dim Price As Integer = 0
                If Data.Length > 2 Then
                    Price = Data(2)
                End If

                Select Case Client.Character.State.TradeType

                    Case Trading.Exchange

                        Dim Exchange = Client.Character.State.GetTraderAs(Of Exchange)()
                        If (Exchange IsNot Nothing) Then
                            If Add Then
                                Exchange.AddItem(Client, ItemId, Quantity)
                            Else
                                Exchange.DelItem(Client, ItemId, Quantity)
                            End If
                        Else
                            Client.Send("EME")
                        End If

                    Case Trading.MerchantBag

                        If Add Then
                            Client.Character.MerchantBag.FromInventoryToMerchantBag(Client, ItemId, Quantity, Price)
                        Else
                            Client.Character.MerchantBag.FromMerchantBagToInventory(Client, ItemId, Quantity)
                        End If


                    Case Trading.Storage

                        Dim Storage = Client.Character.State.GetTraderAs(Of Storage)()
                        If (Storage IsNot Nothing) Then
                            If Add Then
                                Storage.AddItem(Client, ItemId, Quantity)
                            Else
                                Storage.RemoveItem(Client, ItemId, Quantity)
                            End If
                        Else
                            Client.Send("EME")
                        End If

                End Select

            Else
                Client.Send("EME")
            End If

        End Sub

        Private Sub ExchangeValidate()

            Dim Exchange = Client.Character.State.GetTraderAs(Of Exchange)()
            If (Exchange IsNot Nothing) Then
                Exchange.Validate(Client)
            Else
                Client.Send("EKK")
            End If

        End Sub

        Private Sub BattleReady(ByVal ExtraData As String)

            If Not Client.Character.State.InBattle Then Exit Sub
            If Client.Character.State.GetFight.State <> Fight.FightState.Starting Then Exit Sub

            Select Case ExtraData

                Case "0"
                    Client.Character.State.GetFighter.Ready = False
                    Client.Character.State.GetFight.Send("GR0" & Client.Character.ID)

                Case "1"
                    Client.Character.State.GetFighter.Ready = True
                    Client.Character.State.GetFight.Send("GR1" & Client.Character.ID)

                Case Else
                    Client.Send("BN")

            End Select

            Client.Character.State.GetFight.Start(False)

        End Sub

        Private Sub BattleTurn()

            If Not Client.Character.State.InBattle Then Exit Sub
            If Client.Character.State.GetFight.State <> Fight.FightState.WaitTurn Then Exit Sub
            Client.Character.State.GetFighter.TurnReady = True
            Client.Character.State.GetFight.NextPlayer(False)

        End Sub

        Private Sub BattlePass()

            If Not Client.Character.State.InBattle Then Exit Sub

            Dim fight = Client.Character.State.GetFight
            If fight.State <> fight.FightState.Playing Then Exit Sub
            If Not (Client.Character.ID = Client.Character.State.GetFight.ActualId) Then Exit Sub

            If fight.Type = fight.FightType.PvM Then
                For Each Challenge In fight.Challenges
                    Challenge.EndTurn(Client.Character.State.GetFighter)
                Next
            End If

            Client.Character.State.GetFight.FinishTurn()

        End Sub

        Private Sub BattlePosition(ByVal Cell As Integer)

            If Not Client.Character.State.InBattle Then Exit Sub
            If Client.Character.State.GetFight.State <> Fight.FightState.Starting Then Exit Sub

            Client.Character.State.GetFight.PlacePlayer(Client.Character.State.GetFighter, Cell)

        End Sub

        Private Sub UseSmiley(ByVal ExtraData As String)

            If Client.Character.State.InBattle Then
                Client.Character.State.GetFight.Send("cS" & Client.Character.ID & "|" & ExtraData)
            Else
                Client.Character.GetMap.Send("cS" & Client.Character.ID & "|" & ExtraData)
            End If

        End Sub

        Private Sub WhoisPlayer(ByVal CharName As String)
            Dim WhoisClient As GameClient = World.Players.GetCharacter(CharName)
            If Not WhoisClient Is Nothing Then
                Client.Send("BWK" & WhoisClient.AccountName & "|1|" & WhoisClient.Character.Name & "|-1")
            Else
                Client.Send("BWE" & CharName)
            End If
        End Sub

        Private Sub AdminCommand(ByVal ExtraData As String)

            Dim Data() As String = ExtraData.Split(" ".ToCharArray, 2)

            Dim Command As String = Data(0).ToLower
            Dim Parameters() As String = {}
            If Data.Length = 2 Then Parameters = Data(1).Split(" ")
            Dim Infos As String = ""
            If Data.Length = 2 Then Infos = Data(1)

            Admin.Execute(Client, Command, Parameters, Infos)

        End Sub

        Private Sub ChangeFightBlocks(ByVal Type As String)

            If Not Client.Character.State.InBattle Then Exit Sub
            If Client.Character.State.GetFight.State <> Fight.FightState.Starting AndAlso Type <> "S" Then Exit Sub
            If Not Client.Character.State.GetFighter.Starter Then Exit Sub

            Dim iFight As Fight = Client.Character.State.GetFight
            Dim Team As Integer = Client.Character.State.GetFighter.Team
            Dim Blocks As FightBlocks = iFight.Blocks(Team)

            Select Case Type

                Case "N"
                    If Blocks.Basic Then
                        Blocks.Basic = False
                        iFight.Send("Im096", Team)
                        iFight.Map.Send("Go-A" & iFight.Fighters(Team).Id)
                    Else
                        Blocks.Basic = True
                        iFight.Send("Im095", Team)
                        iFight.Map.Send("Go+A" & iFight.Fighters(Team).Id)
                    End If

                Case "H"
                    If Blocks.Help Then
                        Blocks.Help = False
                        iFight.Send("Im0104", Team)
                        iFight.Map.Send("Go-H" & iFight.Fighters(Team).Id)
                    Else
                        Blocks.Help = True
                        iFight.Send("Im0103", Team)
                        iFight.Map.Send("Go+H" & iFight.Fighters(Team).Id)
                    End If

                Case "P"
                    If Blocks.Party Then
                        Blocks.Party = False
                        iFight.Send("Im094", Team)
                        iFight.Map.Send("Go-P" & iFight.Fighters(Team).Id)
                    Else
                        Blocks.Party = True
                        iFight.Send("Im093", Team)
                        iFight.Map.Send("Go+P" & iFight.Fighters(Team).Id)
                    End If

                Case "S"
                    If Client.Character.State.GetFight.State = Fight.FightState.Starting Then
                        If Blocks.Spectator Then
                            Blocks.Spectator = False
                            iFight.Send("Im040", Team)
                            iFight.Map.Send("Go-S" & iFight.Fighters(Team).Id)
                        Else
                            Blocks.Spectator = True
                            iFight.Send("Im039", Team)
                            iFight.Map.Send("Go+S" & iFight.Fighters(Team).Id)
                        End If
                    End If

            End Select

        End Sub

        Private Sub BattleQuit(ByVal Player As Integer)

            If Not Client.Character.State.InBattle Then Exit Sub
            Dim Fight As Fight = Client.Character.State.GetFight

            If Player = -1 Then

                Fight.RemovePlayer(Client.Character.State.GetFighter)
                Client.Send("GV")

            ElseIf Client.Character.State.GetFighter.Starter Then

                Dim Kicked As Fighter = Fight.GetFighter(Player)
                If Not Kicked Is Nothing AndAlso Client.Character.State.GetFighter.Team = Kicked.Team Then
                    Fight.RemovePlayer(Kicked)
                    Kicked.Send("GV")
                End If

            End If

        End Sub

        Private Sub EmoteDirection(ByVal Direction As Integer)

            If Direction >= 0 And Direction <= 7 Then
                If Client.Spam.LastEmote < Environment.TickCount Then
                    Client.Spam.LastEmote = Environment.TickCount + 500
                    Client.Character.GetMap.Send("eD" & Client.Character.ID & "|" & Direction)
                End If
            End If

        End Sub

        Private Sub EmoteUse(ByVal EmoteId As Integer)

            If Client.Spam.LastEmote < Environment.TickCount Then
                Client.Spam.LastEmote = Environment.TickCount + 500

                Client.Character.PlayEmote(EmoteId)

            End If

        End Sub

        Private Sub DialogCreate(ByVal NpcId As Integer)

            If Not Client.Character.State.Occuped AndAlso Client.Character.GetMap.NpcExist(NpcId) AndAlso Client.Character.CheckRegister Then

                Dim DialogNPC As Npc = Client.Character.GetMap.GetNpc(NpcId)
                NpcDialog.Launch(DialogNPC.TemplateID, Client)

            End If

        End Sub


        Private Sub AlignmentSendLost()
            If Client.Character.Player.Alignment.Id <> 0 Then
                Client.Send("GIP" & Client.Character.Player.Alignment.ExpLost)
            End If
        End Sub

        Private Sub AlignmentEnable()
            If Client.Character.Player.Alignment.Id <> 0 Then
                Client.Character.Player.Alignment.Enable(Client)
            End If
        End Sub

        Private Sub AlignmentDisable()
            If Client.Character.Player.Alignment.Id <> 0 Then
                Client.Character.Player.Alignment.Disable(Client)
            End If
        End Sub

        Private Sub ToggleAway()

            Client.Character.State.IsAway = Not Client.Character.State.IsAway

            Client.Send("Im0" & If(Client.Character.State.IsAway, "37", "38"))

        End Sub

        Private Sub ToggleInvisible()

            Client.Character.State.IsAway = Not Client.Character.State.IsAway

            Client.Send("Im0" & If(Client.Character.State.IsAway, "50", "51"))

        End Sub

        Private Sub ParseBasic(ByVal Packet As String)

            Select Case Packet(0)

                Case "A"

                    Select Case Packet(1)

                        Case "B"
                            AccountBoost(Packet.Substring(2))

                        Case Else
                            UnknownPacket(Packet)

                    End Select

                Case "B"

                    Select Case Packet(1)

                        Case "A"
                            AdminCommand(Packet.Substring(2))

                        Case "D"
                            SendDate()

                        Case "S"
                            UseSmiley(Packet.Substring(2))

                        Case "M"
                            ParseMessage(Packet.Substring(2))

                        Case "W"
                            WhoisPlayer(Packet.Substring(2))

                        Case "p"

                        Case "Y"

                            Select Case Packet(2)

                                Case "A"
                                    ToggleAway()

                                Case "I"
                                    ToggleInvisible()

                                Case Else
                                    UnknownPacket(Packet)

                            End Select

                        Case Else
                            UnknownPacket(Packet)

                    End Select

                Case "G"

                    Select Case Packet(1)

                        Case "A"
                            ParseAction(Packet.Substring(2, 3), Packet.Substring(5))

                        Case "I"
                            SendGameInformations()

                        Case "F"
                            Client.Character.Free()

                        Case "K"
                            EndAction(Packet.Substring(2, 1), Packet.Substring(3))

                        Case "R"
                            BattleReady(Packet.Substring(2))

                        Case "T"
                            BattleTurn()

                        Case "t"
                            BattlePass()

                        Case "Q"
                            BattleQuit(If(Packet.Length > 2, CInt(Packet.Substring(2)), -1))

                        Case "p"
                            BattlePosition(Packet.Substring(2))

                        Case "P"

                            Select Case Packet(2)

                                Case "*"
                                    AlignmentSendLost()

                                Case "-"
                                    AlignmentDisable()

                                Case "+"
                                    AlignmentEnable()

                            End Select

                        Case "f"
                            If Client.Character.State.GetFight IsNot Nothing Then
                                Client.Character.State.GetFight.Send("Gf" & Client.Character.ID & "|" & Packet.Substring(2))
                            End If

                        Case "d"

                            Select Case Packet(2)

                                Case "i"
                                    If Client.Character.State.GetFight IsNot Nothing Then
                                        Dim chall = Client.Character.State.GetFight.Challenges.FirstOrDefault(Function(c) c.Id = Packet.Substring(3))
                                        Dim target = Client.Character.State.GetFight.Fighters.FirstOrDefault(Function(f) Not f.Dead AndAlso f.Id = chall.TargetId)
                                        Client.Send("Gf" & chall.TargetId & "|" & target.Cell)
                                    End If

                            End Select

                        Case Else
                            UnknownPacket(Packet)

                    End Select

                Case "g"
                    Select Case Packet(1)

                        Case "V"
                            Client.Send("gV" & Client.Character.Name)

                        Case "C"
                            World.GuildHandler.CreateGuild(Packet, Client)

                        Case "K"
                            World.GuildHandler.OnLeave(Client, Packet)

                        Case "P"
                            World.GuildHandler.OnChangeRank(Client, Packet)

                        Case "H"
                            PerceptorManager.OnDeposit(Client)

                        Case "I"
                            Select Case Packet(2)

                                Case "B"
                                    World.GuildHandler.OnSendInfoPerco(Client)

                                Case "G"
                                    Client.Send(World.GuildHandler.OnSendInfos(Client))

                                Case "M"
                                    Client.Send(GuildHandler.OnSendPlayer(Client))

                            End Select

                        Case "J"
                            Select Case Packet(2)

                                Case "R"
                                    GuildHandler.OnInvite(Client, Packet)

                                Case "K"
                                    GuildHandler.OnAcceptInvitation(Client, Packet)

                                Case "E"
                                    GuildHandler.OnRefuse(Client, Packet)

                            End Select

                    End Select

                Case "S"

                    Select Case Packet(1)

                        Case "B"
                            UpgradeSpell(Packet.Substring(2))

                        Case "M"
                            MoveSpell(Packet.Substring(2))

                        Case Else
                            UnknownPacket(Packet)

                    End Select

                Case "D"

                    Select Case Packet(1)

                        Case "C"
                            DialogCreate(Packet.Substring(2))

                        Case "R"
                            NpcDialog.Reply(Packet, Client)

                        Case "V"
                            Client.Send("DV")

                        Case Else
                            UnknownPacket(Packet)

                    End Select

                Case "E"

                    Select Case Packet(1)


                        Case "A"
                            ExchangeAccept()

                        Case "B"
                            ExchangeBuy(Packet.Substring(2))

                        Case "R"
                            ExchangeRequest(Packet.Substring(2))

                        Case "S"
                            ExchangeSell(Packet.Substring(2))

                        Case "r"

                            Dim Paddock As Paddock = Client.Character.State.GetTraderAs(Of Paddock)()
                            If (Paddock Is Nothing) Then
                                Return
                            End If

                            Select Case Packet(2)

                                Case "C"
                                    Paddock.MountFromCertificateToShed(Client, Packet.Substring(3))

                                Case "c"
                                    Paddock.MountFromShedToCertificate(Client, Packet.Substring(3))

                                Case "g"
                                    Paddock.MountFromShedToInventory(Client, Packet.Substring(3))

                                Case "p"
                                    Paddock.MountFromInventoryToShed(Client)

                                Case "f"
                                    Paddock.GetMountFromShed(Client, Packet.Substring(3)).Kill(Client)

                                Case Else
                                    UnknownPacket(Packet)

                            End Select

                        Case "f"

                            Dim Paddock As Paddock = Client.Character.State.GetTraderAs(Of Paddock)()
                            If (Paddock Is Nothing) Then
                                Return
                            End If

                            Select Case Packet(2)

                                Case "p"
                                    Paddock.MountFromShedToMountPark(Client, Packet.Substring(3))

                                Case "g"
                                    Paddock.MountFromMountParkToShed(Client, Packet.Substring(3))

                                Case "f"
                                    Paddock.GetMountFromMountPark(Packet.Substring(3)).Kill(Client)

                                Case Else
                                    UnknownPacket(Packet)

                            End Select

                        Case "q"
                            If Not Client.Character.CheckRegister Then Exit Sub
                            Client.Character.BeMerchantRequest()

                        Case "Q"
                            If Not Client.Character.CheckRegister Then Exit Sub
                            Character.BeMerchant(Client)

                        Case "H" ' BigStore

                            Select Case Packet(3)

                                Case "T" 'type (Id)

                                Case "l" 'ItemLits (UnicId)

                                Case "B" 'Buy (Id|quant|price)

                                Case "S" 'Buy (type|unicId)

                                Case "P" 'Middle price (itemId)

                            End Select
                        Case "M"
                            Select Case Packet(2)

                                Case "G"
                                    If Client.Character.State.IsInBank = False Then 'Si ce n'est pas une banque (et que c'est une poubelle par exemple)
                                        ExchangeMoveGold(Packet.Substring(3))

                                    ElseIf Client.Character.State.IsInBank = True Then 'Si c'est une banque

                                        If Packet.Contains("-") Then 'Si le packet contient "-" (donc si on met l'argent sur le joueur et qu'on le retire de la banque
                                            BankParser.SuprKamasInBanque(Client, Packet.Substring(3)) 'On lance la suppression de kamas (de la banque)
                                        Else 'Si le packet ne contient pas "-" (donc si le joueur mets de l'argent en banque
                                            BankParser.AddKamasInBanque(Client, Packet.Substring(3)) 'On lance l'ajout de kamas (de la banque)
                                        End If
                                    End If

                                Case "O"
                                    If Client.Character.State.IsInBank = False Then
                                        ExchangeMoveItem(Packet.Substring(3))

                                    ElseIf Client.Character.State.IsInBank = True Then
                                        BankParser.ExchangeMoveItem(Packet.Substring(3), Client)
                                    End If

                                Case Else
                                    UnknownPacket(Packet)

                            End Select
                        Case "M"
                            Select Case Packet(2)

                                Case "G"
                                    ExchangeMoveGold(Packet.Substring(3))

                                Case "O"
                                    ExchangeMoveItem(Packet.Substring(3))

                                Case Else
                                    UnknownPacket(Packet)

                            End Select

                        Case "K"
                            ExchangeValidate()

                        Case "V"
                            ExchangeLeave()

                        Case Else
                            UnknownPacket(Packet)

                    End Select

                Case "c"

                    Select Case Packet(1)

                        Case "C"
                            ChangeCanal(Packet)

                        Case Else
                            UnknownPacket(Packet)

                    End Select

                Case "O"

                    Select Case Packet(1)

                        Case "M"
                            MoveItem(Packet.Substring(2))

                        Case "U"
                            UseItem(Packet.Substring(2))

                        Case "d"
                            DeleteItem(Packet.Substring(2))

                        Case "s"
                            Dim params As String() = Packet.Substring(2).Split("|")
                            Client.Character.Items.SetLivingSkin(Client, params(0), params(1), params(2))

                        Case "f"
                            Dim params As String() = Packet.Substring(2).Split("|")
                            Client.Character.Items.FeedLiving(Client, params(0), params(1), params(2))

                        Case "x"
                            Dim params As String() = Packet.Substring(2).Split("|")
                            Client.Character.Items.Dissociate(Client, params(0), params(1))

                        Case Else
                            UnknownPacket(Packet)

                    End Select

                Case "P"

                    Select Case Packet(1)

                        Case "I"
                            InviteParty(Packet.Substring(2))

                        Case "A"
                            AcceptParty()

                        Case "R"
                            DeclineParty()

                        Case "V"
                            LeaveParty(If(Packet.Length > 2, CInt(Packet.Substring(2)), -1))

                        Case Else
                            UnknownPacket(Packet)

                    End Select

                Case "I"

                    Select Case Packet(1)

                        Case "r"
                            ScreenInfos(Packet.Substring(2))

                        Case Else
                            UnknownPacket(Packet)

                    End Select


                Case "Q"

                    Select Case Packet(1)

                        Case "L"
                            Client.Send("QL")

                        Case Else
                            UnknownPacket(Packet)

                    End Select

                Case "e"

                    Select Case Packet(1)

                        Case "D"
                            EmoteDirection(Packet.Substring(2))

                        Case "U"
                            EmoteUse(Packet.Substring(2))

                        Case Else
                            UnknownPacket(Packet)

                    End Select

                Case "R"

                    Select Case Packet(1)

                        Case "r"
                            If Client.Character.Mount IsNot Nothing Then
                                Client.Character.Mount.Ride(Client)
                            Else
                                Client.Send("BN")
                            End If

                        Case "f"
                            Client.Character.Mount.Kill(Client)

                        Case "x"
                            Client.Character.Mount.SetPercent(Client, Packet.Substring(2))

                        Case "c"
                            Client.Character.Mount.Castre(Client)

                        Case "n"
                            Client.Character.Mount.SetName(Client, Packet.Substring(2))

                        Case "b"
                            'buy paddock

                        Case "p"
                            Client.Character.GetMap.GetMount(Packet.Substring(2)).GetInfoInterface(Client)

                        Case "d"
                            Dim ItemId As Integer = Packet.Substring(2).Split("|")(0)
                            Dim Item As Item = Item.GetItemById(ItemId)
                            If Item IsNot Nothing Then
                                Dim m As New Mount(Item.Args)
                                m.GetInfoInterface(Client)
                            Else
                                Client.Send("BN")
                                Exit Sub
                            End If

                        Case "o"
                            'remove objet

                        Case "s"
                            'sell(Paddock)

                        Case "v"
                            'Leave paddock ?

                    End Select


                Case "f"

                    Select Case Packet(1)

                        Case "N", "H", "S", "P"
                            ChangeFightBlocks(Packet(1))

                    End Select

                Case "p"
                    Client.Send("pong")

                Case "q"
                    Client.Send("qpong")

                Case "W"
                    Select Case Packet(1)

                        Case "p"
                            Dim Prism As PrismTemplate = PrismHandler.GetTemplate(Client.Character.GetMap.Id)
                            PrismHandler.Teleport(Client, Packet.Substring(2), Prism)

                        Case "w"
                            PrismHandler.StopUse(Client)

                        Case "U"
                            Dim Zaap As Interactives.Zaap = Client.Character.State.GetTraderAs(Of Interactives.Zaap)()
                            If Zaap IsNot Nothing Then
                                Zaap.Teleport(Client, Packet.Substring(2))
                            End If

                        Case "V"
                            Dim Zaap As Interactives.Zaap = Client.Character.State.GetTraderAs(Of Interactives.Zaap)()
                            If Zaap IsNot Nothing Then
                                Zaap.StopUse(Client)
                            End If

                        Case "u"
                            Dim Zaapis As Interactives.Zaapis = Client.Character.State.GetTraderAs(Of Interactives.Zaapis)()
                            If Zaapis IsNot Nothing Then
                                Zaapis.Teleport(Client, Packet.Substring(2))
                            End If
                            ' ZaapisManager.OnMove(ZaapisManager._Get(Packet.Substring(2)), Client)

                        Case "v"
                            Dim Zaapis As Interactives.Zaapis = Client.Character.State.GetTraderAs(Of Interactives.Zaapis)()
                            If Zaapis IsNot Nothing Then
                                Zaapis.StopUse(Client)
                            End If

                        Case "w"
                            '  Client.Send("Ww")
                    End Select

                Case "F"
                    Select Case Packet(1)

                        Case "L"
                            Client.Send("FL")
                            AmiManager.SendFriendsList(Client)
                            '
                            'If Client.Character.Married Then
                            'If Not Players.GetCharacter(Client.Character.Partenaire.Name) Is Nothing Then
                            'Client.Send("FS" & Client.Character.Partenaire.Name & "|" & Client.Character.Partenaire.Classe & Client.Character.Partenaire.Sexe & "|" & Client.Character.Partenaire.Color(0) & "|" & Client.Character.Partenaire.Color(1) & "|" & Client.Character.Partenaire.Color(2) & "|" & Client.Character.Partenaire.MapId & "|" & Client.Character.Partenaire.Player.Level & "|")
                            ' Else
                            '  Client.Send("FS" & Client.Character.Partenaire.Name & "|" & Client.Character.Partenaire.Classe & Client.Character.Partenaire.Sexe & "|" & Client.Character.Partenaire.Color(0) & "|" & Client.Character.Partenaire.Color(1) & "|" & Client.Character.Partenaire.Color(2) & "||" & Client.Character.Partenaire.Player.Level & "|")
                            ''  End If
                            ' End If
                        Case "JS"
                            ' If Client.Character.Married Then
                            ' Dim partenaire As GameClient = Players.GetCharacter(Client.Character.Partenaire.ID)

                            'If Not partenaire Is Nothing Then
                            'If Not partenaire.Character.State.Occuped Then
                            'Client.Character.TeleportTo(partenaire.Character.MapId, partenaire.Character.MapCell)
                            'Client.SendNormalMessage("Vous etes téléporter vers votre partenaire.")
                            ' Else
                            ' Client.SendNormalMessage("Votre partenaire est occupé.")
                            '' End If
                            ' Else
                            ' Client.SendNormalMessage("Soit vous n'etes pas marrié soit votre partenaire n'est pas connecté.")
                            'End If
                            'Else
                            ' Client.SendNormalMessage("Vous n'etes pas marrié.")
                            'End If
                        Case "A"
                            AmiManager.AddFriend(Client, Packet.Substring(2))
                        Case "D"
                            AmiManager.DeleteFriend(Client, Packet.Substring(3))
                    End Select
                Case "L"
                    If Migration.VerifMigration(Client) = False Then
                        If Migration.VerifMigration2(Client) = True Then
                            Dim Infos As String = Migration.GetMigration(Client)
                            Migration.LoadCharacter(Infos.Substring(1), Client)
                        End If
                        SendCharacterList()
                    ElseIf Migration.VerifMigration(Client) = True And Not Migration.GetServeurDest(Client) = ServerId Then
                        Migration.StartMigration(Client)
                    Else
                        SendCharacterList()
                    End If

                Case Else
                    UnknownPacket(Packet)

            End Select


        End Sub

        Private Sub GameMove(ByVal Cells As String)

            If Client.Character.State.GetFight Is Nothing AndAlso Client.Character.State.Occuped AndAlso Not Client.Character.State.IsGhost Then
                Client.Send("BN")
                Exit Sub
            End If

            Dim currentCell = Client.Character.MapCell
            Dim destCell = Utils.Cells.GetCellNum(Mid(Cells, Cells.Length - 1, 2))

            Cells = Utils.Cells.RemakePath(Client, Cells, Client.Character.State.InBattle)

            If Not Utils.Cells.IsValidPath(Client, Cells) Then
                Client.Send("GA;0")
                Exit Sub
            End If

            If Client.Character.State.InBattle Then
                If Client.Character.State.GetFight.State <> Fight.FightState.Playing Then Exit Sub
                If Not (Client.Character.ID = Client.Character.State.GetFight.ActualId) Then Exit Sub

                Dim Distance As Integer = Utils.Cells.GetPathLenght(Client, Cells)
                Client.Character.State.GetFight.PlayerMove(Client.Character.State.GetFighter, Cells, Distance)
                Exit Sub
            End If

            Dim NewCell As Integer = Utils.Cells.GetCellNum(Mid(Cells, Cells.Length - 1, 2))
            Dim ActualCell As String = Utils.Cells.GetCellChars(Client.Character.MapCell)
            Dim NewDirection As Integer = Utils.Cells.GetDirNum(Mid(Cells, Cells.Length - 2, 1))

            Dim GameMovePacket As String = "GA0;1;" & Client.Character.ID & ";"
            GameMovePacket &= Utils.Cells.GetDirChar(Client.Character.MapDir) & ActualCell & Cells

            Client.Character.State.IsMoving = True
            Client.Character.GetMap.Send(GameMovePacket)

            If Client.Character.State.IsFollowed Then
                Client.Character.OnMove(NewCell)
            End If

            Client.Character.State.ExecuteWhenArrived(Sub() Client.Character.MapCell = NewCell)
            Client.Character.State.ExecuteWhenArrived(Sub() Client.Character.MapDir = NewDirection)
            Client.Character.GetMap.ActiveInteractiveObject(Client, destCell, 0)

        End Sub

        Private Sub AskChallenge(ByVal Id As Integer)

            If Client.Character.State.Occuped Then
                Exit Sub
            End If

            If Client.Character.GetMap.CharacterExist(Id) Then

                Dim Asked As GameClient = Client.Character.GetMap.GetCharacter(Id)

                If Client.Character.CheckRegisterWP Then
                    If Not Asked.Character.State.Occuped Then
                        Client.Character.State.ChallengeAsk = Asked.Character.ID
                        Asked.Character.State.ChallengeAsked = Client.Character.ID
                        Client.Character.GetMap.Send("GA;900;" & Client.Character.ID & ";" & Asked.Character.ID)
                    Else
                        Client.SendMessage("Personnage actuellement indisponible.")
                    End If
                Else
                    Client.SendMessage("Personnage non abonné")
                End If

            Else
                Client.SendMessage("Personnage introuvable sur la carte.")
            End If

        End Sub

        Private Sub AcceptChallenge(ByVal Id As Integer)

            If Client.Character.GetMap.CharacterExist(Id) Then

                Dim Asked As GameClient = Client.Character.GetMap.GetCharacter(Id)
                If Asked.Character.State.ChallengeAsk = Client.Character.ID Then
                    Asked.Character.State.ChallengeAsk = -1
                    Client.Character.State.ChallengeAsked = -1
                    Client.Send("GA;901;" & Client.Character.ID & ";" & Asked.Character.ID)
                    Asked.Send("GA;901;" & Asked.Character.ID & ";" & Client.Character.ID)

                    FightsHandler.AddFight(New Fight(Asked, Client, Fight.FightType.Challenge))

                Else
                    Client.Send("BN")
                End If

            Else
                Client.Send("BN")
            End If

        End Sub

        Private Sub DenyChallenge(ByVal Id As Integer)

            If Client.Character.GetMap.CharacterExist(Id) Then

                Dim Asked As GameClient = Client.Character.GetMap.GetCharacter(Id)
                If Not Asked Is Nothing Then
                    If Asked.Character.State.ChallengeAsk = Client.Character.ID Then
                        Asked.Character.State.ChallengeAsk = -1
                        Client.Character.State.ChallengeAsked = -1
                        Client.Send("GA;902;" & Client.Character.ID & ";" & Asked.Character.ID)
                        Asked.Send("GA;902;" & Asked.Character.ID & ";" & Client.Character.ID)
                    ElseIf Client.Character.State.ChallengeAsk > -1 Then
                        Asked = Client.Character.GetMap.GetCharacter(Client.Character.State.ChallengeAsk)
                        If Not Asked Is Nothing Then
                            Client.Character.State.ChallengeAsk = -1
                            Asked.Character.State.ChallengeAsked = -1
                            Client.Send("GA;902;" & Client.Character.ID & ";" & Asked.Character.ID)
                            Asked.Send("GA;902;" & Asked.Character.ID & ";" & Client.Character.ID)
                        End If
                    Else
                        Client.Send("BN")
                    End If
                End If

            Else
                Client.Send("BN")
            End If

        End Sub

        Private Sub JoinChallenge(ByVal ExtraData As String)

            Dim Data() As String = ExtraData.Split(";")
            Dim BattleId As Integer = Data(0)
            Dim JoinTeam As Integer = Data(1)

            If Client.Character.GetMap.FightExist(BattleId) Then

                Dim Fight As Fight = Client.Character.GetMap.GetFight(BattleId)

                If Fight.State = World.Fight.FightState.Starting Then

                    Dim Team As Integer = Fight.GetTeam(JoinTeam)

                    If (Team <> -1) Then

                        If Fight.Type = World.Fight.FightType.Agression AndAlso
                            Client.Character.Player.Alignment.Id <>
                            Fight.Fighters(Team).Client.Character.Player.Alignment.Id Then Exit Sub

                        If Fight.Type = World.Fight.FightType.Agression Then Client.Character.Player.Alignment.Enabled = True
                        Fight.AddFighter(Client, Team, False)

                    Else
                        Client.Send("BN")
                    End If

                Else
                    Client.Send("BN")
                End If

            Else
                Client.Send("BN")
            End If

        End Sub

        Private Sub LaunchSpell(ByVal ExtraData As String)

            If Not Client.Character.State.InBattle Then Exit Sub
            If Client.Character.State.GetFight.State <> Fight.FightState.Playing Then Exit Sub
            If Not (Client.Character.ID = Client.Character.State.GetFight.ActualId) Then Exit Sub

            Dim Data() As String = ExtraData.Split(";")

            If Not IsNumeric(Data(0)) OrElse Not IsNumeric(Data(1)) Then Exit Sub

            Dim Spell As Integer = Data(0)
            Dim Cell As Integer = Data(1)

            Client.Character.SendAccountStats()

            If Not Client.Character.Spells.HaveSpell(Spell) Then
                Exit Sub
            End If

            Dim SpellLevel As Integer = Client.Character.Spells.GetSpellLevel(Spell)

            If SpellsHandler.SpellExist(Spell) Then
                Client.Character.State.GetFight.LaunchSpell(Client.Character.State.GetFighter, SpellsHandler.GetSpell(Spell).AtLevel(SpellLevel), Cell)
            End If

        End Sub

        Private Sub UseWeapon(ByVal ExtraData As String)

            If Not Client.Character.State.InBattle Then Exit Sub
            If Client.Character.State.GetFight.State <> Fight.FightState.Playing Then Exit Sub
            If Not (Client.Character.ID = Client.Character.State.GetFight.ActualId) Then Exit Sub

            Dim Cell As Integer = ExtraData

            Client.Character.SendAccountStats()

            Dim Player As Fighter = Client.Character.State.GetFighter

            If Player.State.Has(FighterState.State.Affaibli) Then
                Client.Send("BN")
                Exit Sub
            End If

            Player.State.LostInvisible(Player)

            If Not Client.Character.Items.IsObjectOnPos(ItemsHandler.Positions.ARME) Then

                If Player.PA >= 4 AndAlso _
                  Cells.GoalDistance(Player.Fight.Map, _
                                           Player.Cell, _
                                           Cell) = 1 Then

                    Dim SimpleEffect As New SpellEffect
                    SimpleEffect.Effect = Effect.DamageNeutre
                    SimpleEffect.Value = 1
                    SimpleEffect.Value2 = 5
                    SimpleEffect.Tours = 0
                    SimpleEffect.Chance = 0

                    Player.PA -= 4

                    Dim ListOfCible As New List(Of Fighter)
                    Dim Cible As Fighter = Player.Fight.GetFighterFromCell(Cell)
                    If Not Cible Is Nothing Then ListOfCible.Add(Cible)

                    Player.Fight.Send("GA;303;" & Player.Id & ";" & Cell)
                    SimpleEffect.UseEffect(Player, ListOfCible, Cell, False)

                    Player.Fight.Send("GA;102;" & Player.Id & ";" & Player.Id & ",-4")

                    Player.Fight.VerifyPlayers()

                    If Player.Fight.OnlyOneTeam() Then

                        For Each Challenge In Player.Fight.Challenges
                            Challenge.EndTurn(Player)
                        Next

                        Player.Fight.EndFight()
                    End If

                End If
                Exit Sub

            End If

            Dim Weapon = Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.ARME)
            Client.Character.State.GetFight.UseWeapon(Player, Weapon, Cell)

            If Player.Fight.Type = Fight.FightType.PvM Then
                For Each Challenge In Player.Fight.Challenges
                    Challenge.CheckWeapon(Player, Weapon)
                Next
            End If

        End Sub

        Private Sub LaunchAggression(ByVal Id As Integer)

            If Client.Character.GetMap.CharacterExist(Id) Then

                Dim Target As GameClient = Client.Character.GetMap.GetCharacter(Id)
                If Target.Character.Player.Alignment.Id <> 0 AndAlso Target.Character.Player.Alignment.Id <>
                    Client.Character.Player.Alignment.Id Then

                    If Not Target.Character.State.Occuped Then
                        If Target.Character.CheckRegisterWP Then
                            If Target.Ip <> Client.Ip Then

                                Client.Character.Player.Alignment.Enabled = True
                                Client.Character.GetMap.Send("GA;906;" & Client.Character.ID & ";" & Target.Character.ID)
                                FightsHandler.AddFight(New Fight(Client, Target, Fight.FightType.Agression))

                            Else
                                Client.SendMessage("Impossible d'agresser ce personnage !")
                            End If
                        Else
                            Client.SendMessage("Personnage non abonné")
                        End If
                    End If

                End If

            End If

        End Sub

        Private Sub ParseUseRessource(ByVal Args As String)
            Dim GameAction() As String = Args.Split(";")

            Client.Character.GetMap.ActiveInteractiveObject(Client, GameAction(0), GameAction(1))
        End Sub

        Private Sub ParseUseSkill(ByVal Args As String)
            'Skill

        End Sub

        Private Sub ParseAction(ByVal Action As Integer, ByVal Args As String)

            Select Case Action

                Case 1
                    GameMove(Args)

                Case 300
                    LaunchSpell(Args)

                Case 303
                    UseWeapon(Args)

                Case 500
                    If Not Client.Character.CheckRegister() Then Exit Sub
                    ParseUseRessource(Args)

                Case 507
                    If Not Client.Character.CheckRegister() Then Exit Sub
                    ParseUseSkill(Args)

                Case 512
                    If Not Client.Character.CheckRegister() Then Exit Sub
                    PrismManager.GetPanel(Client, PrismManager._GetByMapID(Client.Character.MapId))

                Case 900
                    If Not Client.Character.CheckRegister() Then Exit Sub
                    AskChallenge(Args)

                Case 901
                    AcceptChallenge(Args)

                Case 902
                    DenyChallenge(Args)

                Case 903
                    If Not Client.Character.CheckRegister() Then Exit Sub
                    JoinChallenge(Args)

                Case 906
                    If Not Client.Character.CheckRegister() Then Exit Sub
                    LaunchAggression(Args)

            End Select

        End Sub

        Private Sub EndAction(ByVal ActionType As String, ByVal Args As String)

            Select Case ActionType

                Case "K"
                    If Client.Character.State.IsMoving Then

                        Client.Send("BN")
                        Client.Character.State.IsMoving = False

                        If Not Client.Character.State.IsGhost Then
                            Dim M As MonsterGroup = Client.Character.GetMap.GetMonsterOnCell(Client.Character.State.OriginalCell)
                            If Not M Is Nothing AndAlso Client.Character.CheckRegister Then
                                FightsHandler.AddFight(New Fight(Client, M, World.Fight.FightType.PvM))
                                Exit Sub
                            End If
                        End If

                        For Each Trigger As MapTrigger In Client.Character.GetMap.MapTriggers
                            If Trigger.Cell = Client.Character.MapCell Then
                                World.Players.LoadMap(Client, Trigger.DestMap, Trigger.DestCell, True)
                                Client.Character.OnMove(Trigger.DestCell)
                                Exit Sub
                            End If
                        Next

                    ElseIf Client.Character.State.InBattle AndAlso Client.Character.State.GetFighter.LastMove > 0 Then
                        Dim Id As Integer = Client.Character.ID
                        Client.Character.State.GetFight.Send("GA;129;" & Id & ";" & Id & ",-" & Client.Character.State.GetFighter.LastMove)
                        Client.Character.State.GetFighter.LastMove = -1
                        If Client.Character.State.GetFight.WalkOnTrap(Client.Character.State.GetFighter.Cell) Then
                            Client.Character.State.GetFight.LaunchTrapEffects(Client.Character.State.GetFighter, Client.Character.State.GetFighter.Cell)
                        End If
                        Client.Send("GAF2|" & Id)
                    ElseIf Client.Character.State.InBattle Then
                        Client.Send("BN")
                    Else
                        Client.Send("BN")
                    End If

                Case "E"

                    Dim NewCell As Integer = Args.Split("|")(1)
                    Client.Character.State.IsMoving = False
                    Client.Character.MapCell = NewCell

            End Select

        End Sub

    End Class
End Namespace