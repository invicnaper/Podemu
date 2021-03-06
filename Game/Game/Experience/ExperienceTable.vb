﻿Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.Utils.Basic

Namespace Game

    Public Class ExperienceTable

        Public Shared Levels As New Dictionary(Of Integer, ExperienceFloor)

        Public Shared Function AtLevel(ByVal Level As Integer) As ExperienceFloor

            If Levels.ContainsKey(Level) Then
                Return Levels(Level)
            ElseIf Level = MaxLevel + 1 Then
                Return New ExperienceFloor(Long.MaxValue)
            Else
                Return New ExperienceFloor
            End If

        End Function

        Public Shared ReadOnly Property MaxLevel() As Integer
            Get
                Return Levels.Last().Key
            End Get
        End Property

        Public Shared Sub SetupExperience()

            MyConsole.StartLoading("Loading experience floors from database...")

            SyncLock Sql.OthersSync

                Levels.Clear()

                Dim SQLText As String = "SELECT * FROM exp_data"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewFloor As New ExperienceFloor

                    Dim Level As Integer = Result("Level")
                    NewFloor.Character = Result("Character")
                    NewFloor.Job = Result("Job")
                    NewFloor.Mount = Result("Mount")
                    NewFloor.Pvp = Result("Pvp")
                    NewFloor.Living = Result("Living")

                    Levels.Add(Level, NewFloor)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()

            MyConsole.Status("'@" & Levels.Count & "@' levels loaded from database")

        End Sub

    End Class
End Namespace