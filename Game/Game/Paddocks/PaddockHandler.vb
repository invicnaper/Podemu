Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports System.Linq

Namespace Game
    Public Class PaddockHandler

        Public Shared TemplateList As New Dictionary(Of Integer, PaddockTemplate)

#Region "Getter"

        Public Shared Function HasPaddock(ByVal MapId As Integer) As Boolean
            Return TemplateList.FirstOrDefault(Function(t) t.Value.MapId = MapId).Value IsNot Nothing
        End Function

        Public Shared Function Exist(ByVal TemplateId As Integer) As Boolean
            Return TemplateList.ContainsKey(TemplateId)
        End Function

        Public Shared Function GetTemplate(ByVal TemplateId As Integer) As PaddockTemplate
            Return TemplateList(TemplateId)
        End Function

#End Region

#Region "Loading"

        Public Shared Sub SetupPaddocks()

            Utils.MyConsole.StartLoading("Loading paddocks from database ...")

            SyncLock Sql.OthersSync

                TemplateList.Clear()

                Dim SQLText As String = "SELECT * FROM paddock_templates"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewPaddock As New PaddockTemplate

                    NewPaddock.Id = Result("id")
                    NewPaddock.MapId = Result("map_id")
                    NewPaddock.CellId = Result("cell_id")
                    NewPaddock.IsPublic = Result("is_public")
                    NewPaddock.Price = Result("price")
                    NewPaddock.MountPlace = Result("mount_places")
                    NewPaddock.ItemPlace = Result("item_places")

                    TemplateList.Add(NewPaddock.Id, NewPaddock)

                End While

                Result.Close()

            End SyncLock

            Utils.MyConsole.StopLoading()
            Utils.MyConsole.Status("'@" & TemplateList.Count & "@' paddocks loaded from database")

        End Sub

#End Region

    End Class
End Namespace