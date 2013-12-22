﻿Imports Podemu.Game
Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World.Players
Imports Podemu.World
Imports System.Threading
Namespace World
    Public Class GuildHandler

#Region "List"
        Public Shared ListOfGuild As New List(Of Guild)
        Public Shared ListOfPlayersGuild As New List(Of PlayerGuild)
        Public Shared ListOfPlayers As New List(Of GameClient)
#End Region

#Region "Functions"

        Private Shared Property Packet As String

        Public Shared Function ExistName(ByVal Name As String) As Boolean
            For Each MyGuild As Guild In ListOfGuild
                If MyGuild.Name = Name Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function ExistID(ByVal ID As Integer) As Boolean
            For Each MyGuild As Guild In ListOfGuild
                If MyGuild.GuildID = ID Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function GetNewID() As Integer
            Dim MyID As Integer = 1
            For Each AllGuild As Guild In ListOfGuild
                MyID += 1
            Next
            Return MyID
        End Function

        Public Shared Function GetGuildByID(ByVal ID As Integer) As Guild
            If ExistID(ID) Then
                For Each MyGuild As Guild In ListOfGuild
                    If MyGuild.GuildID = ID Then
                        Return MyGuild
                    End If
                Next
            Else
                Return Nothing
            End If
            Return Nothing
        End Function

        Public Shared Function GetTotalPlayers(ByVal TheGuild As Guild) As Integer
            Dim TotalCount As Integer = 0
            For Each TheChacters As PlayerGuild In ListOfPlayersGuild
                If TheChacters.GuildID = TheGuild.GuildID Then
                    TotalCount += 1
                End If
            Next
            Return TotalCount
        End Function

#End Region

#Region "SQL"

        Public Shared Sub LoadGuild()
            MyConsole.StartLoading("Loading guild from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM guild_data"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewGuild As New Guild

                    NewGuild.GuildID = Result("id")
                    NewGuild.Name = Result("name")
                    NewGuild.Level = Result("level")
                    NewGuild.Exp = Result("exp")
                    NewGuild.Leader = Result("leader")
                    Dim NewEmblem As New GuildEmblem
                    Dim EmblemData As String = Result("emblem")
                    Dim Data() As String = EmblemData.Split(",")
                    NewEmblem.BackId = Data(0)
                    NewEmblem.BackColor = Data(3)
                    NewEmblem.FrontId = Data(2)
                    NewEmblem.FrontColor = Data(1)
                    NewGuild.Emblem = NewEmblem

                    ListOfGuild.Add(NewGuild)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListOfGuild.Count & "@' guild loaded from database")

        End Sub

        Public Shared Sub SaveGuild(ByVal MyGuild As Guild, ByVal Client As GameClient)



            Dim ConMutex As New Threading.Mutex

            Try
                SyncLock Sql.Others
                    Dim CreateString As String = "@id, @name, @level, @exp, @leader, @emblem"

                    Dim SQLText As String = "INSERT INTO guild_data VALUES (" & CreateString & ")"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)
                    Dim P As MySqlParameterCollection = SQLCommand.Parameters

                    P.Add(New MySqlParameter("@id", MyGuild.GuildID))
                    P.Add(New MySqlParameter("@name", MyGuild.Name))
                    P.Add(New MySqlParameter("@level", MyGuild.Level))
                    P.Add(New MySqlParameter("@exp", MyGuild.Exp))
                    P.Add(New MySqlParameter("@leader", MyGuild.Leader))
                    P.Add(New MySqlParameter("@emblem", MyGuild.Emblem.ToSave))

                    SQLCommand.ExecuteNonQuery()
                End SyncLock

            Catch ex As Exception

            End Try

        End Sub

#End Region

