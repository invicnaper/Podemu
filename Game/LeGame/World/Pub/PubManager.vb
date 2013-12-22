Imports Podemu.Game
Imports Podemu.Utils
Imports MySql.Data.MySqlClient

Namespace World
    Public Class PubManager

        Private Shared WithEvents TimerActions As New Timers.Timer
        Public Shared Numero As Integer = 1

        Public Shared Sub start()
            If Not Utils.Config.GetItem("PUB_ACTIVE") Then Exit Sub
            TimerActions.Interval = Utils.Config.GetItem("TEMPS_PUB")
            TimerActions.Enabled = True
            TimerActions.Start()
        End Sub


        Private Shared Sub TimerActions_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles TimerActions.Elapsed
            If Numero > Utils.Config.GetItem("NOMBRE_PUB") Then
                Numero = 1
            End If
            Chat.SendPubMessage(Utils.Config.GetItem("PUB" & Numero))
            Numero += 1
        End Sub

    End Class
End Namespace
