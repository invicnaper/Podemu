Imports MySql.Data.MySqlClient
Imports Podemu.Utils

Namespace World
    Public Class MapsHandler

        Public Shared ListMaps As New Dictionary(Of Integer, Map)(6000)

        Public Shared Function Exist(ByVal MapID As Integer) As Boolean

            Return ListMaps.ContainsKey(MapID)

        End Function

        Public Shared Function GetSubarea(ByVal MapID As Integer) As Integer

            If Exist(MapID) Then Return ListMaps(MapID).Subarea
            Return Nothing

        End Function

        Public Shared Function GetMap(ByVal MapID As Integer) As Map

            If Exist(MapID) Then Return ListMaps(MapID)
            Return Nothing

        End Function

        Public Shared Function GetMapByPos(ByVal PosX As Integer, ByVal PosY As Integer) As Map
            For Each Map As Map In MapsHandler.ListMaps.Values()
                If Map.PosX = PosX And Map.PosY = PosY Then
                    Return Map
                End If
            Next

            Return Nothing

        End Function


        Public Shared Sub SetupMaps()

            MyConsole.StartLoading("Loading maps from database...")

            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM maps_data"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                Dim Count As Integer = 0

                While Result.Read

                    Count += 1

                    Dim NewMap As New Map

                    NewMap.Id = Result.GetInt32("ID")
                    NewMap.PosX = Result.GetInt32("PosX")
                    NewMap.PosY = Result.GetInt32("PosY")
                    NewMap.Width = Result.GetInt32("Width")
                    NewMap.Height = Result.GetInt32("Height")
                    NewMap.Subarea = Result.GetInt32("Subarea")
                    NewMap.MapData = Result.GetString("MapData")
                    NewMap.Key = Result.GetString("DecryptKey")
                    NewMap.Time = Result.GetString("CreateTime")
                    NewMap.NeedRegister = Result.GetBoolean("needRegister")

                    If Not ListMaps.ContainsKey(NewMap.Id) Then ListMaps.Add(NewMap.Id, NewMap)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListMaps.Count & "@' maps loaded from database")

            LoadTriggers()

        End Sub

        Private Shared Sub LoadTriggers()

            SyncLock Sql.OthersSync

                MyConsole.StartLoading("Loading triggers from database...")
                Dim Total As Integer = 0

                Dim SQLText As String = "SELECT * FROM maps_triggers"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim MapId As Integer = Result.GetInt32("MapId")

                    If Exist(MapId) Then

                        Dim NewTrigger As New MapTrigger
                        NewTrigger.Cell = Result.GetInt32("CellID")
                        NewTrigger.DestMap = Result.GetInt32("NewMap")
                        NewTrigger.DestCell = Result.GetInt32("NewCell")

                        GetMap(MapId).MapTriggers.Add(NewTrigger)

                        Total += 1
                    End If

                End While

                Result.Close()

                MyConsole.StopLoading()
                MyConsole.Status("'@" & Total & "@' triggers loaded from database")

            End SyncLock

        End Sub

    End Class
End Namespace