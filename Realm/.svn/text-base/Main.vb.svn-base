Module Main

    Sub Main()

        Try

            Utils.Basic.StartTime = Environment.TickCount

            Utils.MyConsole.DrawTitle()
            Utils.Config.LoadConfig()
            Utils.Sql.OpenConnexion()
            Realm.GameHandler.LoadGameServers()

            While Not Realm.GameHandler.ValidServer
                Threading.Thread.Sleep(1000)
            End While

            Server.LoginServer.GetInstance()

            Utils.MyConsole.Info("Realm loaded @successfully@ in '@" & (Environment.TickCount - Utils.Basic.StartTime) & "@ ms'")

            While True
                Threading.Thread.Sleep(1)
            End While

        Catch ex As Exception
            Utils.MyConsole.Err(ex.Message, True)
        End Try

    End Sub

End Module
