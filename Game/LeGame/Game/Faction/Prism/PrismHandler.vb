Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World
Imports System.Text
Namespace Game
    Public Class PrismHandler

        Public Shared ListOfPrisms As New Dictionary(Of Integer, PrismTemplate)
        Public Shared DestinationsIdsOfPrism As New List(Of Integer)

        Public Shared Function GetTemplate(ByVal MapId As Integer, ByVal CellId As Integer) As PrismTemplate
            Return ListOfPrisms.FirstOrDefault(Function(z) z.Value.MapID = MapId AndAlso z.Value.CellID = CellId).Value
        End Function

        Public Shared Function GuessTemplate(ByVal MapId As Integer) As PrismTemplate
            Return ListOfPrisms.FirstOrDefault(Function(z) z.Value.MapID = MapId).Value
        End Function

        Public Shared Function HasPrism(ByVal MapId As Integer) As Boolean
            Return ListOfPrisms.FirstOrDefault(Function(z) z.Value.MapID = MapId).Value IsNot Nothing
        End Function

        Public Shared Function GetTemplate(ByVal Zone As Integer) As PrismTemplate
            If ListOfPrisms.ContainsKey(Zone) Then Return ListOfPrisms(Zone)
            Return Nothing
        End Function

        Public Shared Function GetTemplates(ByVal MapId As Integer) As IEnumerable(Of PrismTemplate)
            Return ListOfPrisms.Where(Function(z) z.Value.MapID = MapId).Select(Function(k) k.Value)
        End Function

        Public Shared Sub LoadPrisms()

            MyConsole.StartLoading("Loading prism from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM prism_data"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewPrism As New PrismTemplate

                    NewPrism.MapID = Result("mapid")
                    NewPrism.CellID = Result("cellid")
                    NewPrism.HasMonster = Result("hasMonster")
                    NewPrism.Faction = Result("faction")
                    NewPrism.Zone = Result("zone")

                    Dim pMap As Map = MapsHandler.GetMap(NewPrism.MapID)
                    If (pMap IsNot Nothing) Then

                        'pMap.PrismList.Add(NewPrism)
                        ListOfPrisms.Add(NewPrism.MapID, NewPrism)
                        DestinationsIdsOfPrism.Add(NewPrism.MapID)
                    End If

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListOfPrisms.Count & "@' zaaps loaded from database")

        End Sub

#Region "Functions"

        Private Shared Function FormatPacket(ByVal Packet As String) As Integer
            Dim MyZaapID As String = Packet.Substring(2)
            MyZaapID = MyZaapID.Replace("|", "")
            Return MyZaapID
        End Function

