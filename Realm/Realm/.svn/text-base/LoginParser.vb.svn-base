Imports System.Text
Imports Vemu_rs.Utils

Namespace Realm
    Public Class LoginParser

        Private Client As LoginClient

        Public Sub New(ByVal eClient As LoginClient)
            Client = eClient
        End Sub

        Public Sub Unpack(ByVal Packet As String)

            Try

                Select Case Client.State.State

                    Case LoginState.WaitingPacket.Version
                        Version(Packet)

                    Case LoginState.WaitingPacket.Account
                        Account(Packet)

                    Case LoginState.WaitingPacket.Basic
                        Parse(Packet)

                End Select

            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
                Client.Disconnect()
            End Try

        End Sub

        Private Sub Version(ByVal Version As String)

            If Version = Utils.Config.GetItem("LOGIN_VERSION") Then
                Client.State.State = LoginState.WaitingPacket.Account
            Else
                Client.Send("AlEv" & Utils.Config.GetItem("LOGIN_VERSION"))
                Utils.MyConsole.Warn("Invalid or unknow version (@" & Version & "@) from @" & Client.Ip)
            End If

        End Sub

        Private Sub DisconnetClientsOnAccount(ByVal username As String)

            If Server.LoginServer.Clients.Count > 0 Then
                Dim AlreadyAccountPlayers = Server.LoginServer.Clients.Where(Function(p) p.AccountName = Client.AccountName)
                For Each player In AlreadyAccountPlayers
                    If player IsNot Client Then
                        player.Disconnect()
                    End If
                Next
            End If

        End Sub

        Private Sub Account(ByVal AuthString As String)

            Dim Auth() As String = AuthString.Split("#")

            If Auth.Length = 2 Then
                Dim Username As String = Auth(0)
                Dim CryptedPassword As String = Auth(1)

                Dim Infos As New LoginInfos(Username)
                Client.AccountName = Username

                If Infos.AccountExist AndAlso Client.State.CryptPassword(Infos.RealPassword) = CryptedPassword Then
                    If Not Infos.Banned Then

                        DisconnetClientsOnAccount(Username)

                        Client.Infos = Infos
                        Client.State.State = LoginState.WaitingPacket.Basic
                        LoginSuccess()
                    Else
                        Client.Send("AlEb")
                        Client.State.State = LoginState.WaitingPacket.None
                        Utils.MyConsole.Notice("Account '@" & Client.AccountName & "@' is banned")
                    End If
                Else
                    Client.Send("AlEf")
                    Client.State.State = LoginState.WaitingPacket.None
                    Utils.MyConsole.Notice("Unknow account or incorrect password (@" & Client.AccountName & "@)")
                End If
            Else
                Client.Send("AlEf")
                Client.State.State = LoginState.WaitingPacket.None
            End If

        End Sub

        Private Sub LoginSuccess()

            Utils.MyConsole.Notice("Client '@" & Client.AccountName & "@' authentified")

            Client.Send("Ad" & Client.AccountName)
            Client.Send("Ac0")
            Client.SendHosts()
            Client.Send("AlK" & If(Client.Infos.GmLevel > 0, 1, 0))
            Client.Send("AQ")

            Server.LoginServer.ClientDestroy(Client.Ip)

            If Config.GetItem(Of Boolean)("AUTO_CONNECT") AndAlso World.LoginKeys.KeyExist(Client.AccountName) Then
                Dim Key = World.LoginKeys.GetKey(Client.AccountName)
                If Key.LastServer <> 0 Then
                    SendGameServer(Key.LastServer)
                End If
            End If

        End Sub

        Private Sub Parse(ByVal Packet As String)

            Select Case Packet.Substring(0, 2)

                Case "Af"

                Case "Ax"
                    SendListCharacter()

                Case "AX"
                    SendGameServer(Packet.Substring(2))

            End Select

        End Sub

        Private Sub SendListCharacter()

            Dim SubscriptionTime As Long
            If Config.GetItem(Of Boolean)("ENABLE_SUBSCRIPTION") Then
                SubscriptionTime = Client.Infos.SubscriptionTime
                If SubscriptionTime < 0 Then SubscriptionTime = 0
            Else
                SubscriptionTime = -1
            End If

            Dim ListCharacters As New StringBuilder("AxK" & SubscriptionTime)
            For Each Server As GameServer In GameHandler.GameList
                If (Client.Infos.CharactersList.ContainsKey(Server.Id)) Then
                    ListCharacters.Append("|" & Server.Id & "," & Client.Infos.CharactersList(Server.Id).Count)
                End If
            Next
            Client.Send(ListCharacters.ToString())

        End Sub

        Private Sub SendGameServer(ByVal Id As Integer)

            Dim RequestedServer As GameServer = GameHandler.GetServer(Id)
            If Not RequestedServer Is Nothing Then

                If RequestedServer.Activaded Then

                    If RequestedServer.NumPlayer > Utils.Config.GetItem("MAX_PLAYER") AndAlso Not Client.Infos.GmLevel > 0 Then
                        Client.Send("AXEf")
                        Exit Sub
                    End If

                    Dim GameKey As LoginKey = Nothing

                    If World.LoginKeys.KeyExist(Client.AccountName) Then
                        GameKey = World.LoginKeys.RefreshKey(Client.AccountName, Client.Infos)
                    Else
                        GameKey = New LoginKey(Client.AccountName, Utils.Basic.RandomString(16), Client.Infos)
                        World.LoginKeys.KeyList.Add(GameKey)
                    End If

                    GameKey.LastServer = Id
                    RequestedServer.AddTicket(GameKey)

                    Utils.MyConsole.Info("@" & Client.AccountName & "@ is going to game server.")

                    Client.Send("AYK" & RequestedServer.Ip & ":" & RequestedServer.Port & ";" & GameKey.Key)

                End If

            Else
                Client.Send("AXEd")
            End If

        End Sub

    End Class
End Namespace