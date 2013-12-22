Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World
Namespace Game
    Public Class ZaapisManager

        Public Shared ListOfZaapis As New Dictionary(Of Integer, ZaapisTemplate)

        Public Shared Function GetTemplate(ByVal MapId As Integer, ByVal CellId As Integer) As ZaapisTemplate
            Return ListOfZaapis.FirstOrDefault(Function(z) z.Value.MapID = MapId AndAlso z.Value.CellID = CellId).Value
        End Function

        Public Shared Function GuessTemplate(ByVal MapId As Integer) As ZaapisTemplate
            Return ListOfZaapis.FirstOrDefault(Function(z) z.Value.MapID = MapId).Value
        End Function

        Public Shared Function HasZaapi(ByVal MapId As Integer) As Boolean
            Return ListOfZaapis.FirstOrDefault(Function(z) z.Value.MapID = MapId).Value IsNot Nothing
        End Function

        Public Shared Function GetTemplate(ByVal Id As Integer) As ZaapisTemplate
            If ListOfZaapis.ContainsKey(Id) Then Return ListOfZaapis(Id)
            Return Nothing
        End Function

        Public Shared Function GetTemplates(ByVal MapId As Integer) As IEnumerable(Of ZaapisTemplate)
            Return ListOfZaapis.Where(Function(z) z.Value.MapID = MapId).Select(Function(k) k.Value)
        End Function

        Public Shared Sub LoadZaapis()

            MyConsole.StartLoading("Loading zaapis from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM zaapis"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewZaapis As New ZaapisTemplate

                    NewZaapis.MapID = Result("mapid")
                    NewZaapis.CellID = Result("cellid")
                    NewZaapis.HasMonster = Result("hasMonster")
                    NewZaapis.Zone = Result("zone")

                    Dim destinations As String() = Result.GetString("destIds").Split(";")

                    For Each destination As String In destinations
                        If destination <> "" Then
                            NewZaapis.DestinationIds.Add(CInt(destination))
                        End If
                    Next

                    ListOfZaapis.Add(NewZaapis.MapID, NewZaapis)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListOfZaapis.Count & "@' zaapis loaded from database")

        End Sub

#Region "Functions"

        Public Shared Function FormatPacket(ByVal Packet As String) As Integer
            Dim MyZaapiID As String = Packet.Substring(2)
            MyZaapiID = MyZaapiID.Replace("|", "")
            Return MyZaapiID
        End Function

        Private Shared Function Leave() As String
            Return "WV"
        End Function

        ' Public Shared Function Exist(ByVal MapID As Integer) As Boolean
        'For Each MyZaapis As ZaapisTemplate In ListOfZaapis
        '     If MyZaapis.MapID = MapID Then
        '       Return True
        ''   End If
        ' Next
        '  Return False
        '  End Function
        '
        'Public Shared Function _Get(ByVal MapID As Integer) As Zaapis
        '    For Each MyZaapis As Zaapis In ListOfZaapis
        '        If MyZaapis.MapID = MapID Then
        '            Return MyZaapis
        '        End If
        '    Next
        '    Return Nothing
        'End Function

#End Region

        'Public Shared Sub SendInfos(ByVal Client As GameClient)
        '    If Exist(Client.Character.MapId) Then
        '        Dim MyZaapis As Zaapis = _Get(Client.Character.MapId)

        '        Select Case MyZaapis.Zone ' On check la zone

        '            Case 1 ' Bonta
        '                Dim AllZ As String = "" ' String contenant la liste pour le packet
        '                For Each AllZaapis As Zaapis In ListOfZaapis
        '                    If AllZaapis.Zone = 1 Then
        '                        AllZ += (AllZaapis.MapID & ";20|")
        '                    End If
        '                Next
        '                Client.Send("Wc" & Client.Character.MapId & "|" & AllZ)

        '            Case 2 ' Brakmar 
        '                Dim AllZ As String = "" ' String contenant la liste pour le packet
        '                For Each AllZaapis As Zaapis In ListOfZaapis
        '                    If AllZaapis.Zone = 2 Then
        '                        AllZ += (AllZaapis.MapID & ";20|")
        '                    End If
        '                Next
        '                Client.Send("Wc" & Client.Character.MapId & "|" & AllZ)

        '        End Select

        '    Else
        '        Client.SendNormalMessage("<b>Zaapi</b> inexistant dans la base de données !")
        '    End If
        'End Sub

        'Public Shared Sub OnMove(ByVal MyZaapis As Zaapis, ByVal Client As GameClient)
        '    Try
        '        If MyZaapis.Zone = Client.Character.Player.Alignment.Id Or Client.Character.Player.Alignment.Id = 0 Then
        '            Client.Character.TeleportTo(MyZaapis.MapID, MyZaapis.CellID)
        '            Client.Send("Wv")
        '        Else
        '            Client.SendNormalMessage("Alignement incorrect !")
        '        End If
        '    Catch ex As Exception
        '        Utils.MyConsole.Err(ex.ToString)
        '    End Try
        'End Sub

    End Class
End Namespace
