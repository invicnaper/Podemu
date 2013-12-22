Imports MySql.Data.MySqlClient
Imports Vemu_gs.Utils
Imports Vemu_gs.Utils.Basic

Namespace Game
    Public Class Character

        Public Client As GameClient
        Friend SkinStatic As Boolean


        Public Sub SetClient(ByVal Client As GameClient)
            Me.Client = Client
        End Sub

        Public ID As Integer
        Public Name As String
        Public Classe, Sexe, Skin, Size, Color(2) As Integer

        Public GameType As Byte

        Public Player As New StatsPlayer()
        Public Restriction As New CharacterRestriction(Me)
        Public MerchantBag As New MerchantBag(Me)
        Public Items As New CharacterItems(Me)
        Public State As New CharacterState(Me)
        Public Spells As New CharacterSpells
        Public Emotes As New CharacterEmotes(Me)
        Public Guild As World.Guild
        Public PlayersGuild As World.PlayerGuild
        Public Mount As World.Mount
        Public Mounts As New List(Of World.Mount)
        Public OnBandit As Boolean = False
        Public Bandit As Bandit

        Public IsDead As Boolean = False
        Public DeathCount As Byte
        Public niveauMax As Short

        Public Married As Boolean = False
        Public Partenaire As Character
        Public IDPartenaire As String = ""

        Public MobsSuiveurs As New CharacterMobsSuiveurs

        Public StringQuest As String = ""
        Public Quests As New CharacterQuests

        Public Zaaps As New List(Of Integer)
        Public SaveMap As Integer
        Public SaveCell As Integer

        Public IsFollowed As Boolean = False

        Public GuildID As Integer = 0
        Public Rank As Integer = 0

        Public MapId As Integer
        Public MapCell As Short
        Public MapDir As Byte

        Public Channels As String = "*#$pi:?!"
        Public Rights As String = "6bk"

        Public Delegate Sub Move(ByVal Character As Character, ByVal Cell As Integer)
        Public Event OnMoved As Move

        Private Function CheckExp() As Boolean

            If Player.Level >= ExperienceTable.MaxLevel Then Return False

            If Player.Exp >= ExperienceTable.AtLevel(Player.Level + 1).Character Then

                Player.Level += 1
                Player.SpellPoint += 1
                Player.CharactPoint += 5
                Player.Life += 5

                Return True

            End If

            Return False

        End Function

        Public Sub LevelUp()

            If Player.Level >= ExperienceTable.MaxLevel Then Exit Sub

            If Player.Exp >= ExperienceTable.AtLevel(Player.Level + 1).Character Then

                Do While CheckExp() : Loop

                SpellsHandler.LearnSpells(Client.Character)
                SendAccountStats()
                Client.Send("AN" & Player.Level)

            End If

        End Sub

        Public Sub Save()

            SyncLock Sql.CharactersSync

                Dim UpdateString As String = "niveau=@niveau, classe=@classe, sexe=@sexe, skin=@skin," & _
                    "color1=@color1, color2=@color2, color3=@color3, exp=@exp, kamas=@kamas, " & _
                    "charactpoint=@charactpoint, spellpoints=@spellpoints, vie=@vie, energie=@energie, " & _
                    "terre=@force, vitalite=@vitalite, sagesse=@sagesse, agilite=@agilite, chance=@chance, " & _
                    "intelligence=@intelligence, niveauMax=@niveauMax, mapid=@mapid, mapcell=@mapcell, mapdir=@mapdir, emotes=@emotes, sell_bag=@sell_bag, " & _
                    "spells=@spells, items=@items, mount=@mount, paddock=@paddock, zaaps=@zaaps, savePos=@savePos, is_merchant=@is_merchant, restrictions=@restrictions, is_dead=@is_dead, death_count=@death_count, alignment=@alignment, alignmentExp=@alignmentExp, " & _
                    "alignmentEnabled=@alignmentEnabled, guildid=@guildid, rank=@rank"

                Dim SQLText As String = "UPDATE player_characters SET " & UpdateString & " WHERE id=@Id"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)
                Dim P As MySqlParameterCollection = SQLCommand.Parameters

                P.Add(New MySqlParameter("@Id", ID))

                P.Add(New MySqlParameter("@niveau", Player.Level))
                P.Add(New MySqlParameter("@classe", Classe))
                P.Add(New MySqlParameter("@sexe", Sexe))
                P.Add(New MySqlParameter("@skin", Skin))

                P.Add(New MySqlParameter("@color1", Color(0)))
                P.Add(New MySqlParameter("@color2", Color(1)))
                P.Add(New MySqlParameter("@color3", Color(2)))

                P.Add(New MySqlParameter("@exp", Player.Exp))
                P.Add(New MySqlParameter("@kamas", Player.Kamas))
                P.Add(New MySqlParameter("@charactpoint", Player.CharactPoint))
                P.Add(New MySqlParameter("@spellpoints", Player.SpellPoint))

                P.Add(New MySqlParameter("@vie", Player.Life))
                P.Add(New MySqlParameter("@energie", Player.Energy))

                P.Add(New MySqlParameter("@force", Player.Stats.Base.Force.Base))
                P.Add(New MySqlParameter("@vitalite", Player.Stats.Base.Vitalite.Base))
                P.Add(New MySqlParameter("@sagesse", Player.Stats.Base.Sagesse.Base))
                P.Add(New MySqlParameter("@agilite", Player.Stats.Base.Agilite.Base))
                P.Add(New MySqlParameter("@chance", Player.Stats.Base.Chance.Base))
                P.Add(New MySqlParameter("@intelligence", Player.Stats.Base.Intelligence.Base))

                P.Add(New MySqlParameter("@mapid", MapId))
                P.Add(New MySqlParameter("@mapcell", MapCell))
                P.Add(New MySqlParameter("@mapdir", MapDir))

                P.Add(New MySqlParameter("@niveauMax", niveauMax))

                P.Add(New MySqlParameter("@spells", Spells.GetAllSpellSave))
                P.Add(New MySqlParameter("@items", Items.GetItemsSave))

                If (Mount Is Nothing) Then
                    P.Add(New MySqlParameter("@mount", ""))
                Else
                    P.Add(New MySqlParameter("@mount", Mount.SaveString))
                End If

                P.Add(New MySqlParameter("@paddock", String.Join("~", Mounts.Select(Function(m) m.SaveString))))

                P.Add(New MySqlParameter("@is_merchant", State.IsMerchant))
                P.Add(New MySqlParameter("@is_dead", IsDead))
                P.Add(New MySqlParameter("@death_count", DeathCount))
                P.Add(New MySqlParameter("@restrictions", Restriction.Restriction))
                P.Add(New MySqlParameter("@emotes", Emotes.EmoteCapacity))
                P.Add(New MySqlParameter("@sell_bag", MerchantBag.GetItemsSave))
                P.Add(New MySqlParameter("@zaaps", String.Join(";", Zaaps)))
                P.Add(New MySqlParameter("@savePos", SaveMap & "," & SaveCell))

                P.Add(New MySqlParameter("@alignment", Player.Alignment.Id))
                P.Add(New MySqlParameter("@alignmentExp", Player.Alignment.Exp))
                P.Add(New MySqlParameter("@alignmentEnabled", If(Player.Alignment.Enabled, 1, 0)))

                If GuildID = 0 Then
                    P.Add(New MySqlParameter("@guildid", 0))
                    P.Add(New MySqlParameter("@rank", 0))
                Else
                    P.Add(New MySqlParameter("@guildid", GuildID))
                    P.Add(New MySqlParameter("@rank", Rank))
                End If


                SQLCommand.ExecuteNonQuery()

                If Client.Infos IsNot Nothing Then Client.Infos.Save()

            End SyncLock

        End Sub

        Public Sub Create()

            SyncLock Sql.CharactersSync

                Try

                    Dim CreateString As String = "@nom, @id, @niveau,@niveauMax, @classe, @sexe, @skin," & _
                        "@color1, @color2, @color3, @exp, @kamas, " & _
                        "@charactpoint, @spellpoints, @vie, @energie, " & _
                        "@force, @vitalite, @sagesse, @agilite, @chance, " & _
                        "@intelligence, @mapid, @mapcell, @mapdir, " & _
                            "emotes=@emotes, sell_bag=@sell_bag, zaaps=@zaaps, savePos=@savePos, paddock=@paddock, is_merchant=@is_merchant, restrictions=@restrictions, is_dead=@is_dead, death_count=@death_count, " & _
                        "@spells, @items, @mount, @alignment, @alignmentExp, @alignmentEnabled, @guildid, @rank"

                    Dim SQLText As String = "INSERT INTO player_characters VALUES (" & CreateString & ")"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)
                    Dim P As MySqlParameterCollection = SQLCommand.Parameters

                    P.Add(New MySqlParameter("@nom", Name))

                    P.Add(New MySqlParameter("@id", ID))
                    P.Add(New MySqlParameter("@niveau", Player.Level))
                    P.Add(New MySqlParameter("@classe", Classe))
                    P.Add(New MySqlParameter("@sexe", Sexe))
                    P.Add(New MySqlParameter("@skin", Skin))

                    P.Add(New MySqlParameter("@color1", Color(0)))
                    P.Add(New MySqlParameter("@color2", Color(1)))
                    P.Add(New MySqlParameter("@color3", Color(2)))

                    P.Add(New MySqlParameter("@exp", Player.Exp))
                    P.Add(New MySqlParameter("@kamas", Player.Kamas))
                    P.Add(New MySqlParameter("@charactpoint", Player.CharactPoint))
                    P.Add(New MySqlParameter("@spellpoints", Player.SpellPoint))

                    P.Add(New MySqlParameter("@vie", Player.Life))
                    P.Add(New MySqlParameter("@energie", Player.Energy))

                    P.Add(New MySqlParameter("@force", Player.Stats.Base.Force.Base))
                    P.Add(New MySqlParameter("@vitalite", Player.Stats.Base.Vitalite.Base))
                    P.Add(New MySqlParameter("@sagesse", Player.Stats.Base.Sagesse.Base))
                    P.Add(New MySqlParameter("@agilite", Player.Stats.Base.Agilite.Base))
                    P.Add(New MySqlParameter("@chance", Player.Stats.Base.Chance.Base))
                    P.Add(New MySqlParameter("@intelligence", Player.Stats.Base.Intelligence.Base))

                    P.Add(New MySqlParameter("@mapid", MapId))
                    P.Add(New MySqlParameter("@mapcell", MapCell))
                    P.Add(New MySqlParameter("@mapdir", MapDir))

                    P.Add(New MySqlParameter("@spells", Spells.GetAllSpellSave))
                    P.Add(New MySqlParameter("@items", Items.GetItemsSave))

                    P.Add(New MySqlParameter("@niveauMax", niveauMax))

                    P.Add(New MySqlParameter("@mount", ""))
                    P.Add(New MySqlParameter("@paddock", ""))
                    P.Add(New MySqlParameter("@is_merchant", "0"))
                    P.Add(New MySqlParameter("@is_dead", "0"))
                    P.Add(New MySqlParameter("@death_count", "0"))
                    P.Add(New MySqlParameter("@restrictions", "79"))
                    P.Add(New MySqlParameter("@emotes", "1376255"))
                    P.Add(New MySqlParameter("@sell_bag", ""))
                    P.Add(New MySqlParameter("@zaaps", ""))
                    P.Add(New MySqlParameter("@savePos", "0,0"))

                    P.Add(New MySqlParameter("@alignment", Player.Alignment.Id))
                    P.Add(New MySqlParameter("@alignmentExp", Player.Alignment.Exp))
                    P.Add(New MySqlParameter("@alignmentEnabled", If(Player.Alignment.Enabled, 1, 0)))

                    P.Add(New MySqlParameter("@guildid", 0))
                    P.Add(New MySqlParameter("@rank", 0))

                    SQLCommand.ExecuteNonQuery()

                Catch ex As Exception
                    Utils.MyConsole.Err("Can't create character '@" & Name & "@' : " & ex.Message)
                End Try

            End SyncLock

        End Sub

        Public ReadOnly Property GetMap() As World.Map
            Get
                Return World.MapsHandler.GetMap(MapId)
            End Get
        End Property

        Private ReadOnly Property Aura() As Integer
            Get
                If Config.GetItem("ENABLE_AURA") = True Then
                    Return If(Player.Level >= 100, If(Player.Level >= 200, 2, 1), 0)
                Else
                    Return If(Player.Level >= 200, 2, 0)
                End If
            End Get
        End Property

        Public ReadOnly Property PatternOnCharChoose() As String
            Get

                Return String.Concat(
                    ID, ";", Name, ";", Player.Level, ";", Skin & ";", _
                    PatternColor(";"), _
                    Items.GetItemsIDs, ";", If(State.IsMerchant, "1", "0"), ";", ServerId, ";" & If(IsDead, "1", "0") & ";" & If(Config.GetItem(Of Boolean)("ACTIVE_HEROIC"), DeathCount.ToString(), "") & ";" & niveauMax)
            End Get
        End Property

        Public Function PatternColor(ByVal separator As Char) As String

            Return String.Concat(DeciToHex(Color(0)), separator, DeciToHex(Color(1)), separator, DeciToHex(Color(2)), separator)

        End Function

        Public ReadOnly Property PatternOnCharSelect() As String
            Get

                Return String.Concat("|", ID, "|", Name, "|", Player.Level, "|", Classe, "|", Skin, "|", _
                                     PatternColor("|"), "|", _
                                     Items.GetItemsString, "|")
            End Get
        End Property

        Private ReadOnly Property AlignmentInfos() As String
            Get
                Return String.Concat(Player.Alignment.Id, ",", Player.Alignment.Id, ",", If(Player.Alignment.Enabled, Player.Alignment.Rank, 0), ",", (Player.Level + ID))
            End Get
        End Property
        Private ReadOnly Property niveauInfos() As String
            Get
                Return String.Concat(Player.niveau.Id, ",", Player.niveau.Id, ",", If(Player.niveau.Enabled, Player.niveau.Rank, 0), ",", (Player.Level + ID))
            End Get
        End Property

        Public ReadOnly Property GuildInfos() As String
            Get
                If Not Guild Is Nothing Then
                    Return Guild.Name & ";" & Guild.Emblem.ToString
                Else : Return ";"
                End If
            End Get
        End Property

        Public ReadOnly Property PatternDisplayEmote() As String
            Get
                Return String.Concat(Emotes.PlayedEmote & ";360000")
            End Get
        End Property

        Public ReadOnly Property PatternDisplayChar() As String
            Get

                Return String.Concat(
                    MapCell, ";", MapDir, ";0;", ID, ";", Name, _
                 ";", Classe & ";", Skin, "^", Size, ";" & Sexe, _
                 ";", AlignmentInfos, ";", _
                 PatternColor(";"), _
                 Items.GetItemsIDs, ";", _
                 Aura(), ";" & PatternDisplayEmote & ";", GuildInfos, _
                 ";" & Restriction.B36Restrictions & ";", If(State.IsMounted, Mount.MountLightDisplayInfo, ""), ";")

            End Get
        End Property

        Public ReadOnly Property PatternPrismView(ByVal MyPrism As Prism) As String
            Get

                Return String.Concat(
                    "GM|+", MyPrism.CellID, ";1;0;-13;", _
                 PrismManager.GetName(MyPrism), ";-10;", PrismManager.GetSkin(MyPrism), "^", _
                 "100;3;3;" & MyPrism.Faction)

            End Get
        End Property

        Public ReadOnly Property PatternDisplayMerchant() As String
            Get
                Dim Pattern As String = ""
                Pattern &= MapCell & ";" & MapDir & ";0;" & ID & ";" & Name & ";"
                Pattern &= "-5;" & Skin & "^" & Size & ";" & PatternColor(";") & Items.GetItemsIDs & ";"
                Pattern &= GuildInfos & ";0"
                Return Pattern
            End Get
        End Property

        Public ReadOnly Property PatternBattle(ByVal Cell As Integer, ByVal Team As Integer) As String
            Get
                Dim Pattern As String = String.Empty
                Pattern &= Cell & ";" & 1 & ";0;" & ID & ";" & Name
                Pattern &= ";" & Classe & ";" & Skin & "^" & Size & ";" & Sexe & ";" & Player.Level
                Pattern &= ";" & AlignmentInfos & ";"
                For i As Integer = 0 To 2
                    Pattern &= DeciToHex(Color(i)) & ";"
                Next
                Pattern &= Items.GetItemsIDs & ";" & Player.Life & ";" & Player.MaxPA & ";" & Player.MaxPM
                Pattern &= ";" & Player.Stats.Armor.Resistances.PercentNeutre.Total
                Pattern &= ";" & Player.Stats.Armor.Resistances.PercentTerre.Total
                Pattern &= ";" & Player.Stats.Armor.Resistances.PercentFeu.Total
                Pattern &= ";" & Player.Stats.Armor.Resistances.PercentEau.Total
                Pattern &= ";" & Player.Stats.Armor.Resistances.PercentAir.Total
                Pattern &= ";" & Player.Stats.Armor.EsquivePA.Total & ";" & Player.Stats.Armor.EsquivePM.Total
                Dim MyDindeID As String = ""
                If State.IsMounted = True Then
                    MyDindeID = Mount.Type.Id
                End If
                Pattern &= ";" & Team & ";" & MyDindeID & ";"
                Return Pattern
            End Get
        End Property

        Public ReadOnly Property PatternOnParty() As String
            Get
                Dim Pattern As String = String.Empty
                Pattern &= ID & ";" & Name & ";" & Skin & ";"
                For i As Integer = 0 To 2
                    Pattern &= Color(i) & ";"
                Next
                Pattern &= Items.GetItemsIDs & ";" & Player.Life & "," & Player.MaximumLife & ";" & Player.Level & ";"
                Pattern &= Player.Initiative & ";" & Player.Prospection & ";0"
                Return Pattern
            End Get
        End Property

        Public ReadOnly Property PatternSendGuild() As String
            Get
                Return String.Concat("gS", Guild.Name, "|4|8w33x|1|0|1")
            End Get
        End Property

        Public ReadOnly Property PatternPerceptor(ByVal Perceptor As World.Perceptor) As String
            Get
                Return String.Concat(
                    "GM|+", Perceptor.CellID & ";1;0;106;1h,34;-6;6000^99;;", _
                   Perceptor.Guild.Name & ";" & Perceptor.Guild.Emblem.ToString)
            End Get
        End Property

        Public Sub SendAccountStats()
            Client.Send("As" & Player.ToString)
        End Sub

        Public Sub SendPods()
            Client.Send("Ow", Items.Pods, "|", Player.MaxPods)
        End Sub

        Public Sub TeleportTo(ByVal MapId As Integer, ByVal MapCell As Integer)

            World.Players.LoadMap(Client, MapId, MapCell, True)

        End Sub

        Public Sub Restat()

            With Player.Stats.Base
                .Agilite.Base = 0
                .Chance.Base = 0
                .Force.Base = 0
                .Intelligence.Base = 0
                .Sagesse.Base = 0
                .Vitalite.Base = 0
            End With
            Player.CharactPoint = Player.Level * 5 - 5
            SendAccountStats()

        End Sub

        Public Sub OnMove(ByVal Cell As Integer)
            RaiseEvent OnMoved(Me, Cell)
        End Sub

        Public Sub BeMerchantRequest()
            Dim percent As String = Config.GetItem("MERCHANT_PERCENT")
            Dim price As Integer = MerchantBag.TotalPrice * (percent / 100)
            Client.Send("Eq|" & percent & "|" & price)
        End Sub

        Public Shared Sub BeMerchant(ByVal Client As GameClient)

            Dim MaxMerchant As Integer = Config.GetItem(Of Integer)("MAX_MERCHANT")
            Dim Map = Client.Character.GetMap

            If Map.MerchantList.Count < MaxMerchant Then
                If Client.Character.MerchantBag.Count > 0 Then
                    Dim percent As String = Config.GetItem("MERCHANT_PERCENT")
                    Dim price As Integer = Client.Character.MerchantBag.TotalPrice * (percent / 100)
                    If Client.Character.Player.Kamas >= price Then
                        Client.Character.Player.Kamas -= price

                        Dim Merchant As Merchant = CharactersManager.ToMerchant(Client.Character)
                        CharactersManager.CharactersList(Client.Character.Name) = Merchant
                        Map.MerchantList.Add(Merchant)

                        Client.Disconnect()

                        Map.AddEntity(Merchant.PatternDisplayMerchant)

                    Else
                        Client.SendMessage("Impossible vous n'avez pas assez de kamas")
                    End If
                Else
                    Client.SendMessage("Impossible vous n'avez pas d'objet en vente")
                End If
            Else
                Client.SendMessage("Impossible il y a déja " & MaxMerchant & " sur la carte")
            End If

        End Sub

        Public ReadOnly Property EmoteListPattern As String
            Get
                Return "eL" & Emotes.EmoteCapacity & "|"
            End Get
        End Property

        Public Sub PlayEmote(ByVal EmoteId As Integer)

            If Emotes.PlayedEmote = EmoteId Then
                GetMap.Send("eUK" & EmoteId & "|0")
                Emotes.PlayedEmote = 0
                Return
            End If

            Emotes.PlayedEmote = EmoteId
            GetMap.Send("eUK" & ID & "|" & EmoteId & "|360000")
        End Sub

        Public Sub AddEmote(ByVal EmoteId As Integer, ByVal Refresh As Boolean)
            If Not Emotes.Can(EmoteId) Then
                Client.Send("eA", EmoteId, "|", If(Refresh, "1", "0"))
                Emotes.SetValue(EmoteId, True)
            End If
        End Sub

        Public Sub RemoveEmote(ByVal EmoteId As Integer, ByVal Refresh As Boolean)
            If Emotes.Can(EmoteId) Then
                Client.Send("eR", EmoteId, "|", If(Refresh, "1", "0"))
                Emotes.SetValue(EmoteId, False)
            End If
        End Sub

        Public Sub Dead()

            If Config.GetItem(Of Boolean)("ACTIVE_HEROIC") Then

                Client.Send("M112")
                Skin = Classe & "3"
                IsDead = True
                Restriction.SetValue(CharacterRestriction.RestrictionEnum.IsTombe, True)
                DeathCount += 1
                niveauMax = Player.Level

            Else

                Dim LooseEnergy = Player.Level * 10
                If (Player.Energy > LooseEnergy) Then

                    Player.Life = 1
                    Player.Energy -= LooseEnergy
                    Client.SendMessage("Vous perdez " & LooseEnergy & " points d'énergies")

                    If SaveMap <> 0 AndAlso SaveCell <> 0 Then
                        MapId = SaveMap
                        MapCell = SaveCell
                    Else
                        MapId = Config.GetItem(Of Integer)("START_MAP")
                        MapCell = Config.GetItem(Of Integer)("START_CELL")
                    End If

                    Client.SendMessage("Retour au point de sauvegarde")

                Else

                    Client.Send("M112")
                    Skin = Classe & "3"
                    IsDead = True
                    Restriction.SetValue(CharacterRestriction.RestrictionEnum.IsTombe, True)

                End If
            End If
        End Sub

        Public Sub Free()

            If Config.GetItem(Of Boolean)("ACTIVE_HEROIC") Then

                Client.Send("GO")

            Else
                Skin = "8004"
                IsDead = True
                State.IsGhost = True
                Restriction.SetValue(CharacterRestriction.RestrictionEnum.IsTombe, False)
                Restriction.SetValue(CharacterRestriction.RestrictionEnum.IsSlow, True)
                Restriction.SetValue(CharacterRestriction.RestrictionEnum.ForceWalk, True)
                GetMap.RefreshCharacter(Client)
            End If
        End Sub

        Public WatchedBy As GameClient = Nothing

        Public Sub SendWatch(ByVal Type As String, ByVal Text As String)
            If Not WatchedBy Is Nothing Then
                WatchedBy.ConsoleMessage("[" & Name & "] (" & Type & ") : " & Text, 0)
            End If
        End Sub

        Public Shared Function Exist(ByVal CharacterName As String) As Boolean

            SyncLock Sql.CharactersSync

                Dim SQLText As String = "SELECT COUNT(*) FROM player_characters WHERE nom=@CharacterName"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)
                SQLCommand.Parameters.Add(New MySqlParameter("@CharacterName", CharacterName))

                Dim Result As Integer = SQLCommand.ExecuteScalar

                Return Result > 0

            End SyncLock

        End Function

        Private Shared ActualId As Integer = -1

        Public Shared Function GetActualID() As Integer

            If ActualId = -1 Then

                SyncLock Sql.CharactersSync

                    Dim SQLText As String = "SELECT id FROM player_characters ORDER BY id DESC LIMIT 0,1"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)

                    Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                    ActualId = 1000
                    If Result.Read Then
                        ActualId = Result("id")
                    End If

                    Result.Close()

                    ActualId += 1

                    Return ActualId

                End SyncLock

            Else

                ActualId += 1
                Return ActualId

            End If

        End Function

        Public Function CheckRegister() As Boolean
            If Client.Character.GetMap.NeedRegister AndAlso Not Client.Infos.IsRegister Then
                Client.Send("BP+5")
                Return False
            End If
            Return True
        End Function

        Public Function CheckRegisterWP() As Boolean
            If Client.Character.GetMap.NeedRegister AndAlso Not Client.Infos.IsRegister Then
                Return False
            End If
            Return True
        End Function

    End Class
End Namespace