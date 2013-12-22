Imports Podemu.Game
Imports Podemu.World.Players
Imports Podemu.Utils
Imports MySql.Data.MySqlClient

Namespace World
    Public Class Save

        Private Shared WithEvents SaveTimer As New Timers.Timer
        Private Shared WithEvents CleanTimer As New Timers.Timer
        Private Shared Started As Boolean = False

        Public Shared Sub Save()

            SendMessage("Une sauvegarde du serveur est en cours... Vous pouvez continuer de jouer, mais l'accès au serveur est temporairement bloqué. La connexion sera de nouveau possible d'ici quelques instants. Merci de votre patience.")

            SyncLock Sql.CharactersSync

                Try

                    Dim SQLStart As New MySqlCommand("START TRANSACTION", Sql.Characters)
                    SQLStart.ExecuteNonQuery()

                    'Player
                    For Each Player As GameClient In GetPlayers()
                        Player.Character.Save()
                    Next

                    'Paddock
                    PaddockManager.SavePaddocks()

                    'Merchant
                    For Each Key As KeyValuePair(Of String, Character) In CharactersManager.CharactersList.Where(Function(c) c.Value.State.IsMerchant)
                        Dim Merchant As Merchant = Key.Value
                        Merchant.Save()
                    Next

                    'Empty Merchant
                    For Each Key As KeyValuePair(Of String, Character) In CharactersManager.CharactersList.Where(Function(c) c.Value.State.IsEmptyMerchant)
                        Key.Value.Save()
                        Key.Value.State.IsEmptyMerchant = False
                    Next

                    Dim SQLEnd As New MySqlCommand("COMMIT", Sql.Characters)
                    SQLEnd.ExecuteNonQuery()

                    SendMessage("La sauvegarde du serveur est terminée. L'accès au serveur est de nouveau possible. Merci de votre compréhension.")


                Catch ex As Exception
                    MyConsole.Err(ex.ToString)
                    SendMessage("La sauvegarde du serveur est terminée. L'accès au serveur est de nouveau possible. Merci de votre compréhension.")

                    Dim SQLFailed As New MySqlCommand("ROLLBACK", Sql.Characters)
                    SQLFailed.ExecuteNonQuery()

                End Try

            End SyncLock

        End Sub

        Private Shared Sub SaveAll() Handles SaveTimer.Elapsed

            If Started Then Exit Sub
            Started = True

            Save()

            Started = False

        End Sub

        Private Shared Sub Clean() Handles CleanTimer.Elapsed

            Server.GameServer.GetInstance.ReListen(Utils.Config.GetItem("GAME_PORT"))

            For Each Player As GameClient In Players.GetPlayers
                Player.Ping.Ping(Player)
            Next

        End Sub

        Public Shared Sub StartTimer()
            CleanTimer.Interval = 5000
            CleanTimer.Enabled = True
            SaveTimer.Interval = Config.GetItem("SAVE_TIME")
            SaveTimer.Enabled = True
        End Sub

    End Class
End Namespace