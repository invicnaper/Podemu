Imports System.Text

Namespace Game
    Public MustInherit Class AbstractClient

        Private WithEvents m_Socket As Server.BazSocket

        Protected Event PacketReceived(ByVal Packet As String)
        Protected Event Disconnected()

        Private m_Buffer As New StringBuilder
        Private m_TempIp As String = ""
        Private m_AccountName As String = ""
        Protected m_Closed As Boolean = False

        Public Sub New(ByVal Socket As Server.BazSocket)
            m_Socket = Socket
            m_Socket.AlwaysRaiseClose = True
            m_Socket.AsyncEvent = False
        End Sub

        Public Sub Send(ByVal Message As String)
            If m_Closed Then Exit Sub
            Try
                m_Socket.Send(Message & Chr(0))
            Catch ex As IO.IOException
                m_Closed = True
                m_Socket.Close()
            End Try
        End Sub

        Public Sub Send(ByVal ParamArray Message As String())
            If m_Closed Then Exit Sub
            Try
                m_Socket.Send(String.Concat(Message) & Chr(0))
            Catch ex As IO.IOException
                m_Closed = True
                m_Socket.Close()
            End Try
        End Sub

        Public Sub Disconnect()
            m_Socket.Close()
        End Sub

        Public ReadOnly Property Ip() As String
            Get
                If m_TempIp = "" Then
                    m_TempIp = m_Socket.RemoteEP.Address.ToString
                End If
                Return m_TempIp
            End Get
        End Property

        Public Property AccountName() As String
            Get
                Return m_AccountName
            End Get
            Set(ByVal value As String)
                m_AccountName = Mid(value, 1, 1).ToUpper & Mid(value, 2).ToLower
            End Set
        End Property

        Private Sub Client_Close(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_Socket.Closed
            If m_Closed Then Exit Sub
            m_Closed = True
            RaiseEvent Disconnected()
        End Sub

        Private Sub Client_DataArrival(ByVal sender As Object, ByVal data() As Byte) Handles m_Socket.DataArrival

            If m_Closed Then Exit Sub
            Dim Packet As String = System.Text.Encoding.Default.GetString(data)
            Packet = Packet.Replace(Chr(10), "")

            For i As Integer = 0 To Packet.Length - 1
                If (Packet(i) <> Chr(0)) Then
                    m_Buffer.Append(Packet(i))
                    If m_Buffer.Length > 1024 Then
                        Disconnect()
                        Exit Sub
                    End If
                Else
                    RaiseEvent PacketReceived(m_Buffer.ToString())
                    m_Buffer.Clear()
                End If
            Next

        End Sub

    End Class
End Namespace