Namespace Server
    Public MustInherit Class TCPServer

        Public Shared BannedClients As New Dictionary(Of String, Integer)
        Protected WithEvents m_Server As Server.BazSocket

        Public Sub New(ByVal Port As Integer)

            m_Server = New BazSocket()
            m_Server.AlwaysRaiseClose = True
            m_Server.AsyncEvent = False

            m_Server.Listen("0.0.0.0", Port)

        End Sub

        Public Sub ReListen(ByVal Port As Integer)

            If m_Server.State = BazSocketState.Disconnected Then

                m_Server.Close()
                m_Server = New BazSocket()
                m_Server.AlwaysRaiseClose = True
                m_Server.AsyncEvent = False

                m_Server.Listen("0.0.0.0", Port)

            End If

        End Sub

        Public Shared Sub ClientConnect(ByVal Ip As String)
            If Not BannedClients.ContainsKey(Ip) Then
                BannedClients.Add(Ip, 1)
            Else
                BannedClients(Ip) += 1
            End If
        End Sub

        Public Shared Function ClientBanned(ByVal Ip As String) As Boolean
            Return (BannedClients(Ip) >= 15)
        End Function

        Public Shared Sub ClientDestroy(ByVal Ip As String)
            BannedClients.Remove(Ip)
        End Sub

        Protected MustOverride Sub Server_Accepted(ByVal sender As System.Object, ByVal Request As AcceptRequest) Handles m_Server.Accepted
        Protected MustOverride Sub Server_Listen(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_Server.Listening
        Protected MustOverride Sub Server_ListenFailed(ByVal sender As Object, ByVal ex As System.Exception) Handles m_Server.ListenFailed

    End Class
End Namespace
