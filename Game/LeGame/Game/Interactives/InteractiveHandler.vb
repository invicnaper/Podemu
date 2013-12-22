Imports Podemu.Game.Interactives
Imports MySql.Data.MySqlClient
Imports System.Reflection
Imports Podemu.Utils
Imports System.Linq

Namespace Game
    Public Class InteractiveHandler

        Public Shared TemplateList As New Dictionary(Of Integer, Type)(200)

#Region "Getter"

        Public Shared Function IsInteractive(ByVal TemplateId As Integer) As Boolean
            Return TemplateList.ContainsKey(TemplateId)
        End Function

        Public Shared Function GetTemplate(ByVal GfxId As Integer, ByVal Map As World.Map, ByVal CellId As Integer) As InteractiveObject
            If TemplateList(GfxId) IsNot Nothing Then
                Return Activator.CreateInstance(TemplateList(GfxId), Map, CellId)
            End If
            Return Nothing
        End Function

#End Region

#Region "Loading"

        Public Shared Sub SetupInteractives()

            Utils.MyConsole.StartLoading("Loading interactive from database ...")

            SyncLock Sql.OthersSync

                TemplateList.Clear()

                Dim SQLText As String = "SELECT * FROM interactive_objects"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim interactives As String = Result.GetString("gfx_Ids")
                    Dim type As Type = Assembly.GetExecutingAssembly().GetType("Vemu_gs.Game.Interactives." + Result.GetString("name"))

                    For Each interactive As String In interactives.Split(",")

                        If interactive = "" Then Continue For

                        TemplateList.Add(CInt(interactive), type)
                    Next

                End While

                Result.Close()

            End SyncLock

            Utils.MyConsole.StopLoading()
            Utils.MyConsole.Status("'@" & TemplateList.Count & "@' interactives loaded from database")

        End Sub

#End Region

    End Class
End Namespace