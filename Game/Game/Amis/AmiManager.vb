'Travail anarchique oeuvré par mopamopa xD

Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.Utils.Basic
Imports Podemu.Game
Imports System.Text
Imports Podemu.World

Public Class AmiManager

    Public Shared ListOfFriends As New List(Of String)

    Public Shared Function LoadFriends(ByVal Client As GameClient) As String

        Dim MyList As String = ""

            SyncLock Sql.AccountsSync
                Dim SQLText As String = "SELECT * FROM player_accounts WHERE username='" & Client.AccountName & "'"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

                Using Result = SQLCommand.ExecuteReader

                    If Result.Read Then

                        Dim Friends As String = Result("friends")
                        If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()

                        ListOfFriends.Clear()

                        If Friends <> "" Then
                            If Friends.Contains(";") Then
                            For Each f As String In Friends.Split(";")
                                Dim pFriend As String = GetCharacterConnectedByPseudo(f)
                                Dim iFriend As GameClient = Players.GetCharacter(pFriend)

                                If Not IsNothing(iFriend) Then
                                    If MyList = "" Then
                                        MyList &= GetPseudo(iFriend) & ";" & IIf(iFriend.Character.State.InBattle = True, "2", "0") & ";" & iFriend.Character.Name & ";" & IIf(IsFriendOf(iFriend.AccountName, GetPseudo(Client)) = True, iFriend.Character.Player.Level, "?") & ";" & IIf(IsFriendOf(iFriend.AccountName, GetPseudo(Client)) = True, iFriend.Character.Player.Alignment.Id, "") & ";" & iFriend.Character.Classe & ";" & iFriend.Character.Sexe & ";" & iFriend.Character.Classe & iFriend.Character.Sexe
                                    Else
                                        MyList &= "|" & GetPseudo(iFriend) & ";" & IIf(iFriend.Character.State.InBattle = True, "2", "0") & ";" & iFriend.Character.Name & ";" & IIf(IsFriendOf(iFriend.AccountName, GetPseudo(Client)) = True, iFriend.Character.Player.Level, "?") & ";" & IIf(IsFriendOf(iFriend.AccountName, GetPseudo(Client)) = True, iFriend.Character.Player.Alignment.Id, "") & ";" & iFriend.Character.Classe & ";" & iFriend.Character.Sexe & ";" & iFriend.Character.Classe & iFriend.Character.Sexe
                                    End If
                                Else
                                    If MyList = "" Then
                                        MyList &= f
                                    Else
                                        MyList &= "|" & f
                                    End If
                                End If

                                ListOfFriends.Add(f)
                            Next
                        Else
                            Dim pFriend As String = GetCharacterConnectedByPseudo(Friends)
                            Dim iFriend As GameClient = Players.GetCharacter(pFriend)

                            If Not IsNothing(iFriend) Then
                                MyList &= GetPseudo(iFriend) & ";" & IIf(iFriend.Character.State.InBattle = True, "2", "0") & ";" & iFriend.Character.Name & ";" & IIf(IsFriendOf(iFriend.AccountName, GetPseudo(Client)) = True, iFriend.Character.Player.Level, "?") & ";" & IIf(IsFriendOf(iFriend.AccountName, GetPseudo(Client)) = True, iFriend.Character.Player.Alignment.Id, "") & ";" & iFriend.Character.Classe & ";" & iFriend.Character.Sexe & ";" & iFriend.Character.Classe & iFriend.Character.Sexe
                            Else
                                MyList &= Friends
                            End If
                            ListOfFriends.Add(Friends)
                            End If
                        End If
                    End If
                End Using
            End SyncLock
        Return MyList
    End Function

    Public Shared Function GetPseudo(ByVal Client As GameClient) As String

        Dim Data1 As String = ""

        SyncLock Sql.AccountsSync
            Dim SQLText As String = "SELECT * FROM player_accounts WHERE username='" & Client.AccountName & "'"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

            Using Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    Data1 = Result("pseudonyme")
                Else
                    Data1 = 0
                End If

                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()
            End Using
        End SyncLock
        Return Data1
    End Function

    Public Shared Sub SendFriends(ByVal Client As GameClient, ByVal MyList As List(Of String))
        Dim SQLText As String = "UPDATE player_accounts SET friends = '" & StringFriends() & "' WHERE username = '" & Client.AccountName & "'"
        Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
        SQLCommand.ExecuteNonQuery()
    End Sub

    Public Shared Sub AddFriend(ByVal Client As GameClient, ByVal NewFriend As String)

        If Not NewFriend.Contains("%") Then
                Dim iFriend As GameClient = World.Players.GetCharacter(NewFriend)
            Dim Pseudo As String = GetPseudoByPlayer(NewFriend)
            If Not ListOfFriends.Contains(Pseudo) Then

                If Not IsNothing(iFriend) Then
                    ListOfFriends.Clear()
                    LoadFriends(Client)
                    ListOfFriends.Add(Pseudo)
                    SendFriends(Client, ListOfFriends)
                    Client.Send("FA*" & NewFriend)
                    'FL|<compte>;<2=encombat>;<pseudo>;<niveau>;<alignID>;<classeid>;<sexid>;<classeid><sexid>
                    Client.Send("FL|" & GetPseudo(iFriend) & ";" & IIf(iFriend.Character.State.InBattle = True, "2", "0") & ";" & iFriend.Character.Name & ";" & iFriend.Character.Player.Level & ";" & iFriend.Character.Player.Alignment.Id & ";" & iFriend.Character.Classe & ";" & iFriend.Character.Sexe & ";" & iFriend.Character.Classe & iFriend.Character.Sexe)
                Else
                    Client.SendMessage("Impossible, ce personnage ou ce compte n'existe pas ou n'est pas connecté.")
                End If
            Else
                Client.SendMessage("Vous possédez déjà <b>" & iFriend.Character.Name & "</b> dans vos amis !")
            End If
        Else
            Dim eFriend() As String = NewFriend.Split("%")
            Dim iFriend As GameClient = World.Players.GetCharacter(eFriend(1))
            Dim Pseudo As String = GetPseudoByPlayer(eFriend(1))
            If Not ListOfFriends.Contains(Pseudo) Then

                If Not IsNothing(iFriend) Then
                    ListOfFriends.Clear()
                    LoadFriends(Client)
                    ListOfFriends.Add(Pseudo)
                    SendFriends(Client, ListOfFriends)
                    Client.Send("FA*" & eFriend(1))
                    'FL|<compte>;<2=encombat>;<pseudo>;<niveau>;<alignID>;<classeid>;<sexid>;<classeid><sexid>
                    Client.Send("FL|" & GetPseudo(iFriend) & ";" & IIf(iFriend.Character.State.InBattle = True, "2", "0") & ";" & iFriend.Character.Name & ";" & iFriend.Character.Player.Level & ";" & iFriend.Character.Player.Alignment.Id & ";" & iFriend.Character.Classe & ";" & iFriend.Character.Sexe & ";" & iFriend.Character.Classe & iFriend.Character.Sexe)
                Else
                    Client.SendMessage("Impossible, ce personnage ou ce compte n'existe pas ou n'est pas connecté.")
                    'Client.Send("cMEf" & eFriend(1))
                End If
            Else
                Client.SendMessage("Vous possédez déjà <b>" & iFriend.Character.Name & "</b> dans vos amis !")
            End If
        End If
    End Sub

    Public Shared Sub DeleteFriend(ByVal Client As GameClient, ByVal NewFriend As String)

        Dim Other As GameClient = Players.GetCharacter(NewFriend)

        If Not IsNothing(Other) Then
            Client.Send("FD*" & NewFriend)
            ListOfFriends.Remove(NewFriend)
            SendFriends(Client, ListOfFriends)
            Client.Send("FL|" & LoadFriends(Client))
        Else
            Client.Send("FD*" & GetPseudoByPlayer(NewFriend))
            ListOfFriends.Remove(NewFriend)
            SendFriends(Client, ListOfFriends)
            SendFriendsList(Client)
        End If

    End Sub

    Private Shared Function StringFriends() As String
        Return String.Join(";", ListOfFriends)
    End Function

    Public Shared Sub SendFriendsList(ByVal Client As GameClient)
        Client.Send("FL|" & LoadFriends(Client))
    End Sub

    Public Shared Function GetPseudoByPlayer(ByVal Player As String) As String
        Dim NombreDeLignes As Integer
        Dim Pseudonyme As String = ""

        SyncLock Sql.AccountsSync
            Dim SQLText As String = "SELECT COUNT(*) FROM player_accounts"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

            Using Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    NombreDeLignes = Result("COUNT(*)")
                Else
                    NombreDeLignes = 0
                End If
                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()
            End Using

            Dim i As Integer
            Dim Characters As String = ""
            For i = 1 To NombreDeLignes
                Dim SQLText2 As String = "SELECT * FROM player_accounts WHERE id='" & i & "'"
                Dim SQLCommand2 As New MySqlCommand(SQLText2, Sql.Accounts)

                Using Result2 = SQLCommand2.ExecuteReader
                    If Result2.Read Then
                        Characters = Result2("characters")
                    Else
                        Characters = ""
                    End If

                    If Characters.Contains(Player) Then
                        Pseudonyme = Result2("pseudonyme")
                    End If

                    If Not Result2 Is Nothing AndAlso Not Result2.IsClosed Then Result2.Close()
                End Using
            Next
        End SyncLock
        Return Pseudonyme
    End Function

    Public Shared Function GetCharacterConnectedByPseudo(ByVal Pseudo As String) As String
        Dim Characters As String
        Dim Pseudonyme As String = ""
        Dim CharacterCo As String = ""
        Dim Player As String
        Dim Min As Integer
        Dim Max As Integer

        SyncLock Sql.AccountsSync

            Dim SQLText As String = "SELECT * FROM player_accounts WHERE pseudonyme='" & Pseudo & "'"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

            Using Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    Characters = Result("characters")
                Else
                    Characters = ""
                End If

                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()
            End Using
        End SyncLock

        SyncLock Sql.CharactersSync
            Dim SQLText As String = "SELECT MIN(id) FROM player_characters"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)

            Using Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    Min = Result("MIN(id)")
                Else
                    Min = 0
                End If
                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()
            End Using

            Dim SQLText2 As String = "SELECT MAX(id) FROM player_characters"
            Dim SQLCommand2 As New MySqlCommand(SQLText2, Sql.Characters)

            Using Result = SQLCommand2.ExecuteReader
                If Result.Read Then
                    Max = Result("MAX(id)")
                Else
                    Max = 0
                End If
                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()
            End Using

            Dim i As Integer = Min
            Dim Connected As Boolean = False
            For i = i To Max
                Dim SQLText5 As String = "SELECT * FROM player_characters WHERE id='" & i & "'"
                Dim SQLCommand5 As New MySqlCommand(SQLText5, Sql.Characters)

                Using Result2 = SQLCommand5.ExecuteReader
                    If Result2.Read Then
                        Player = Result2("nom")
                        If Not Result2 Is Nothing AndAlso Not Result2.IsClosed Then Result2.Close()
                        If Characters.Contains(Player) And Not IsNothing(Players.GetCharacter(Player)) Then
                            Connected = True
                            If Characters.Contains(Player) And Connected = True Then
                                CharacterCo = Player
                            End If

                        End If
                    Else
                        Player = ""
                    End If
                End Using
            Next
        End SyncLock
        Return CharacterCo
    End Function

    Public Shared Function IsFriendOf(ByVal Username As String, ByVal TheFriend As String) As Boolean

        Dim Friends As String
        SyncLock Sql.AccountsSync

            Dim SQLText As String = "SELECT * FROM player_accounts WHERE username='" & Username & "'"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

            Using Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    Friends = Result("friends")
                Else
                    Friends = ""
                End If

                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()
            End Using
        End SyncLock

        If Friends.Contains(TheFriend) Then
            Return True
        Else
            Return False
        End If
    End Function
End Class
