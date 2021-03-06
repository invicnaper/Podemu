﻿Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Namespace World
    Public Class PerceptorManager

#Region "List"
        Public Shared ListOfPerceptor As New List(Of Perceptor)
#End Region

#Region "Functions and Action"

        Public Shared Function GetPerceptorOnMap(ByVal MapID As Integer) As Perceptor
            For Each MyPerco As Perceptor In ListOfPerceptor
                If MyPerco.MapID = MapID Then
                    Return MyPerco
                End If
            Next
            Return Nothing
        End Function

        Public Shared Function ExistOnMap(ByVal MapID As Integer) As Boolean
            For Each MyPerco As Perceptor In ListOfPerceptor
                If MyPerco.MapID = MapID Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function GetNewID() As Integer
            Dim MyID As Integer = 1
            For Each AllPerco As Perceptor In ListOfPerceptor
                If AllPerco.ID = MyID Then
                    MyID += 1
                Else
                    Return MyID
                End If
            Next
            Return MyID
        End Function

        Public Shared Sub AddPerceptor(ByVal Client As Game.GameClient)
            Dim MyPerceptor As New Perceptor
            MyPerceptor.ID = GetNewID()
            MyPerceptor.MapID = Client.Character.MapId
            MyPerceptor.CellID = Client.Character.MapCell
            MyPerceptor.Name = 1
            ' MyPerceptor.Creator = Client
            MyPerceptor.Guild = Client.Character.Guild
            Client.Send(Client.Character.PatternPerceptor(MyPerceptor))
            ListOfPerceptor.Add(MyPerceptor)
            SavePerceptor(MyPerceptor)
            Chat.SendGuildMessage(Client, "Un Percepteur a ete poser !")
        End Sub

#End Region

#Region "SQL"

        Public Shared Sub LoadPerceptor()
            MyConsole.StartLoading("Loading perceptor from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM perceptor_data"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewPerceptor As New Perceptor

                    NewPerceptor.ID = Result("id")
                    NewPerceptor.MapID = Result("mapid")
                    NewPerceptor.CellID = Result("cellid")
                    NewPerceptor.Name = 1
                    NewPerceptor.Guild = GuildHandler.GetGuildByID(Result("guild"))

                    ListOfPerceptor.Add(NewPerceptor)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListOfPerceptor.Count & "@' perceptor loaded from database")

        End Sub

        Public Shared Sub SavePerceptor(ByVal MyPerceptor As Perceptor)


            Dim ConMutex As New Threading.Mutex

            Try
                SyncLock Sql.Others
                    Dim CreateString As String = "@id, @mapid, @cellid, @guild"

                    Dim SQLText As String = "INSERT INTO perceptor_data VALUES (" & CreateString & ")"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)
                    Dim P As MySqlParameterCollection = SQLCommand.Parameters

                    P.Add(New MySqlParameter("@id", MyPerceptor.ID))
                    P.Add(New MySqlParameter("@mapid", MyPerceptor.MapID))
                    P.Add(New MySqlParameter("@cellid", MyPerceptor.CellID))
                    P.Add(New MySqlParameter("@guild", MyPerceptor.Guild.GuildID))

                    SQLCommand.ExecuteNonQuery()
                End SyncLock

            Catch ex As Exception

            End Try

        End Sub

#End Region

#Region "Perceptor Processor"

        Public Shared Sub OnDeposit(ByVal Client As Game.GameClient)
            Try
                If Not ExistOnMap(Client.Character.MapId) Then
                    AddPerceptor(Client)
                Else
                    'Mettre le IM
                End If
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

#End Region
#Region "Saving"

        Public Shared Sub SavePerceptor()

            For Each Perceptor As Perceptor In ListOfPerceptor
                Perceptor.Save()
            Next

        End Sub

#End Region


    End Class
End Namespace