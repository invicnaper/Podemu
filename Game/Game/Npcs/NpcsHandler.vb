﻿Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.Utils.Basic

Namespace Game
    Public Class NpcsHandler

        Private Shared NPCTemplates As New Dictionary(Of Integer, NpcTemplate)
        Private Shared NpcCount As Integer = 0

        Public Shared Function NPCExist(ByVal ID As Integer) As Boolean
            Return NPCTemplates.ContainsKey(ID)
        End Function

        Public Shared Function GetTemplate(ByVal ID As Integer) As NpcTemplate
            Return NPCTemplates(ID)
        End Function

        Private Shared Sub LoadNpcs()

            Dim SQLText As String = "SELECT * FROM npcs_templates"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read

                Dim NewTemplate As New NpcTemplate

                NewTemplate.TemplateID = Result.GetInt32("ID")
                NewTemplate.Name = Result.GetString("Name")
                NewTemplate.Skin = Result.GetInt32("Gfx")
                NewTemplate.Size = Result.GetInt32("Size")
                NewTemplate.Sexe = Result.GetInt32("Sex")
                NewTemplate.Color(0) = Result.GetInt32("Color1")
                NewTemplate.Color(1) = Result.GetInt32("Color2")
                NewTemplate.Color(2) = Result.GetInt32("Color3")
                NewTemplate.ItemsArt = Result.GetString("Items")
                NewTemplate.ArtWork = Result.GetInt32("ArtWork")

                Dim SellingString As String = Result.GetString("SellingList")
                If SellingString <> "" Then
                    Dim SellingArray() As String = SellingString.Split(",")
                    For Each SellingItem As String In SellingArray
                        NewTemplate.SellingList.Add(CInt(SellingItem))
                    Next
                End If

                NewTemplate.SellingList.Sort()

                NPCTemplates.Add(NewTemplate.TemplateID, NewTemplate)

            End While

            Result.Close()

        End Sub

        Private Shared Sub LoadPendingsNpcs()

            Dim SQLText As String = "SELECT * FROM npcs_maps"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read

                Dim NewNPC As New Npc

                NewNPC.TemplateID = Result.GetInt32("templateid")
                Dim mapId As Integer = Result.GetInt32("mapid")
                NewNPC.CellID = Result.GetInt32("cell")
                NewNPC.Direction = Result.GetInt32("dir")

                If World.MapsHandler.Exist(mapId) Then
                    Dim map As World.Map = World.MapsHandler.GetMap(mapId)
                    NewNPC.ID = map.NextID
                    NewNPC.Map = map
                    map.NpcList.Add(NewNPC)
                    NpcCount += 1
                End If

            End While

            Result.Close()

        End Sub

        Public Shared Sub SetupNpcs()

            NPCTemplates.Clear()
            NpcCount = 0

            MyConsole.StartLoading("Loading npcs from database...")

            SyncLock Sql.OthersSync

                LoadNpcs()
                LoadPendingsNpcs()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & NpcCount & "@' npcs and '@" & NPCTemplates.Count & "@' templates loaded from database")

        End Sub

    End Class
End Namespace