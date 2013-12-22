Imports MySql.Data.MySqlClient
Imports Vemu_gs.Utils
Imports Vemu_gs.World
Namespace Game
    Public Class ZaapisManager

        Public Shared ListOfZaapis As New List(Of Zaapis)

        Public Shared Sub LoadZaapis()

            MyConsole.StartLoading("Loading zaapis from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM zaapis"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewZaapis As New Zaapis

                    NewZaapis.MapID = Result("mapid")
                    NewZaapis.CellID = Result("cellid")
                    NewZaapis.Zone = Result("zone")

                    ListOfZaapis.Add(NewZaapis)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListOfZaapis.Count & "@' zaapis loaded from database")

        End Sub

#Region "Functions"

        Public Shared Function FormatPacket(ByVal Packet As String) As Integer
            Dim MyZaapID As String = Packet.Substring(2)
            MyZaapID = MyZaapID.Replace("|", "")
            Return MyZaapID
        End Function

        Public Shared Function Exist(ByVal MapID As Integer) As Boolean
            For Each MyZaapis As Zaapis In ListOfZaapis
                If MyZaapis.MapID = MapID Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function _Get(ByVal MapID As Integer) As Zaapis
            For Each MyZaapis As Zaapis In ListOfZaapis
                If MyZaapis.MapID = MapID Then
                    Return MyZaapis
                End If
            Next
            Return Nothing
        End Function

#End Region

        Public Shared Sub SendInfos(ByVal Client As GameClient)
            If Exist(Client.Character.MapId) Then
                Dim MyZaapis As Zaapis = _Get(Client.Character.MapId)

                Select Case MyZaapis.Zone ' On check la zone

                    Case 1 ' Bonta
                        Dim AllZ As String = "" ' String contenant la liste pour le packet
                        For Each AllZaapis As Zaapis In ListOfZaapis
                            If AllZaapis.Zone = 1 Then
                                AllZ += (AllZaapis.MapID & ";20|")
                            End If
                        Next
                        Client.Send("Wc" & Client.Character.MapId & "|" & AllZ)

                    Case 2 ' Brakmar 
                        Dim AllZ As String = "" ' String contenant la liste pour le packet
                        For Each AllZaapis As Zaapis In ListOfZaapis
                            If AllZaapis.Zone = 2 Then
                                AllZ += (AllZaapis.MapID & ";20|")
                            End If
                        Next
                        Client.Send("Wc" & Client.Character.MapId & "|" & AllZ)

                End Select

            Else
                Client.SendNormalMessage("<b>Zaapi</b> inexistant dans la base de données !")
            End If
        End Sub

        Public Shared Sub OnMove(ByVal MyZaapis As Zaapis, ByVal Client As GameClient)
            Try
                If MyZaapis.Zone = Client.Character.Player.Alignment.Id Or Client.Character.Player.Alignment.Id = 0 Then
                    Client.Character.TeleportTo(MyZaapis.MapID, MyZaapis.CellID)
                    Client.Send("Wv")
                Else
                    Client.SendNormalMessage("Alignement incorrect !")
                End If
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

    End Class
End Namespace
