﻿Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class SubAreasHandler

        Public Shared TemplateList As New Dictionary(Of Integer, SubAreaTemplate)(400)

        Public Shared Function Exist(ByVal TemplateId As Integer) As Boolean
            Return TemplateList.ContainsKey(TemplateId)
        End Function

        Public Shared Function GetTemplate(ByVal TemplateId As Integer) As SubAreaTemplate
            Return TemplateList(TemplateId)
        End Function


        Public Shared Sub SetupSubAreas()

            Dim SQLText As String = "SELECT * FROM subareas"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read

                Dim NewSubArea As New SubAreaTemplate

                NewSubArea.Id = Result("id")
                NewSubArea.CanConquest = Result("can_conquest")
                NewSubArea.GroupMinLevel = Result("groupMinLevel")
                NewSubArea.GroupMaxLevel = Result("groupMaxLevel")
                NewSubArea.GroupMinSize = Result("groupMinSize")
                NewSubArea.GroupMaxSize = Result("groupMaxSize")
                NewSubArea.MaxGroup = Result("maxGroup")
                NewSubArea.RespawnTime = Result("respawnTime")
                NewSubArea.IsFixedGroup = Result("fixedGroup")

                Dim Monsters = Result.GetString("monsters").Split(";")
                If Monsters.Length > 0 Then
                    For Each Monster In Monsters.Where(Function(v) v.Length > 0)
                        NewSubArea.MonstersList.Add(MonstersHandler.GetTemplate(Monster))
                    Next
                End If

                'Dim Prism= Result("prism")
                Dim FixedGroup = Result("fixedGroup")

                For Each Map As KeyValuePair(Of Integer, Map) In MapsHandler.ListMaps
                    If Map.Value.Subarea = NewSubArea.Id Then
                        NewSubArea.Maps.Add(Map.Value)
                    End If
                Next

                If Not TemplateList.ContainsKey(NewSubArea.Id) Then
                    TemplateList.Add(NewSubArea.Id, NewSubArea)
                End If

            End While

            Result.Close()

        End Sub

     

    End Class
End Namespace