#Region "Guild Processor"

        Public Shared Sub CreateGuild(ByVal Packet As String, ByVal Client As GameClient)
            Try
                If Client.Character.GuildID = 0 Then
                    Dim NewEmblem As New GuildEmblem ' On Créer l'embleme grace au packet
                    Packet = Packet.Substring(2) ' On enleve le gC :)
                    Dim Data() As String = Packet.Split("|") ' Et on parse =D
                    If ExistName(Data(4)) Then
                        Client.Send("an")
                        Exit Sub
                    End If
                    NewEmblem.BackId = Data(0)
                    NewEmblem.BackColor = Data(3)
                    NewEmblem.FrontId = Data(2)
                    NewEmblem.FrontColor = Data(1)
                    Dim NewGuild As New Guild
                    NewGuild.GuildID = GetNewID()
                    NewGuild.Emblem = NewEmblem
                    NewGuild.Name = Data(4)
                    NewGuild.Level = 1
                    NewGuild.Exp = 0
                    NewGuild.Leader = Client.Character.ID
                    Client.Character.GuildID = NewGuild.GuildID
                    Dim NewPlayersGuild As New PlayerGuild
                    NewPlayersGuild.ID = Client.Character.ID
                    NewPlayersGuild.GuildID = Client.Character.GuildID
                    NewPlayersGuild.Name = Client.Character.Name
                    NewPlayersGuild.Rank = 1
                    NewPlayersGuild.XpGive = 1
                    NewPlayersGuild.XpGived = 1
                    Client.Character.Rank = 1
                    ListOfGuild.Add(NewGuild) ' Ajout de la guilde
                    SaveGuild(NewGuild, Client)
                    ListOfPlayersGuild.Add(NewPlayersGuild) ' On ajoute le joueur au client qui ont une guilde
                    NewGuild.ListOfPlayer.Add(Client.Character)
                    Client.Character.Guild = NewGuild
                    Client.Character.Save() ' On sauvegarde 
                    Client.Send("gS" & NewGuild.Name & "|" & NewGuild.Emblem.ToString & "|" & 0)
                    Client.Send("gV")
                    Client.SendNormalMessage("Guilde créer !")
                    Client.Character.GetMap.RefreshCharacter(Client)
                Else
                    Client.SendNormalMessage("Vous posseder déjà une guilde !")
                    Client.Send("gV")
                End If
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

        Public Shared Function OnSendInfos(ByVal Client As GameClient) As String
            If Not Client.Character.GuildID = 0 Then ' Juste ou cas où :)
                Dim PacketgiG As String = "gIG"
                Dim ThisGuild As Guild = Client.Character.Guild
                PacketgiG &= "1|" & ThisGuild.Level & "|1|" & ThisGuild.Exp & "|1100"
                Return PacketgiG
            Else
                Return Nothing
                Exit Function
            End If
            Return Nothing
        End Function

        Public Shared Function OnSendPlayer(ByVal Client As GameClient) As String
            Try
                '|;NOM;LEVEL;SKIN;RANK;EXPGG;PERCENT;1;CONNECTED;ALIGNID;0
                If Client.Character.GuildID = Nothing Or Client.Character.GuildID = 0 Then
                    Return "|"
                End If

                Dim PacketSend As String = "gIM+"
                Dim IsFirst As String = ""
                For Each MyGuildPartners As GameClient In ListOfPlayers
                    If Not MyGuildPartners.Character.GuildID = Nothing And Not MyGuildPartners.Character.GuildID = 0 Then
                        If MyGuildPartners.Character.GuildID = Client.Character.GuildID Then
                            Dim IsConnected As Integer = 0
                            If MyGuildPartners.State.Created Then
                                IsConnected = 1
                            End If
                            PacketSend &= IsFirst & MyGuildPartners.Character.ID & ";" & MyGuildPartners.Character.Name & ";"
                            PacketSend &= MyGuildPartners.Character.Player.Level & ";"
                            PacketSend &= MyGuildPartners.Character.Skin & ";"
                            PacketSend &= MyGuildPartners.Character.Rank & ";"
                            PacketSend &= MyGuildPartners.Character.PlayersGuild.XpGived & ";"
                            PacketSend &= MyGuildPartners.Character.PlayersGuild.XpGive & ";"
                            PacketSend &= "1;"
                            PacketSend &= IsConnected & ";"
                            PacketSend &= MyGuildPartners.Character.Player.Alignment.Id
                            PacketSend &= ";0"
                            IsFirst = "|"
                        End If

                    End If
                Next
                ' Client.SendNormalMessage("PACKET GUILDE = " & PacketSend)
                Return PacketSend
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
            Return Nothing
        End Function

        Public Shared Sub OnInvite(ByVal Client As GameClient, ByVal Packet As String)
            Try
                Packet = Packet.Substring(3) ' On Enleve le prefixe du packet
                Dim InvitedClient As GameClient = World.Players.GetCharacter(Packet) ' On Get le client
                If Not InvitedClient.Character.GuildID = 0 Then ' Ou Cas où :)
                    Exit Sub
                End If
                Client.Send("gJR" & Packet)
                InvitedClient.Send("gJr" & Client.Character.ID & "|" & Client.Character.Name & "|" & Client.Character.Guild.Name)
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try

        End Sub

        Public Shared Sub OnAcceptInvitation(ByVal Client As GameClient, ByVal Packet As String)
            Try
                Packet = Packet.Substring(3)
                Dim ClientInviter As GameClient = World.Players.GetCharacter(CInt(Packet))
                ClientInviter.Send("gJKa" & Client.Character.Name)
                Client.Send("gS" & ClientInviter.Character.Guild.Name & "|" & ClientInviter.Character.Guild.Emblem.ToString & "|0")
                Dim NewPlayersGuild As New PlayerGuild
                NewPlayersGuild.GuildID = ClientInviter.Character.Guild.GuildID
                NewPlayersGuild.ID = Client.Character.ID
                NewPlayersGuild.Name = Client.Character.Name
                NewPlayersGuild.Rank = 0
                NewPlayersGuild.XpGive = 1
                NewPlayersGuild.XpGived = 1
                Client.Character.GuildID = ClientInviter.Character.GuildID
                Client.Character.Rank = 0
                Client.Character.PlayersGuild = NewPlayersGuild
                Client.Character.Guild = ClientInviter.Character.Guild
                ListOfPlayersGuild.Add(NewPlayersGuild)
                ListOfPlayers.Add(Client)
                Client.Character.GetMap.RefreshCharacter(Client)
                Client.Character.Save()
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

        Public Shared Sub OnRefuse(ByVal Client As GameClient, ByVal Packet As String)
            Packet = Packet.Substring(3)
            Dim ClientInviter As GameClient = World.Players.GetCharacter(CInt(Packet))
            ClientInviter.Send("gJEc")
        End Sub

        Public Shared Sub OnLeave(ByVal Client As GameClient, ByVal Packet As String)
            Packet = Packet.Substring(2)
            Dim MyKickedPlayer As GameClient = World.Players.GetCharacter(Packet)
            If MyKickedPlayer.Character.Rank = 1 And MyKickedPlayer Is Client Then
                OnAdminLeave(Client)
                Exit Sub
            Else
                If MyKickedPlayer.Character.Rank = 1 Then
                    Client.SendNormalMessage("<b>Impossible de supprimer le meneur de la guilde !</b>")
                    Exit Sub
                End If
                MyKickedPlayer.Character.GuildID = 0
                ListOfPlayersGuild.Remove(MyKickedPlayer.Character.PlayersGuild)
                ListOfPlayers.Remove(MyKickedPlayer)
                MyKickedPlayer.Character.Save()
                MyKickedPlayer.Send("gKK" & MyKickedPlayer.Character.Name & "|" & MyKickedPlayer.Character.Name)
            End If
        End Sub

        Public Shared Sub OnChangeRank(ByVal Client As GameClient, ByVal Packet As String)
            'Packet = gP1001|5|2|1
            Packet = Packet.Substring(2)
            Dim Data() As String = Packet.Split("|")
            Dim ID As Integer = Data(0)
            Dim TheRank As Integer = Data(1)
            Dim Percent As Integer = Data(2)
            Dim MyClient As GameClient = World.Players.GetCharacter(ID)
            If Not TheRank = 1 Then
                MyClient.Character.Rank = TheRank
            End If
            MyClient.Character.PlayersGuild.XpGive = Percent

        End Sub

        Public Shared Sub OnAdminLeave(ByVal Client As GameClient)
            ' A complter 
            Packet = Packet.Substring(2)
            Dim Data() As String = Packet.Split("|")
            Dim ID As Integer = Data(0)
            Dim TheRank As Integer = Data(1)
            Dim Percent As Integer = Data(2)
            Dim MyClient As GameClient = World.Players.GetCharacter(ID)
            If Not TheRank = 1 Then
                MyClient.Character.Rank = 5
            End If
            MyClient.Character.PlayersGuild.XpGive = Percent
            Dim MyKickedPlayer As GameClient = World.Players.GetCharacter(Packet)
            If MyKickedPlayer.Character.Rank = 1 And MyKickedPlayer Is Client Then
                OnAdminLeave(Client)
                Exit Sub
            Else
                If MyKickedPlayer.Character.Rank = 1 Then
                    Client.SendNormalMessage("<b>Impossible de supprimer  la guilde !</b>")
                    Exit Sub
                End If
                MyKickedPlayer.Character.GuildID = 0
                ListOfPlayersGuild.Remove(MyKickedPlayer.Character.PlayersGuild)
                ListOfPlayers.Remove(MyKickedPlayer)
                MyKickedPlayer.Character.Save()
                MyKickedPlayer.Send("gKK" & MyKickedPlayer.Character.Name & "|" & MyKickedPlayer.Character.Name)
            End If
        End Sub

        Public Shared Sub OnSendInfoPerco(ByVal Client As GameClient)
            '  ThisClient.Send("gIB2|0|700|7|1000|105|5|2|0|1070|462;0|461;0|460;1|459;0|458;1|457;0|456;0|455;0|454;0|453;0|452;0|451;0")
            'gIB2|0|PdV|Bonus DMG|Pods|Prospect|Sagesse|Nbr De perco|Pts a répartir|
            Client.Send("gIB1|0|700|7|1000|105|5|2|8|1070|462;0|461;0|460;1|459;0|458;1|457;0|456;0|455;0|454;0|453;0|452;0|451;0")


        End Sub

#End Region

    End Class
End Namespace
