Namespace Server
    Public Class UselessServer
        Inherits TCPServer

        Private Shared _This As UselessServer

        Sub New()
            MyBase.New(Utils.Config.GetItem("USELESS_PORT"))
        End Sub

        Protected Overrides Sub Server_Accepted(ByVal sender As Object, ByVal Request As AcceptRequest)
            Dim Socket As New BazSocket(Nothing, Request)
            Socket.Disconnect()
        End Sub

        Protected Overrides Sub Server_Listen(ByVal sender As Object, ByVal e As System.EventArgs)
            Utils.MyConsole.Status("Useless server started on port @" & m_Server.LocalEP.Port)
        End Sub

        Protected Overrides Sub Server_ListenFailed(ByVal sender As Object, ByVal ex As System.Exception)
            Utils.MyConsole.Err("Useless server can't start on port @" & m_Server.LocalEP.Port, True)
        End Sub

        Public Shared Function GetInstance() As UselessServer
            If _This Is Nothing Then
                _This = New UselessServer
            End If
            Return _This
        End Function

    End Class
End Namespace