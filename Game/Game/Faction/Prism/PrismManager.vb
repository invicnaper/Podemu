Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World.Players
Imports Podemu.World
Imports System.Threading
Namespace Game
    Public Class PrismManager

#Region "List"

        Public Shared ListOfPrism As New List(Of Prism)

#End Region

#Region "Functions"

        Public Shared Function ExistOnMap(ByVal MapID As Integer) As Boolean
            For Each MyPrism As Prism In ListOfPrism
                If MapID = MyPrism.Pos Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function ExistOnZone(ByVal Zone As Integer) As Boolean
            For Each MyZone As Prism In ListOfPrism
                If MyZone.Zone = Zone Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function Listing(ByVal Client As GameClient) As String ' Fonction pour envoyer un listing des prismes poser
            Dim MyListing As String = ("Wp" & Client.Character.MapId & "|")
            For Each MyPrism As Prism In ListOfPrism
                MyListing += MyPrism.Zone + "1000|"
            Next
            Return MyListing
        End Function

        Public Shared Function _GetByMapID(ByVal MapID As Integer) As Prism
            For Each MyPrism As Prism In ListOfPrism
                If MyPrism.Pos = MapID Then
                    Return MyPrism
                End If
            Next
            Return Nothing
        End Function

        Public Shared Function _GetByZone(ByVal Zone As Integer) As Prism
            For Each MyPrism As Prism In ListOfPrism
                If MyPrism.Zone = Zone Then
                    Return MyPrism
                End If
            Next
            Return Nothing
        End Function

        Public Shared Function Leave() As String
            Return "Ww"
        End Function

        Public Shared Function HaveCaracts(ByVal Client As GameClient) As Boolean
            If Client.Character.Player.Alignment.Rank >= 3 And Client.Character.Player.Alignment.Enabled = True Then
                Return True
            End If
            Return False
        End Function

        Public Shared Sub SendDeposit(ByVal Client As GameClient, ByVal MyPrism As Prism)
            For Each Player As GameClient In Players.GetPlayers
                Select Case Client.Character.Player.Alignment.Id
                    Case 1
                        Player.Send("am" & MyPrism.Zone & "|1|0")
                    Case 2
                        Player.Send("am" & MyPrism.Zone & "|2|0")
                End Select
            Next
        End Sub

        Public Shared Sub AddPrismOnMap(ByVal ThePrism As Prism, ByVal Client As GameClient)
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
            ThePrism.Pos = Client.Character.MapId
            ThePrism.CellID = Client.Character.MapCell
            ThePrism.Zone = Client.Character.GetMap.Subarea
            ThePrism.ID = GetNewID()
            ' On send le packet a toute la map + un world message pour l'avertissement
            Client.Character.GetMap.Send("GM|+" & Client.Character.MapCell & ";1;0;-13;" & NameID & ";-10;" & SkinID & "^" & Utils.Config.GetItem("PRISM_SIZE") & ";3;3;" & Client.Character.Player.Alignment.Id)
            SendDeposit(Client, ThePrism)
            ListOfPrism.Add(ThePrism) ' Ajout du prism dans la liste
            SavePrism(ThePrism, Client) ' Sauvegarde dans le SQL
        End Sub

        Public Shared Function GetName(ByVal ThePrism As Prism) As Integer
            If ThePrism.Faction = 1 Then
                Return 1111
            Else
                Return 1112
            End If
        End Function

        Public Shared Function GetSkin(ByVal ThePrism As Prism) As Integer
            If ThePrism.Faction = 1 Then
                Return 8101
            Else
                Return 8100
            End If
        End Function

        Public Shared Function GetNewID() As Integer
            Dim MyNewId As Integer = 1
            For Each MyPrism As Prism In ListOfPrism
                MyNewId += 1
            Next
            Return MyNewId
        End Function

#End Region

#Region "SQL"

        Public Shared Sub LoadPrism()
            MyConsole.StartLoading("Loading prism from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM prism_data"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewPrism As New Prism

                    NewPrism.ID = Result("id")
                    NewPrism.Faction = Result("faction")
                    NewPrism.Zone = Result("zone")
                    NewPrism.Pos = Result("pos")
                    NewPrism.CellID = Result("cell")
                    NewPrism.Timer = Result("timer")

                    ListOfPrism.Add(NewPrism)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListOfPrism.Count & "@' prisms loaded from database")
        End Sub

        Public Shared Sub SavePrism(ByVal MyPrism As Prism, ByVal Client As GameClient)

            Try
                SyncLock Sql.Others

                    Dim CreateString As String = "@id, @faction, @zone, @pos, @cell, @timer"

                    Dim SQLText As String = "INSERT INTO prism_data VALUES (" & CreateString & ")"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)
                    Dim P As MySqlParameterCollection = SQLCommand.Parameters

                    P.Add(New MySqlParameter("@id", MyPrism.ID))

                    P.Add(New MySqlParameter("@faction", MyPrism.Faction))
                    P.Add(New MySqlParameter("@zone", MyPrism.Zone))
                    P.Add(New MySqlParameter("@pos", MyPrism.Pos))
                    P.Add(New MySqlParameter("@cell", MyPrism.CellID))
                    P.Add(New MySqlParameter("@timer", MyPrism.Timer))

                    SQLCommand.ExecuteNonQuery()
                End SyncLock
            Catch ex As Exception
                Utils.MyConsole.Err("Can't create prism '@" & MyPrism.Pos & "@' : " & ex.Message)
                Client.SendNormalMessage(ex.ToString)
            End Try

        End Sub

#End Region

#Region "Prism Processor"

        Public Shared Sub OnDeposit(ByVal Client As GameClient, ByVal MyPrism As Prism)
            Try
                If HaveCaracts(Client) Then
                        If Not ExistOnZone(Client.Character.GetMap.Subarea) Then
                            AddPrismOnMap(MyPrism, Client)
                        Else
                        Client.Send("Im1149")
                        End If
                Else
                    Client.SendNormalMessage("Pour poser un prisme vous devez etre de grade <b>3</b> et vos <b>ailes</b> doivent etre <b>activer</b> !")
                End If
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

        Public Shared Sub GetPanel(ByVal Client As GameClient, ByVal MyPrism As Prism)
            Try
                If MyPrism.Faction = Client.Character.Player.Alignment.Id Then
                    Dim WpPacket As String = "Wp" & Client.Character.GetMap.Subarea & "|"
                    For Each AllPrism As Prism In ListOfPrism
                        If MyPrism.Faction = AllPrism.Faction Then
                            WpPacket += AllPrism.Zone & ";1000|"
                        End If
                    Next
                    Client.Send(WpPacket)
                    'Client.SendNormalMessage("<b>Received</b> = " & WpPacket)
                Else
                    Client.SendNormalMessage("Action impossible, vous n'etes pas du même <b>Alignement</b> que le prisme !")
                End If
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

        Public Shared Sub OnTeleport(ByVal Client As GameClient, ByVal ThePrism As Prism)
            Try
                Client.Character.TeleportTo(ThePrism.Pos, ThePrism.CellID)
                Client.Send("Ww")
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

#End Region

    End Class
End Namespace
