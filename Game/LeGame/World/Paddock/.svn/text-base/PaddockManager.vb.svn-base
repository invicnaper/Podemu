Imports Vemu_gs.Game
Imports Vemu_gs.Utils
Imports MySql.Data.MySqlClient
Namespace World
    Public Class PaddockManager

        Public Shared PaddockList As New List(Of Paddock)

#Region "Getters"

        Public Shared Function ExistOnMap(ByVal MapID As Int32) As Boolean
            For Each MyPark As Paddock In PaddockList
                If MyPark.Template.MapId = MapID Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function PaddocksOnMap(ByVal MapID As Int32) As IEnumerable(Of Paddock)
            Return PaddockList.Where(Function(p) p.Template.MapId = MapID)
        End Function

        Public Shared Function GuessPaddock(ByVal Map As Map) As Paddock
            Return PaddockList.FirstOrDefault(Function(z) z.Map Is Map)
        End Function

        Public Shared Function PaddockOnMapAndCell(ByVal MapID As Int32, ByVal CellId As Integer) As Paddock
            Return PaddockList.FirstOrDefault(Function(p) p.Template.MapId = MapID AndAlso p.Template.CellId = CellId)
        End Function

        Public Shared Function ContainsTemplate(ByVal template As PaddockTemplate) As Boolean
            Return PaddockList.FirstOrDefault(Function(p) p.Template.Id = template.Id) IsNot Nothing
        End Function

#End Region

#Region "Loading"

        Public Shared Sub LoadPaddocks()
            MyConsole.StartLoading("Loading paddocks from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM paddocks"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim Template As PaddockTemplate = PaddockHandler.GetTemplate(Result("id"))
                    Dim GuildId As Integer = Result("guild_id")
                    Dim Guild = If(guildId = 0, Nothing, GuildHandler.GetGuildByID(guildId))
                    Dim Price = Result("price")
                    Dim Mounts As New List(Of Mount)
                    Dim Items As New List(Of Int32)

                    If (Result.GetString("items").Length <> 0) Then
                        Items.AddRange(Result.GetString("items").Split("*").Select(Function(m As String) CInt(m)))
                    End If

                    If (Result.GetString("mounts").Length <> 0) Then
                        Mounts.AddRange(Result.GetString("mounts").Split("~").Select(Function(m As String) New Mount(m)))
                    End If

                    Dim pMap = MapsHandler.GetMap(Template.MapId)
                    If (pMap IsNot Nothing) Then

                        Dim paddock As New Paddock(Template, pMap, Mounts, Items, Price, Guild)
                        pMap.Paddocks.Add(paddock)
                        PaddockList.Add(paddock)
                    End If

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & PaddockList.Count & "@' paddocks loaded from database")

            CompleteWithPaddockTemplate()

        End Sub

        Private Shared Sub CompleteWithPaddockTemplate()

            For Each Paddock As KeyValuePair(Of Integer, PaddockTemplate) In Game.PaddockHandler.TemplateList
                If Not ContainsTemplate(Paddock.Value) Then

                    Dim Template = Paddock.Value

                    Dim pMap = MapsHandler.GetMap(Template.MapId)
                    If (pMap IsNot Nothing) Then

                        Dim NewPaddock As New Paddock(Template, pMap, New List(Of Mount), New List(Of Integer), 0, Nothing)
                        pMap.Paddocks.Add(NewPaddock)
                        PaddockList.Add(NewPaddock)
                    End If

                End If
            Next

        End Sub


#End Region

#Region "Saving"

        Public Shared Sub SavePaddocks()

            For Each Paddock As Paddock In PaddockList
                Paddock.Save()
            Next

        End Sub

#End Region

    End Class
End Namespace
