Namespace Game
    Public Class RealmSocket
        Inherits AbstractClient

        Public Lock As New Object

        Public Sub New(ByVal Socket As Server.BazSocket)
            MyBase.New(Socket)
            For Each Player As GameClient In World.Players.GetPlayers
                Send("ANANAS*" & Player.AccountName)
            Next
        End Sub

        Private Sub ReceivePacket(ByVal Packet As String) Handles Me.PacketReceived

            Try

                Dim Data() As String = Packet.Split("*")

                Select Case Data(0)

                    Case "KIWI"
                        World.LoginKeys.KeyList.Add(
                            New LoginKey(Data(2), Data(3),
                            New LoginInfos(Data(1), Data(4), Data(5), Data(6), Data(7), Data(8))
                        ))

                End Select

            Catch ex As Exception

                Utils.MyConsole.Err(ex.ToString, True)

            End Try

        End Sub

        Public Sub DelGift(ByVal Client As GameClient, ByVal GiftId As Integer)
            Send(String.Join("*", "MANGUE", Client.AccountName, GiftId))
        End Sub


        Private Sub Closed() Handles Me.Disconnected

            Server.RealmLink.RealmSocket = Nothing

        End Sub


    End Class
End Namespace