#End Region

        Public Shared Sub DeposePrism(ByVal Client As GameClient, ByVal MyPrism As PrismTemplate)
            Try
                If Client.Character.Player.Alignment.Rank >= 2 And Client.Character.Player.Alignment.Enabled = True And Client.Character.Player.Level >= 9 Then
                    If Not ListOfPrisms.ContainsKey(Client.Character.GetMap.Subarea) Then
                        AddPrism(MyPrism, Client)
                    Else
                        Client.Send("Im1149")
                    End If
                Else
                    Client.SendNormalMessage("Pour poser un prisme votre grade doit être supérieur au niveau <b>3</b> et votre <b>alignement</b> doit être <b>activé</b> !")
                End If
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

        Public Shared Sub AddPrism(ByVal ThePrism As PrismTemplate, ByVal Client As GameClient)
            Dim SkinID As Integer
            Dim NameID As Integer
            Select Case Client.Character.Player.Alignment.Id
                Case 1
                    SkinID = 8101
                    NameID = 1111
                    ThePrism.Faction = 1

                Case 2
                    SkinID = 8100
                    NameID = 1112
                    ThePrism.Faction = 2

            End Select
            ThePrism.MapID = Client.Character.MapId
            ThePrism.CellID = Client.Character.MapCell
            ThePrism.Zone = Client.Character.GetMap.Subarea
            ' On send le packet a toute la map + un world message pour l'avertissement
            Client.Character.GetMap.Send("GM|+" & Client.Character.MapCell & ";1;0;-13;" & NameID & ";-10;" & SkinID & "^" & Utils.Config.GetItem("PRISM_SIZE") & ";3;3;" & Client.Character.Player.Alignment.Id)
            SendDeposePrism(Client, ThePrism)
            SaveNewPrism(ThePrism, Client) ' Sauvegarde dans le SQL

            Dim pMap As Map = MapsHandler.GetMap(ThePrism.MapID)
            'pMap.PrismList.Add(ThePrism)
            ListOfPrisms.Add(ThePrism.Zone, ThePrism)
            DestinationsIdsOfPrism.Add(ThePrism.Zone)
        End Sub

        Public Shared Sub SendDeposePrism(ByVal Client As GameClient, ByVal MyPrism As PrismTemplate)
            For Each Player As GameClient In Players.GetPlayers
                Select Case Client.Character.Player.Alignment.Id
                    Case 1
                        Player.Send("am" & MyPrism.Zone & "|1|0")
                    Case 2
                        Player.Send("am" & MyPrism.Zone & "|2|0")
                End Select
            Next
        End Sub

        Public Shared Sub SaveNewPrism(ByVal MyPrism As PrismTemplate, ByVal Client As GameClient)

            Try
                SyncLock Sql.Others

                    Dim CreateString As String = "@mapid, @cellid, @hasMonster, @faction, @zone"

                    Dim SQLText As String = "INSERT INTO prism_data VALUES (" & CreateString & ")"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)
                    Dim P As MySqlParameterCollection = SQLCommand.Parameters

                    P.Add(New MySqlParameter("@mapid", MyPrism.MapID))
                    P.Add(New MySqlParameter("@cellid", MyPrism.CellID))
                    P.Add(New MySqlParameter("@hasMonster", "0"))
                    P.Add(New MySqlParameter("@faction", MyPrism.Faction))
                    P.Add(New MySqlParameter("@zone", MyPrism.Zone))

                    SQLCommand.ExecuteNonQuery()
                End SyncLock
            Catch ex As Exception
                Utils.MyConsole.Err("Can't create prism '@" & MyPrism.MapID & "@' : " & ex.Message)
                Client.SendNormalMessage(ex.ToString)
            End Try

        End Sub

        Private Shared Function DestsInfo(ByVal Client As GameClient, ByVal Prism As PrismTemplate) As String

            Dim pattern As New StringBuilder

            For Each dest As Integer In DestinationsIdsOfPrism
                Dim temp As PrismTemplate = GetTemplate(dest)
                Dim map1 As World.Map = Prism.GetMap
                Dim map2 As World.Map = temp.GetMap
                Dim price As Integer = (10 * (Math.Abs(map2.PosX - map1.PosX) + Math.Abs(map2.PosY - map1.PosY) - 1)) * 2
                If temp.Faction = Client.Character.Player.Alignment.Id And Not temp.MapID = Prism.MapID Then
                    If temp IsNot Nothing Then pattern.Append("|" & temp.MapID & ";" & price)
                End If
            Next

            Return pattern.ToString()
        End Function

        Public Shared Sub StartUse(ByVal Client As GameClient, ByVal Prism As PrismTemplate)

            If Prism.Faction = Client.Character.Player.Alignment.Id And Client.Character.Player.Alignment.Enabled = True Then
                Client.Character.State.IsTrading = True
                Client.Send("Wp", Client.Character.GetMap.Subarea, DestsInfo(Client, Prism))
            Else
                Client.SendMessage("Votre <b>alignement</b> est invalide ou <b>désactivé !</b>")
            End If

        End Sub

        Public Shared Sub Teleport(ByVal Client As GameClient, ByVal DestId As Integer, ByVal Prism As PrismTemplate)

            If Prism.Faction = Client.Character.Player.Alignment.Id And Client.Character.Player.Alignment.Enabled = True Then
                Dim temp As PrismTemplate = GetTemplate(DestId)
                Dim map1 As World.Map = Prism.GetMap
                Dim map2 As World.Map = temp.GetMap
                Dim price As Integer = (10 * (Math.Abs(map2.PosX - map1.PosX) + Math.Abs(map2.PosY - map1.PosY) - 1)) * 2

                If Client.Character.Player.Kamas < price Then
                    Client.SendNormalMessage("Vous n'avez pas assez de kamas pour voyager.")
                    Exit Sub
                End If

                Client.Character.Player.Kamas -= price
                Client.SendNormalMessage("Vous avez perdu " & price & " kamas.")
                Client.Character.TeleportTo(map2.Id, Cells.NearerCell(map2, Prism.CellID))
                StopUse(Client)
                Client.Character.SendAccountStats()
            Else
                Client.SendMessage("<b>Alignement</b> incorrect ou <b>désactivé</b> !")
            End If

        End Sub

        Public Shared Sub StopUse(ByVal Client As GameClient)

            Client.Character.State.EndTrade()
            Client.Send("Ww")

        End Sub
    End Class
End Namespace
