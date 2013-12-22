Namespace Game
    Public Class GamePing

        Public LastResponse As Long = 0
        Private LastPing As Long = 0
        Public PingNumber As Integer = 0

        Private ReadOnly Property CanPing() As Boolean
            Get
                Return (LastPing = 0 OrElse Environment.TickCount - LastPing > 10000)
            End Get
        End Property

        Private ReadOnly Property NeedPing() As Boolean
            Get
                Return LastResponse = 0 OrElse Environment.TickCount - LastResponse > 10000
            End Get
        End Property

        Public Sub Ping(ByVal Client As GameClient)
            Try
                If CanPing AndAlso NeedPing Then
                    If PingNumber >= 5 Then
                        Client.Disconnect()
                    Else
                        LastPing = Environment.TickCount
                        Client.Send("Bp")
                        PingNumber += 1
                    End If
                End If
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

    End Class
End Namespace