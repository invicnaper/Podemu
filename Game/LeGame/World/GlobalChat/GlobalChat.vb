Imports Vemu_gs.Server
Imports Vemu_gs.Utils

Namespace World
    Public Class GlobalChat

        Public Shared WithEvents MyClient As BazSocket

        Public Shared Sub ConnectToServer()
            If Config.GetItem("GLOBALCHAT_ENABLED") Then
                MyConsole.StartLoading("Connection with@ global chat@")
                MyClient = New BazSocket
                MyClient.Connect(Config.GetItem("GLOBALCHAT_IP"), Config.GetItem("GLOBALCHAT_PORT"))
                MyConsole.StopLoading()
                MyConsole.Status("Connected to @global chat@")
            End If
        End Sub

        Public Shared Sub SendGlobalMessage(ByVal Message As String, ByVal Client As Game.GameClient)
            MyClient.Send("GM|" & Client.Character.Name & "|" & Message)
        End Sub

        Public Shared Sub Client_DataArrival(ByVal sender As Object, ByVal data() As Byte) Handles MyClient.DataArrival
            Dim packet As String = System.Text.Encoding.Default.GetString(data)
            Dim TheParsedPacket() As String = packet.Split("|")
            Try

                Select Case TheParsedPacket(0)

                    Case "GLOBAL"
                        Chat.SendGlobalMessage(TheParsedPacket(1), TheParsedPacket(2))

                End Select

            Catch ex As Exception
                Utils.MyConsole.Err(ex.Message)
            End Try
        End Sub

    End Class
End Namespace
