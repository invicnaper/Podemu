Imports System.Text
Imports Vemu_rs.Server

Namespace Realm
    Public Class GameServer

        Public Property Id As Integer
        Public Property Ip As String = "127.0.0.1"
        Public Property Port As Integer = 0
        Public Property Accounts As New List(Of String)

        Public ReadOnly Property Activaded
            Get
                Return m_Connexion.State = BazSocketState.Connected
            End Get
        End Property

        Public ReadOnly Property NumPlayer As Integer
            Get
                Return Accounts.Count
            End Get
        End Property

        Private WithEvents m_Connexion As New BazSocket
        Private WithEvents m_Timer As New Timers.Timer
        Private m_Buffer As New StringBuilder
        Private m_SystemPort As Integer = 0

        Public Sub New(ByVal Id As Integer, ByVal Ip As String, ByVal Port As Integer, ByVal SystemPort As Integer)
            Me.Id = Id
            Me.Ip = Ip
            Me.Port = Port
            Me.m_SystemPort = SystemPort
            m_Timer.Interval = 1000
            m_Timer.Enabled = True
            Refresh()
        End Sub

        Public Sub AddTicket(ByVal Ticket As LoginKey)

            Dim Packet As String =
                String.Format("KIWI*{0}*{1}*{2}*{3}*{4}*{5}*{6}*{7}",
                              Ticket.SqlInfos.Id,
                              Ticket.AccountName,
                              Ticket.Key,
                              Ticket.SqlInfos.BaseCharString,
                              Ticket.SqlInfos.GmLevel,
                              Ticket.SqlInfos.Points,
                              Ticket.SqlInfos.SubscriptionDate,
                              Ticket.SqlInfos.Gifts)

            Send(Packet)

        End Sub


        Private Sub Send(ByVal Packet As String)
            m_Connexion.Send(Packet & Chr(0))
        End Sub

        Private Sub Refresh() Handles m_Timer.Elapsed

            If Not Activaded AndAlso Not m_Connexion.State = BazSocketState.Connecting Then
                m_Connexion.Connect(Ip, m_SystemPort)
            End If

        End Sub

        Private Sub Receive(ByVal sender As Object, ByVal data As Byte()) Handles m_Connexion.DataArrival

            Dim Packet As String = System.Text.Encoding.Default.GetString(data)

            For i As Integer = 0 To Packet.Length - 1
                If (Packet(i) <> Chr(0)) Then
                    m_Buffer.Append(Packet(i))
                Else
                    PacketReceived(m_Buffer.ToString())
                    m_Buffer.Clear()
                End If
            Next

        End Sub

        Private Sub PacketReceived(ByVal Packet As String)

            Dim Data() As String = Packet.Split("*")

            Select Case Data(0)

                Case "ANANAS"
                    Utils.MyConsole.Notice(String.Concat("Client '@", Data(1), "@' from server ", Id, " connected"))
                    Accounts.Add(Data(1))

                Case "PECHE"
                    Utils.MyConsole.Notice(String.Concat("Client '@", Data(1), "@' from server ", Id, " disconnected"))
                    Accounts.Remove(Data(1))

                Case "MANGUE"
                    LoginInfos.UpdateGifts(Data(1), Data(2))

            End Select

        End Sub

        Private Sub ConnexionEnabled() Handles m_Connexion.Connected
            Accounts.Clear()
            Server.LoginServer.GetInstance.RefreshAllHosts()
            Utils.MyConsole.Status("Game server @" & Id & "@ connected")
        End Sub

        Private Sub ConnexionClosed() Handles m_Connexion.Closed
            Accounts.Clear()
            Server.LoginServer.GetInstance.RefreshAllHosts()
            Utils.MyConsole.Err("Game server @" & Id & "@ disconnected")
        End Sub

        Private Sub ConnexionFailed() Handles m_Connexion.ConnectionFailed
            Utils.MyConsole.Warn("Game server @" & Id & "@ can't connect, will retry in one second")
        End Sub

        Public Overrides Function ToString() As String
            Return Id & ";" & If(Activaded, 1, 0) & ";" & (75 * Id) & ";" & 1
        End Function

    End Class
End Namespace