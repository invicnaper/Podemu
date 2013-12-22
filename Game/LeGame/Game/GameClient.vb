﻿Namespace Game
    Public Class GameClient
        Inherits AbstractClient

        Public State As New GameState
        Public Infos As LoginInfos
        Public CharacterList As New Dictionary(Of Integer, Character)
        Public Character As Character
        Public Spam As New GameSpam
        Public Ping As New GamePing

        Public Lock As New Object
        Friend map As Object
        Friend mapID As Object

        Public Sub New(ByVal Socket As Server.BazSocket)
            MyBase.New(Socket)
            Send("HG")
        End Sub

        Private Sub ReceivePacket(ByVal Packet As String) Handles Me.PacketReceived
            Ping.LastResponse = Environment.TickCount
            Ping.PingNumber = 0
            Dim Parser As New GameParser(Me)
            Parser.Unpack(Packet)
        End Sub

        Private Sub Closed() Handles Me.Disconnected

            Server.GameServer.Clients.Remove(Me)

            If AccountName <> "" AndAlso Not Server.RealmLink.RealmSocket Is Nothing Then
                Server.RealmLink.RealmSocket.Send("PECHE " & AccountName)
            End If

            If State.State = GameState.WaitingPacket.Basic Then

                Try
                    Character.Save()
                Catch ex As Exception
                    Utils.MyConsole.Err("Can't save character '@" & Character.Name & "@' : " & ex.Message)
                End Try

                If (Not Character.State.InBattle) AndAlso World.MapsHandler.Exist(Character.MapId) Then
                    Character.GetMap.DelCharacter(Me)
                End If

                If Character.State.InBattle Then
                    Character.State.GetFighter.IsDeco = True
                    Character.State.GetFight.SendMessage(Character.Name & " viens de se déconnecter")
                End If

                If Character.State.InParty Then
                    Character.State.GetParty.DelCharacter(Me)
                End If

            End If

        End Sub

        Public Sub SendMessage(ByVal Message As String)
            Send("cs<font color=""" & Utils.Config.GetItem("MSG_COLOR") & """>" & Utils.Encoding.Utf8Encoder(Message) & "</font>")
        End Sub

        Public Sub SendNormalMessage(ByVal Message As String)
            Send("cs<font color=""009a13"">" & Utils.Encoding.Utf8Encoder(Message) & "</font>")
        End Sub

        Public Sub SendGlobalMessage(ByVal Message As String)
            Send("cs<font color=""3333FF"">" & Utils.Encoding.Utf8Encoder(Message) & "</font>")
        End Sub
        Public Sub SendAideMessage(ByVal Message As String)
            Send("cs<font color=""3333FF"">" & Utils.Encoding.Utf8Encoder(Message) & "</font>")
        End Sub
        Public Sub SendPublMessage(ByVal Message As String)
            Send("cs<font color=""3333FF"">" & Utils.Encoding.Utf8Encoder(Message) & "</font>")
        End Sub

        Public Sub SendGuildMessage(ByVal Message As String)
            Send("cs<font color=""663399"">" & Utils.Encoding.Utf8Encoder(Message) & "</font>")
        End Sub

        Public Sub ConsoleMessage(ByVal Message As String, Optional ByVal bColor As Integer = 2)
            Send("BAT" & bColor & Message)
        End Sub

        Sub GetMap(ByVal p1 As Object)
            Throw New NotImplementedException
        End Sub

    End Class
End Namespace