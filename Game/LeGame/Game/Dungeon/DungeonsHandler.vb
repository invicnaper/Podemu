Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class DungeonsHandler

        Public Shared TemplateList As New Dictionary(Of Integer, DungeonTemplate)(50)

        Public Shared Function Exist(ByVal TemplateId As Integer) As Boolean
            Return TemplateList.ContainsKey(TemplateId)
        End Function

        Public Shared Function IsDungeon(ByVal Map As Map) As Boolean
            Return TemplateList.FirstOrDefault(Function(t) t.Value.Rooms.Contains(Map)).Value IsNot Nothing
        End Function

        Public Shared Function GetTemplate(ByVal Map As Map) As DungeonTemplate
            Return TemplateList.First(Function(t) t.Value.Rooms.Contains(Map)).Value
        End Function

        Public Shared Function GetTemplate(ByVal TemplateId As Integer) As DungeonTemplate
            Return TemplateList(TemplateId)
        End Function

        Public Shared Sub SetupDungeons()

            Dim SQLText As String = "SELECT * FROM dungeons"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read

                Dim NewDungeon As New DungeonTemplate

                NewDungeon.Id = Result("id")

                Dim Rooms = Result.GetString("rooms").Split(";")
                If Rooms.Length > 0 Then
                    For Each Room In Rooms
                        Dim map = MapsHandler.GetMap(Room)
                        NewDungeon.Rooms.Add(map)
                    Next
                End If

                If Not TemplateList.ContainsKey(NewDungeon.Id) Then
                    TemplateList.Add(NewDungeon.Id, NewDungeon)
                End If

            End While

            Result.Close()

        End Sub

    End Class
End Namespace