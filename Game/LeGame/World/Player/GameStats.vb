﻿Namespace World
    Public Class GameStats

        Public Enum GameLeftStat

            Uptime = 1
            PlayersConnected = 2
            NumberErrors = 3
            faq = 4

        End Enum

        Public Enum GameRightStat

            CharactersWon = 1

        End Enum

        Private Shared LeftValues(7) As Long
        Private Shared RightValues(7) As Long

        Private Shared Sub UpdateTask()

            While True

                Threading.Thread.Sleep(500)

                Try

                    SetStat(GameLeftStat.Uptime, Utils.Basic.GetUptime)
                    SetStat(GameLeftStat.PlayersConnected, Players.GetPlayerCount)
                    SetStat(GameLeftStat.NumberErrors, Utils.MyConsole.ErrorCount)
                    SetStat(GameLeftStat.faq, Utils.Config.GetItem("FAQ_ID"))

                Catch ex As Exception
                End Try

            End While

        End Sub

        Public Shared Sub SetupStats()

            Dim Task As New Threading.Tasks.Task(AddressOf UpdateTask)
            Task.Start()

            Utils.MyConsole.SetName(GameLeftStat.Uptime, "Uptime", False)
            Utils.MyConsole.SetName(GameLeftStat.PlayersConnected, "Players Online", False)
            Utils.MyConsole.SetName(GameRightStat.CharactersWon, "Characters Earned", True)
            Utils.MyConsole.SetName(GameLeftStat.NumberErrors, "Errors Count", False)
            Utils.MyConsole.SetName(GameLeftStat.faq, "Faq", False)
        End Sub

        Public Shared Sub AddToStats(ByVal Stat As GameRightStat, ByVal Value As Long)

            RightValues(Stat) += Value
            Utils.MyConsole.SetProperty(Stat, RightValues(Stat), True)

        End Sub

        Public Shared Sub SetStat(ByVal Stat As GameLeftStat, ByVal Value As String)

            Utils.MyConsole.SetProperty(Stat, Value, False)

        End Sub

    End Class
End Namespace