Imports MySql.Data.MySqlClient
Imports System.Linq
Imports Podemu.Utils

Namespace Game
    Public Class CharactersManager

        Public Shared CharactersList As New Dictionary(Of String, Character)
        Private Shared _getCharacter As Object

        Shared Property GetCharacter(owner As Integer) As Object
            Get
                Return _getCharacter
            End Get
            Set(value As Object)
                _getCharacter = value
            End Set
        End Property

        Public Shared Sub LoadAll()

            MyConsole.StartLoading("Loading several number of characters...")

            SyncLock Sql.CharactersSync

                Dim SQLText As String = "SELECT * FROM player_characters"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Try

                        Dim c As Character

                        If (Result.GetBoolean("is_merchant")) Then
                            c = New Merchant()
                        Else
                            c = New Character()
                        End If

                        c.Name = Result("nom")

                        c.ID = Result("id")
                        c.Player.Level = Result("niveau")
                        c.Classe = Result("classe")
                        c.Sexe = Result("sexe")
                        c.Skin = Result("skin")
                        c.Size = 100

                        c.Color(0) = Result("color1")
                        c.Color(1) = Result("color2")
                        c.Color(2) = Result("color3")
                        c.Player.Exp = CLng(Result("exp"))
                        c.Player.Kamas = CLng(Result("kamas"))
                        c.Player.CharactPoint = Result("charactpoint")
                        c.Player.SpellPoint = Result("spellpoints")

                        c.Player.Life = Result("vie")
                        c.Player.Energy = Result("energie")

                        c.niveauMax = Result("niveauMax")

                        c.Player.Stats.Base.Force = New StatsRow(Result("terre"))
                        c.Player.Stats.Base.Vitalite = New StatsRow(Result("vitalite"))
                        c.Player.Stats.Base.Sagesse = New StatsRow(Result("sagesse"))
                        c.Player.Stats.Base.Agilite = New StatsRow(Result("agilite"))
                        c.Player.Stats.Base.Chance = New StatsRow(Result("chance"))
                        c.Player.Stats.Base.Intelligence = New StatsRow(Result("intelligence"))

                        c.MapId = Result("mapid")
                        c.MapCell = Result("mapcell")
                        c.MapDir = Result("mapdir")

                        c.Player.Alignment.Id = Result("alignment")
                        c.Player.Alignment.Exp = Result("alignmentExp")
                        c.Player.Alignment.Enabled = Result("alignmentEnabled")

                        c.GuildID = Result("guildid")
                        c.Rank = Result("rank")
                        If Not c.GuildID = 0 Then
                            c.Guild = World.GuildHandler.GetGuildByID(c.GuildID)
                            Dim NewGuildPlayer As New World.PlayerGuild
                            NewGuildPlayer.GuildID = c.GuildID
                            NewGuildPlayer.ID = c.ID
                            NewGuildPlayer.Name = c.Name
                            NewGuildPlayer.Rank = c.Rank
                            NewGuildPlayer.XpGive = 1
                            NewGuildPlayer.XpGived = 1
                            World.GuildHandler.ListOfPlayersGuild.Add(NewGuildPlayer)
                            c.PlayersGuild = NewGuildPlayer
                        End If

                        Dim mount As String = Result("mount")

                        If (mount <> "") Then
                            c.Mount = New World.Mount(mount)
                        End If

                        Dim mounts As String() = Result("paddock").Split("~")
                        For Each mString As String In mounts
                            If mString <> "" Then
                                c.Mounts.Add(New World.Mount(mString))
                            End If
                        Next

                        c.State.IsMerchant = Result.GetBoolean("is_merchant")
                        c.IsDead = Result.GetBoolean("is_dead")
                        c.DeathCount = Result("death_count")
                        c.Restriction.Restriction = Result.GetInt32("restrictions")
                        c.Emotes.EmoteCapacity = Result.GetInt32("emotes")

                        Dim savePos As String() = Result.GetString("savePos").Split(",")
                        If savePos.Length < 2 Then savePos = {"0", "0"}
                        c.SaveMap = savePos(0)
                        c.SaveCell = savePos(1)

                        Dim eSpells As String = Result("spells")
                        Dim eItems As String = Result("items")
                        Dim bagItems As String = Result("sell_bag")
                        Dim zaaps As String = Result("zaaps")

                        c.Spells.ClearSpells()

                        If eSpells <> "" Then
                            Dim SpellsSplit() As String = eSpells.Split("|")
                            For i As Integer = 0 To SpellsSplit.Length - 1
                                Dim SpellSplit() As String = SpellsSplit(i).Split(",")
                                c.Spells.AddSpell(SpellSplit(0), SpellSplit(1), SpellSplit(2))
                            Next
                        End If

                        c.Items.ClearItems()

                        If eItems <> "" Then
                            Dim ItemsSplit() As String = eItems.Split(";")
                            For i As Integer = 0 To ItemsSplit.Length - 1
                                Try
                                    Dim ItemSplit() As String = ItemsSplit(i).Split("~")
                                    c.Items.LoadItem(ItemSplit(0), ItemSplit(1), ItemSplit(2), ItemSplit(3), ItemSplit(4))
                                Catch
                                End Try
                            Next
                        End If

                        If bagItems <> "" Then
                            Dim ItemsSplit() As String = bagItems.Split(";")
                            For i As Integer = 0 To ItemsSplit.Length - 1
                                Try
                                    Dim ItemSplit() As String = ItemsSplit(i).Split("~")
                                    c.MerchantBag.LoadItem(ItemSplit(0), ItemSplit(1), ItemSplit(2), ItemSplit(3), ItemSplit(4))
                                Catch
                                End Try
                            Next
                        End If

                        If zaaps <> "" Then
                            For Each Zaap As String In zaaps.Split(";")
                                If Zaap <> "" Then
                                    c.Zaaps.Add(CInt(Zaap))
                                End If
                            Next
                        End If

                        If c.State.IsMerchant Then
                            c.GetMap.MerchantList.Add(c)
                        End If

                        If Not CharactersList.ContainsKey(c.Name) Then _
                            CharactersList.Add(c.Name, c)

                    Catch
                    End Try

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()

            MyConsole.Status("'@" & CharactersList.Count & "@' characters loaded from database")

            MyConsole.StartLoading("Loading guildPlayer from database...")
            MyConsole.StopLoading()
            MyConsole.Status("'@" & World.GuildHandler.ListOfPlayersGuild.Count & "@' guildPlayer loaded from database")

        End Sub

        Public Shared Function ToMerchant(ByVal C As Character) As Merchant

            Dim m As New Merchant

            m.Client = C.Client
            m.ID = C.ID
            m.Name = C.Name
            m.Classe = C.Classe
            m.Sexe = C.Sexe
            m.Skin = C.Skin
            m.Size = C.Size
            m.Color = C.Color
            m.Player = C.Player
            m.Restriction = C.Restriction
            m.MerchantBag = C.MerchantBag
            m.Items = C.Items
            m.State = C.State
            m.Spells = C.Spells
            m.Guild = C.Guild
            m.PlayersGuild = C.PlayersGuild
            m.Mount = C.Mount
            m.Mounts = C.Mounts

            m.IsFollowed = C.IsFollowed

            m.GuildID = C.GuildID
            m.Rank = C.Rank

            m.MapId = C.MapId
            m.MapCell = C.MapCell
            m.MapDir = If(Rnd() < 0.5, 1, 3)

            m.Channels = C.Channels
            m.Rights = C.Rights

            m.State.IsMerchant = True

            Return m

        End Function

        Public Shared Function ToCharacter(ByVal m As Merchant) As Character

            Dim c As New Character

            c.Client = m.Client
            c.ID = m.ID
            c.Name = m.Name
            c.Classe = m.Classe
            c.Sexe = m.Sexe
            c.Skin = m.Skin
            c.Size = m.Size
            c.Color = m.Color
            c.Player = m.Player
            c.Restriction = m.Restriction
            c.MerchantBag = m.MerchantBag
            c.Items = m.Items
            c.State = m.State
            c.Spells = m.Spells
            c.Guild = m.Guild
            c.PlayersGuild = m.PlayersGuild
            c.Mount = m.Mount
            c.Mounts = m.Mounts

            c.IsFollowed = m.IsFollowed

            c.GuildID = m.GuildID
            c.Rank = m.Rank

            c.MapId = m.MapId
            c.MapCell = m.MapCell
            c.MapDir = m.MapDir

            c.Channels = m.Channels
            c.Rights = m.Rights

            c.State.IsMerchant = False

            Return c

        End Function

    End Class
End Namespace