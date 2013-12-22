Imports System.Text
Imports Podemu.Utils

Namespace Game.Interactives
    Public Class Zaap : Inherits InteractiveObject

        Private ZaapTemplate As ZaapTemplate

        Public Sub New(ByVal Map As World.Map, ByVal CellId As Integer)

            MyBase.New(Map, CellId, New List(Of Integer)(New Integer() {44, 114}))

            ZaapTemplate = ZaapsHandler.GetTemplate(Map.Id, CellId)

            If ZaapTemplate Is Nothing Then
                MyConsole.Err(String.Format("Zaap {0}/{1} don't match any zaap on DB", Map.Id, CellId), False)
                ZaapTemplate = ZaapsHandler.GuessTemplate(Map.Id)
                If ZaapTemplate Is Nothing Then Throw New Exception
            End If

        End Sub

        Public Overrides Sub Use(ByVal Client As GameClient, ByVal SkillId As Integer)

            Select Case SkillId

                Case 44
                    Save(Client)

                Case 114
                    StartUse(Client)

            End Select

        End Sub

        Private Function AvailableDest(ByVal Client As GameClient) As IEnumerable(Of Integer)
            Return ZaapTemplate.DestinationIds.Where(Function(d) Client.Character.Zaaps.Contains(d))
        End Function

        Private Function DestsInfo(ByVal Client As GameClient) As String

            Dim pattern As New StringBuilder

            For Each dest As Integer In AvailableDest(Client)
                Dim temp As ZaapTemplate = ZaapsHandler.GetTemplate(dest)
                Dim map1 As World.Map = ZaapTemplate.GetMap
                Dim map2 As World.Map = temp.GetMap
                Dim price As Integer = (10 * (Math.Abs(map2.PosX - map1.PosX) + Math.Abs(map2.PosY - map1.PosY) - 1))
                If temp IsNot Nothing Then pattern.Append("|" & temp.MapID & ";" & price)
            Next

            Return pattern.ToString()
        End Function

        Public Sub StartUse(ByVal Client As GameClient)

            'Not known

            If Not Client.Character.Zaaps.Contains(ZaapTemplate.MapID) Then

                Client.Character.Zaaps.Add(ZaapTemplate.MapID)
                Client.SendNormalMessage("Tu viens de mémoriser un nouveau zaap.")

                End If

            Client.Character.State.BeginTrade(Trading.Zaap, Me)
            Client.Send("WC", Map.Id, DestsInfo(Client))

        End Sub

        Public Sub Teleport(ByVal Client As GameClient, ByVal DestId As Integer)

            Dim Dests = AvailableDest(Client)

            If Dests.Contains(DestId) Then
                Dim template As ZaapTemplate = ZaapsHandler.GetTemplate(DestId)
                Dim map1 As World.Map = ZaapTemplate.GetMap
                Dim map2 As World.Map = template.GetMap
                Dim price As Integer = (10 * (Math.Abs(map2.PosX - map1.PosX) + Math.Abs(map2.PosY - map1.PosY) - 1))

                If Client.Character.Player.Kamas < price Then
                    Client.SendNormalMessage("Vous n'avez pas assez de kamas pour voyager.")
                    Exit Sub
                End If

                Client.Character.Player.Kamas -= price
                Client.SendNormalMessage("Vous avez perdu " & price & " kamas.")
                Client.Character.TeleportTo(template.MapID, template.CellID)
                'Cells.NearerCell(World.MapsHandler.GetMap(template.MapID),
                Client.Character.State.EndTrade()
                Client.Send("WV")
                Client.Character.SendAccountStats()
            Else
                Client.Send("WUE")
            End If

        End Sub

        Public Sub Save(ByVal Client As GameClient)

            'Already known
            If Client.Character.Zaaps.Contains(ZaapTemplate.MapID) Then

                Client.Character.SaveMap = ZaapTemplate.MapID
                Client.Character.SaveCell = Client.Character.MapCell
                Client.SendNormalMessage("Position sauvegardé.")

            Else

                Client.Character.Zaaps.Add(ZaapTemplate.MapID)
                Client.Character.SaveMap = ZaapTemplate.MapID
                Client.Character.SaveCell = Client.Character.MapCell
                Client.SendNormalMessage("Tu viens de mémoriser un nouveau zaap.")

            End If

        End Sub

        Public Sub StopUse(ByVal Client As GameClient)

            Client.Character.State.EndTrade()
            Client.Send("WV")

        End Sub

    End Class

End Namespace