Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World
Namespace Game
    Public Class ZaapsHandler

        Public Shared ListOfZaaps As New Dictionary(Of Integer, ZaapTemplate)

        Public Shared Function GetTemplate(ByVal MapId As Integer, ByVal CellId As Integer) As ZaapTemplate
            Return ListOfZaaps.FirstOrDefault(Function(z) z.Value.MapID = MapId AndAlso z.Value.CellID = CellId).Value
        End Function

        Public Shared Function GuessTemplate(ByVal MapId As Integer) As ZaapTemplate
            Return ListOfZaaps.FirstOrDefault(Function(z) z.Value.MapID = MapId).Value
        End Function

        Public Shared Function HasZaap(ByVal MapId As Integer) As Boolean
            Return ListOfZaaps.FirstOrDefault(Function(z) z.Value.MapID = MapId).Value IsNot Nothing
        End Function

        Public Shared Function GetTemplate(ByVal Id As Integer) As ZaapTemplate
            If ListOfZaaps.ContainsKey(Id) Then Return ListOfZaaps(Id)
            Return Nothing
        End Function

        Public Shared Function GetTemplates(ByVal MapId As Integer) As IEnumerable(Of ZaapTemplate)
            Return ListOfZaaps.Where(Function(z) z.Value.MapID = MapId).Select(Function(k) k.Value)
        End Function

        Public Shared Sub LoadZaaps()

            MyConsole.StartLoading("Loading zaaps from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM zaaps"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewZaap As New ZaapTemplate

                    NewZaap.Id = Result.GetInt32("id")
                    NewZaap.MapID = Result("mapid")
                    NewZaap.CellID = Result("cellid")
                    NewZaap.HasMonster = Result("hasMonster")

                    Dim destinations As String() = Result.GetString("destIds").Split(";")

                    For Each destination As String In destinations
                        If destination <> "" Then
                            NewZaap.DestinationIds.Add(CInt(destination))
                        End If
                    Next

                    ListOfZaaps.Add(NewZaap.MapID, NewZaap)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListOfZaaps.Count & "@' zaaps loaded from database")

        End Sub

#Region "Functions"

        Private Shared Function FormatPacket(ByVal Packet As String) As Integer
            Dim MyZaapID As String = Packet.Substring(2)
            MyZaapID = MyZaapID.Replace("|", "")
            Return MyZaapID
        End Function

        'Private Shared Function GetCell(ByVal MapID As Integer) As Integer
        '    Dim DefaultCell As Integer = 255
        '    For Each MyRealCell As ZaapTemplate In ListOfZaaps
        '        If MyRealCell.MapID = MapID Then
        '            Return MyRealCell.CellID
        '        End If
        '    Next
        '    Return DefaultCell
        'End Function

        'Private Shared Function Exist(ByVal MapID As String) As Boolean
        '    For Each MyZaap As ZaapTemplate In ListOfZaaps
        '        If MyZaap.MapID = MapID Then
        '            Return True
        '        End If
        '    Next
        '    Return False
        'End Function

        Private Shared Function Leave() As String
            Return "WV"
        End Function

#End Region

        'Public Shared Sub SendInfos(ByVal TheClient As Game.GameClient)
        '    Try
        '        Dim AllZaaps As String = ""
        '        For Each Listing As ZaapTemplate In ListOfZaaps
        '            AllZaaps += Listing.MapID & ";" & Listing.CellID & "|"
        '        Next
        '        TheClient.Send("WC" & TheClient.Character.MapId & "|" & AllZaaps)
        '    Catch ex As Exception
        '        Utils.MyConsole.Err(ex.ToString)
        '        Debug.Print(ex.ToString)
        '    End Try
        'End Sub

        'Public Shared Sub OnMove(ByVal Packet As String, ByVal TheClient As Game.GameClient)
        '    Try
        '        If Packet.Contains("NaN") Then
        '            Exit Sub
        '        End If
        '        Dim MyZaap As Integer = FormatPacket(Packet)
        '        TheClient.Character.TeleportTo(MyZaap, GetCell(MyZaap))
        '        TheClient.Send(Leave)
        '    Catch ex As Exception
        '        Utils.MyConsole.Err(ex.ToString)
        '        Debug.Print(ex.ToString)
        '    End Try
        'End Sub

    End Class
End Namespace
