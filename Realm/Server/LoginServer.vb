Namespace Server
    Public Class LoginServer
        Inherits TCPServer

        Private Shared _This As LoginServer
        Public Shared Clients As New List(Of Realm.LoginClient)
        Public LastConnection As Integer = 0

        Sub New()
            MyBase.New(Utils.Config.GetItem("LOGIN_PORT"))
        End Sub

        Protected Overrides Sub Server_Accepted(ByVal sender As Object, ByVal Request As AcceptRequest)

            Dim Socket As New BazSocket(Nothing, Request)

            'If LastConnection + 20 > Environment.TickCount Then
            'Socket.Send("M016" & Chr(0))
            'Socket.Close()
            'Exit Sub
            'End If
            'LastConnection = Environment.TickCount

            ClientConnect(Socket.RemoteEP.Address.ToString)
            If ClientBanned(Socket.RemoteEP.Address.ToString) Then
                Socket.Send("M00" & Chr(0))
                Socket.Disconnect()
                Exit Sub
            End If

            Clients.Add(New Realm.LoginClient(Socket))

        End Sub

        Protected Overrides Sub Server_Listen(ByVal sender As Object, ByVal e As System.EventArgs)
            Utils.MyConsole.Status("Login server started on port @" & m_Server.LocalEP.Port)
        End Sub

        Protected Overrides Sub Server_ListenFailed(ByVal sender As Object, ByVal ex As System.Exception)
            Utils.MyConsole.Err("Login server can't start on port @" & m_Server.LocalEP.Port, True)
        End Sub

        Public Sub RefreshAllHosts()

            For Each Client As Realm.LoginClient In Clients.ToArray
                If Client.State.State = Realm.LoginState.WaitingPacket.Basic Then
                    Client.SendHosts()
                End If
            Next

        End Sub

        Public Shared Obj As New Object

        Public Shared Function GetInstance() As LoginServer
            SyncLock Obj
                If _This Is Nothing Then
                    _This = New LoginServer
                End If
                Return _This
            End SyncLock
        End Function

    End Class
End Namespace