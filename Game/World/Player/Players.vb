Imports Podemu.Game
Imports Podemu.Utils

Namespace World
    Public Class Players

        Private Shared _level As GameClient

        Shared Property Level(ByVal Parameters As String) As GameClient
            Get
                Return _level
            End Get
            Set(ByVal value As GameClient)
                _level = value
            End Set
        End Property

        Public Shared Function GetPlayers() As List(Of GameClient)
            Return (From Player In Server.GameServer.Clients.ToArray Select Player Where (Not Player Is Nothing) AndAlso Player.State.Created = True).ToList
        End Function


        Public Shared Function GetPlayerCount() As Integer
            Return GetPlayers().Count
        End Function

        Public Shared Sub Send(ByVal Packet As String)

            Dim Players As List(Of GameClient) = GetPlayers()
            For Each Player As GameClient In Players
                Player.Send(Packet)
            Next

        End Sub

        Public Shared Sub SendMessage(ByVal Message As String)

            Dim Players As List(Of GameClient) = GetPlayers()
            For Each Player As GameClient In Players
                Player.SendMessage(Message)
            Next

        End Sub

        Public Shared Sub SendGlobalmessage(ByVal Message As String)

            Dim Players As List(Of GameClient) = GetPlayers()
            For Each Player As GameClient In Players
                Player.SendGlobalMessage(Message)
            Next

        End Sub


        Public Shared Sub SendAidemessage(ByVal Message As String)

            Dim Players As List(Of GameClient) = GetPlayers()
            For Each Player As GameClient In Players
                Player.SendAideMessage(Message)
            Next

        End Sub

        Public Shared Sub SendPublmessage(ByVal Message As String)

            Dim Players As List(Of GameClient) = GetPlayers()
            For Each Player As GameClient In Players
                Player.SendPublMessage(Message)
            Next

        End Sub

        Public Shared Sub GuildMessage(ByVal Message As String)

            Dim Players As List(Of GameClient) = GetPlayers()
            For Each Player As GameClient In Players
                Player.SendMessage(Message)
            Next

        End Sub

        Public Shared Sub SendWelcomeMessage(ByVal Client As GameClient)
            Client.SendMessage(Config.GetItem("MSG_WELCOME"))
            If Config.GetItem("MSG_ONCONNECT") = True Then
                Dim Message As String = Config.GetItem("MSG_CONNECT"). _
                    Replace("[name]", Client.Character.Name). _
                    Replace("[total]", World.Players.GetPlayerCount())
                SendMessage(Message)
            End If
        End Sub

        Public Shared Function GetCharacter(ByVal Name As String) As GameClient

            Dim NameList As List(Of GameClient) = _
                (From Player In GetPlayers() Select Player Where Player.Character.Name.ToLower = Name.ToLower).ToList

            If NameList.Count > 0 Then Return NameList(0)
            Return Nothing

        End Function

        Public Shared Function GetCharacter(ByVal Id As Integer) As GameClient

            Dim NameList As List(Of GameClient) = _
                (From Player In GetPlayers() Select Player Where Player.Character.ID = Id).ToList

            If NameList.Count > 0 Then Return NameList(0)
            Return Nothing

        End Function

        Public Shared Sub LoadMap(ByVal Client As GameClient, ByVal MapID As String, ByVal Cell As Integer, ByVal ChangeMap As Boolean)

            If MapsHandler.Exist(MapID) Then

                If ChangeMap AndAlso MapID = Client.Character.MapId Then
                    Client.Character.MapCell = Cell
                    Client.Character.GetMap.Send("GA;4;" & Client.Character.ID & ";" & Client.Character.ID & "," & Cell)
                Else

                    Dim NewMap As Map = MapsHandler.GetMap(MapID)

                    If ChangeMap Then
                        Client.Send("GA;2;" & Client.Character.ID & ";")
                        If Client.Character.GetMap IsNot Nothing Then Client.Character.GetMap.DelCharacter(Client)
                        Client.Character.MapCell = Cell
                        Client.Character.MapId = MapID
                    End If

                    Client.Send("GDM|" & NewMap.Id & "|" & NewMap.Time & "|" & NewMap.Key)

                End If

            Else
                Client.SendMessage("Carte de destination introuvable.")
                Exit Sub
            End If

        End Sub

        Shared Sub SendPubMessage(ByVal p1 As String)
            Throw New NotImplementedException
        End Sub
        Shared Sub SendAidemodoMessage(ByVal p1 As String)
            Throw New NotImplementedException
        End Sub

    End Class
End Namespace