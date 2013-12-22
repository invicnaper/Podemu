Imports Podemu.Game
Imports System.Text
Imports MySql.Data.MySqlClient
Imports Podemu.World
Imports Podemu.Utils

Public Class Migration

    Public Shared Sub StartMigration(ByVal Client As GameClient)
        'Client.Send("AM?" & Client.Character.ID)
        Client.Send("AM" & SendCharacterList(Client))
    End Sub

    Private Shared Function SendCharacterList(ByVal Client As GameClient) As String

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
        'Client.Send(ListCharacter.ToString()) Then

        Return ListCharacter.ToString
    End Function

    Private Sub SendListCharacter(ByVal Client As GameClient)

        Dim SubscriptionTime As Long
        If Config.GetItem(Of Boolean)("ENABLE_SUBSCRIPTION") Then
            SubscriptionTime = Client.Infos.SubscriptionTime
            If SubscriptionTime < 0 Then SubscriptionTime = 0
        Else
            SubscriptionTime = -1
        End If

        Dim ListCharacters As New StringBuilder("AxK" & SubscriptionTime)
        'For Each Server As GameServer In GameHandler.GameList
        '    If (Client.Infos.CharactersList.ContainsKey(Server.Id)) Then
        '        ListCharacters.Append("|" & Server.Id & "," & Client.Infos.CharactersList(Server.Id).Count)
        '    End If
        'Next
        Client.Send(ListCharacters.ToString())

    End Sub

    Public Shared Sub Migration(ByVal Client As GameClient, ByVal Packet As String)

        If Packet.StartsWith("-") Then
            DeleteCharacter(Client, Packet.Substring(1))
        Else
            Dim Infos() As String = Packet.Split(";")

            ChangeCharacterServ(Client, Infos(1), GetServeurDest(Client), Infos(0))

            If Not GetCharacterName(Infos(0)) = Infos(1) Then
                ChangeName(Infos(1), Infos(0), Client, GetServeurDest(Client))
                'CharactersManager.CharactersList.Add(Infos(1), LoadCharacter(Infos(0)))
                'MsgBox("CharactersList = " & CharactersManager.CharactersList.ToString & "CharacterList = " & Client.CharacterList.ToString)
                'Client.CharacterList.Add(GetServeurDest(Client), LoadCharacter(Infos(0)))
                'Client.Infos.CharactersList.Add(GetServeurDest(Client), New List(Of String))
            End If

            ContinueMigration(Infos(0), Client)
            'SupprMigration(Client)
            Client.Send("M018|" & "le serveur pour terminer la migration de votre personnage" & "; ")
            Client.Disconnect()
            'LoadCharacters(Client)

        End If
    End Sub

    Private Shared Sub LoadCharacters(ByVal Client As GameClient)

        For i As Integer = 0 To Client.Infos.CharacterNumber - 1

            If CharactersManager.CharactersList.ContainsKey(Client.Infos.CharactersList(ServerId)(i)) Then

                Dim LoadChar As Character = CharactersManager.CharactersList(Client.Infos.CharactersList(ServerId)(i))

                If Not Client.CharacterList.ContainsKey(LoadChar.ID) Then
                    Client.CharacterList.Add(LoadChar.ID, LoadChar)
                End If

            Else
                DeleteCharacterFromAccount(Client, Client.Infos.CharactersList(ServerId)(i))
            End If

        Next

    End Sub

    Private Shared Sub DeleteCharacterFromAccount(ByVal Client As GameClient, ByVal CharacterName As String)

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

    Private Shared Sub DeleteCharacterFromDatabase(ByVal CharacterName As String)

        SyncLock Sql.CharactersSync

            Dim SQLText As String = "DELETE FROM player_characters WHERE nom=@CharacterName"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)
            SQLCommand.Parameters.Add(New MySqlParameter("@CharacterName", CharacterName))

            SQLCommand.ExecuteNonQuery()

            CharactersManager.CharactersList.Remove(CharacterName)

        End SyncLock

    End Sub

    Private Shared Sub DeleteCharacter(ByVal Client As GameClient, ByVal CharId As Integer)

        Client.Send("BN")

        If Client.CharacterList.ContainsKey(CharId) Then

            Dim CharacterName As String = Client.CharacterList(CharId).Name
            DeleteCharacterFromAccount(Client, CharacterName)
            DeleteCharacterFromDatabase(CharacterName)

            Client.Infos.DelCharacter(CharacterName)
            Client.CharacterList.Remove(CharId)
            'SendCharacterList(Client)
            StartMigration(Client)

            GameStats.AddToStats(GameStats.GameRightStat.CharactersWon, -1)

        End If

    End Sub

    Public Shared Function GetServeurDest(ByVal Client As GameClient) As String
        Dim Data1 As String

        SyncLock Sql.AccountsSync
            Dim SQLText As String = "SELECT * FROM player_accounts WHERE username='" & Client.AccountName & "'"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

            Using Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    Data1 = Result("migration")
                Else
                    Data1 = 0
                End If

                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()
            End Using
        End SyncLock
        Return Data1
    End Function

    Public Shared Function VerifMigration(ByVal Client As GameClient) As Boolean

        Dim Data1 As String
        Dim MigrationActivate As Boolean = False

        SyncLock Sql.AccountsSync
            Dim SQLText As String = "SELECT * FROM player_accounts WHERE username='" & Client.AccountName & "'"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

            Using Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    Data1 = Result("migration")
                Else
                    Data1 = 0
                End If

                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()

                If Data1 = "0" Or Data1.Contains("_") Then
                    MigrationActivate = False
                Else
                    MigrationActivate = True
                End If
            End Using
        End SyncLock
        Return MigrationActivate
    End Function

    Private Shared Function GetCharacterName(ByVal Id As Integer) As String

        Dim Data1 As String

        SyncLock Sql.CharactersSync
            Dim SQLText As String = "SELECT * FROM player_characters WHERE id=" & Id
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)

            Using Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    Data1 = Result("nom")
                Else
                    Data1 = ""
                End If

                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()
            End Using
        End SyncLock
        Return Data1
    End Function

    Public Shared Sub ChangeCharacterServ(ByVal Client As GameClient, ByVal NewCharacterName As String, ByVal ServDest As Integer, ByVal CharacterId As Integer)
        SyncLock Sql.AccountsSync

            Dim CharacterList As String = GetCharactersList(Client)
            Dim OldName As String = GetCharacterName(CharacterId)

            Dim NewCharacterList As String = CharacterList.Replace(OldName & "," & ServerId, NewCharacterName & "," & ServDest)
            Dim UpdateString As String = "characters=@NewList"

            Dim SQLText As String = "UPDATE player_accounts SET " & UpdateString & " WHERE username='" & Client.AccountName & "'"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
            Dim P As MySqlParameterCollection = SQLCommand.Parameters

            P.Add(New MySqlParameter("@NewList", NewCharacterList))
            SQLCommand.ExecuteNonQuery()
        End SyncLock
    End Sub

    Public Shared Sub ChangeName(ByVal NewName As String, ByVal CharacterId As Integer, ByVal Client As GameClient, ByVal ServDest As String)
        Dim SQLText As String = "UPDATE player_characters SET nom='" & NewName & "' WHERE id=" & CharacterId
        Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)
        SQLCommand.ExecuteNonQuery()
    End Sub

    Public Shared Sub ContinueMigration(ByVal CharactId As String, ByVal Client As GameClient)
        Dim SQLUpdate As String = "_" & CharactId
        Dim SQLText As String = "UPDATE player_accounts SET migration='" & SQLUpdate & "' WHERE username='" & Client.AccountName & "'"
        Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
        SQLCommand.ExecuteNonQuery()
    End Sub

    Public Shared Sub SupprMigration(ByVal Client As GameClient)
        Dim SQLText As String = "UPDATE player_accounts SET migration=0 WHERE username='" & Client.AccountName & "'"
        Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
        SQLCommand.ExecuteNonQuery()
    End Sub

    Public Shared Sub SupprAllMigration()
        Dim SQLText As String = "UPDATE player_accounts SET migration=0"
        Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
        SQLCommand.ExecuteNonQuery()
    End Sub

    Private Shared Function GetCharactersList(ByVal Client As GameClient) As String

        Dim Data1 As String

        SyncLock Sql.AccountsSync
            Dim SQLText As String = "SELECT * FROM player_accounts WHERE username='" & Client.AccountName & "'"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

            Using Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    Data1 = Result("characters")
                Else
                    Data1 = ""
                End If

                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()
            End Using
        End SyncLock
        Return Data1
    End Function

    Public Shared Sub LoadCharacter(ByVal CharacterId As Integer, ByVal Client As GameClient)

        Dim c As Character

        SyncLock Sql.CharactersSync

            Dim SQLText As String = "SELECT * FROM player_characters WHERE id='" & CharacterId & "'"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)

            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read


                If (Result.GetBoolean("is_merchant")) Then
                    c = New Merchant()
                Else
                    c = New Character()
                End If

                c.Name = Result("nom")

                c.ID = Result("id")
                c.Player.Level = Result("niveau")
                c.niveauMax = Result("niveauMax")
                c.Emotes.EmoteCapacity = Result.GetInt32("emotes")
                Dim zaaps As String = Result("zaaps")

                If zaaps <> "" Then
                    For Each Zaap As String In zaaps.Split(";")
                        If Zaap <> "" Then
                            c.Zaaps.Add(CInt(Zaap))
                        End If
                    Next
                End If


                Dim savePos As String() = Result.GetString("savePos").Split(",")
                If savePos.Length < 2 Then savePos = {"0", "0"}
                c.SaveMap = savePos(0)
                c.SaveCell = savePos(1)

                c.Restriction.Restriction = Result.GetInt32("restrictions")
                c.IsDead = Result.GetBoolean("is_dead")
                c.DeathCount = Result("death_count")
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

                c.Player.Stats.Base.Force = New StatsRow(Result("terre"))
                c.Player.Stats.Base.Vitalite = New StatsRow(Result("vitalite"))
                c.Player.Stats.Base.Sagesse = New StatsRow(Result("sagesse"))
                c.Player.Stats.Base.Agilite = New StatsRow(Result("agilite"))
                c.Player.Stats.Base.Chance = New StatsRow(Result("chance"))
                c.Player.Stats.Base.Intelligence = New StatsRow(Result("intelligence"))

                c.MapId = Result("mapid")
                c.MapCell = Result("mapcell")
                c.MapDir = Result("mapdir")

                Dim eSpells As String = Result("spells")

                c.Spells.ClearSpells()

                If eSpells <> "" Then
                    Dim SpellsSplit() As String = eSpells.Split("|")
                    For i As Integer = 0 To SpellsSplit.Length - 1
                        Dim SpellSplit() As String = SpellsSplit(i).Split(",")
                        c.Spells.AddSpell(SpellSplit(0), SpellSplit(1), SpellSplit(2))
                    Next
                End If

                Dim eItems As String = Result("items")

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

                c.State.IsMerchant = Result.GetBoolean("is_merchant")
                Dim bagItems As String = Result("sell_bag")

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

                If c.State.IsMerchant Then
                    c.GetMap.MerchantList.Add(c)
                End If

                'If Not CharactersManager.CharactersList.ContainsKey(c.Name) Then
                'If VerifMigration2(Client) = True Then
                CharactersManager.CharactersList.Add(c.Name, c)
                Client.CharacterList.Add(c.ID, c)
                Client.Infos.CharactersList.Add(c.ID, New List(Of String))
                'End If
                'End If

            End While

            Result.Close()

        End SyncLock
    End Sub

    Public Shared Function GetMigration(ByVal Client As GameClient) As String

        Dim Data1 As String

        SyncLock Sql.AccountsSync
            Dim SQLText As String = "SELECT * FROM player_accounts WHERE username='" & Client.AccountName & "'"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

            Using Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    Data1 = Result("migration")
                Else
                    Data1 = ""
                End If

                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()
            End Using
        End SyncLock
        Return Data1
    End Function

    Public Shared Function VerifMigration2(ByVal Client As GameClient) As Boolean

        Dim Data1 As String
        Dim MigrationActivate As Boolean = False

        SyncLock Sql.AccountsSync
            Dim SQLText As String = "SELECT * FROM player_accounts WHERE username='" & Client.AccountName & "'"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

            Using Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    Data1 = Result("migration")
                Else
                    Data1 = 0
                End If

                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()

                If Data1.Contains("_") Then
                    MigrationActivate = True
                Else
                    MigrationActivate = False
                End If
            End Using
        End SyncLock
        Return MigrationActivate
    End Function
End Class
