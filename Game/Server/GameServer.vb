Namespace Server
    Public Class GameServer
        Inherits TCPServer

        Private Shared _This As GameServer
        Public Shared Clients As New List(Of Game.GameClient)
        Public Shared WaitingClient As New List(Of Game.LoginInfos)


        Sub New()
            MyBase.New(Utils.Config.GetItem("GAME_PORT"))
        End Sub

        Protected Overrides Sub Server_Accepted(ByVal sender As Object, ByVal Request As AcceptRequest)

            Dim Socket As New BazSocket(Nothing, Request)

            'ClientConnect(Socket.RemoteEP.Address.ToString)
            'If ClientBanned(Socket.RemoteEP.Address.ToString) Then
            'Socket.Disconnect()
            'Exit Sub
            'End If

            If Socket.State = BazSocketState.Connected Then
                Dim Client As New Game.GameClient(Socket)
                Clients.Add(Client)
            End If

        End Sub

        Protected Overrides Sub Server_Listen(ByVal sender As Object, ByVal e As System.EventArgs)
        End Sub

        Protected Overrides Sub Server_ListenFailed(ByVal sender As Object, ByVal ex As System.Exception)
        End Sub

        Public Shared Function GetInstance() As GameServer
            If _This Is Nothing Then
                _This = New GameServer
            End If
            Return _This
        End Function

    End Class
End Namespace