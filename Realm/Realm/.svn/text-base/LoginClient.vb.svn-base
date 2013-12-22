Imports System.Text

Namespace Realm
    Public Class LoginClient
        Inherits AbstractClient

        Public State As New LoginState
        Public Infos As LoginInfos

        Public Sub New(ByVal Socket As Server.BazSocket)
            MyBase.New(Socket)
            Utils.MyConsole.Info("New login client (@" & Ip & "@)")
            SendKey()
        End Sub

        Private Sub SendKey()
            State.Key = Utils.Basic.RandomString(32)
            Send("HC" & State.Key)
        End Sub

        Public Sub SendHosts()

            Dim Packet As New StringBuilder("AH")

            Dim First As Boolean = True
            For Each Server As GameServer In GameHandler.GameList
                If First Then
                    First = False
                Else
                    Packet.Append("|")
                End If
                Packet.Append(Server.ToString)
            Next

            Send(Packet.ToString())

        End Sub

        Private Sub ReceivePacket(ByVal Packet As String) Handles Me.PacketReceived
            Dim Parser As New LoginParser(Me)
            Parser.Unpack(Packet)
        End Sub

        Private Sub Closed() Handles Me.Disconnected
            Utils.MyConsole.Info("Login client deleted (@" & Ip & "@)")
            Server.LoginServer.Clients.Remove(Me)
        End Sub

    End Class
End Namespace