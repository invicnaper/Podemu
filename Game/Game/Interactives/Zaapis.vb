Imports System.Text
Imports Podemu.Utils

Namespace Game.Interactives
    Public Class Zaapis : Inherits InteractiveObject

        Private ZaapisTemplate As ZaapisTemplate

        Public Sub New(ByVal Map As World.Map, ByVal CellId As Integer)

            MyBase.New(Map, CellId, New List(Of Integer)(New Integer() {157}))

            ZaapisTemplate = ZaapisManager.GetTemplate(Map.Id, CellId)

            If ZaapisTemplate Is Nothing Then
                MyConsole.Err(String.Format("Zaap {0}/{1} don't match any zaap on DB", Map.Id, CellId), False)
                ZaapisTemplate = ZaapisManager.GuessTemplate(Map.Id)
                If ZaapisTemplate Is Nothing Then Throw New Exception
            End If

        End Sub

        Public Overrides Sub Use(ByVal Client As GameClient, ByVal SkillId As Integer)

            Select Case SkillId

                Case 157
                    StartUse(Client)

            End Select

        End Sub

        Private Function DestsInfo(ByVal Client As GameClient) As String

            Dim pattern As New StringBuilder
            Dim price As Integer = IIf(ZaapisTemplate.Zone = Client.Character.Player.Alignment.Id, 10, 20)

            For Each dest As Integer In ZaapisTemplate.DestinationIds
                Dim temp As ZaapisTemplate = ZaapisManager.GetTemplate(dest)
                If temp IsNot Nothing Then pattern.Append("|" & temp.MapID & ";" & price)
            Next

            Return pattern.ToString()
        End Function

        Public Sub StartUse(ByVal Client As GameClient)

            If ZaapisTemplate.Zone = Client.Character.Player.Alignment.Id Or Client.Character.Player.Alignment.Id = 0 Then
                Client.Character.State.BeginTrade(Trading.Zaap, Me)
                Client.Send("Wc", Map.Id, DestsInfo(Client))
            Else
                Client.SendMessage("Alignement invalide !")
            End If

        End Sub

        Public Sub Teleport(ByVal Client As GameClient, ByVal DestId As Integer)

            If ZaapisTemplate.Zone = Client.Character.Player.Alignment.Id Or Client.Character.Player.Alignment.Id = 0 Then
                Dim Dests = ZaapisTemplate.DestinationIds

                If Dests.Contains(DestId) Then
                    Dim template As ZaapisTemplate = ZaapisManager.GetTemplate(DestId)
                    Dim map1 As World.Map = ZaapisTemplate.GetMap
                    Dim map2 As World.Map = template.GetMap
                    Dim price As Integer = IIf(ZaapisTemplate.Zone = Client.Character.Player.Alignment.Id, 10, 20)

                    If Client.Character.Player.Kamas < price Then
                        Client.SendNormalMessage("Vous n'avez pas assez de kamas pour voyager.")
                        Exit Sub
                    End If

                    Client.Character.Player.Kamas -= price
                    Client.SendNormalMessage("Vous avez perdu " & price & " kamas.")
                    Client.Character.TeleportTo(template.MapID, Cells.NearerCell(map2, template.CellID)) ', template.CellID)
                    'Cells.NearerCell(World.MapsHandler.GetMap(template.MapID))
                    'Client.Character.State.EndTrade()
                    'Client.Send("Wv")
                    StopUse(Client)
                    Client.Character.SendAccountStats()
                Else
                    Client.Send("WUE")
                End If
            Else
                Client.SendMessage("Alignement incorrect !")
            End If

        End Sub

        Public Sub StopUse(ByVal Client As GameClient)

            Client.Character.State.EndTrade()
            Client.Send("Wv")

        End Sub

    End Class

End Namespace