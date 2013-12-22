Namespace Server
    Public Class RealmLink
        Inherits TCPServer

        Private Shared _This As RealmLink
        Public Shared RealmSocket As Game.RealmSocket

        Sub New()
            MyBase.New(Utils.Config.GetItem("SYSTEM_PORT"))
        End Sub

        Protected Overrides Sub Server_Accepted(ByVal sender As Object, ByVal Request As AcceptRequest)

            Dim Socket As New BazSocket(Nothing, Request)

            If RealmSocket Is Nothing Then
                RealmSocket = New Game.RealmSocket(Socket)
            Else
                Socket.Close()
            End If

        End Sub

        Protected Overrides Sub Server_Listen(ByVal sender As Object, ByVal e As System.EventArgs)
            Utils.MyConsole.Status("Login link started on port @" & m_Server.LocalEP.Port)
        End Sub

        Protected Overrides Sub Server_ListenFailed(ByVal sender As Object, ByVal ex As System.Exception)
            Utils.MyConsole.Err("Login link can't listen on port @" & m_Server.LocalEP.Port, True)
        End Sub

        Public Shared Function GetInstance() As RealmLink
            If _This Is Nothing Then
                _This = New RealmLink
            End If
            Return _This
        End Function

    End Class
End Namespace