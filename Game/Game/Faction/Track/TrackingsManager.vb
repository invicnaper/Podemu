﻿Imports Podemu.World
Namespace Game
    Public Class TrackingsManager

        Public Shared Sub _Get(ByVal TheClient As Game.GameClient)
            Try
                If TheClient.Character.Player.Alignment.Id = 0 Then
                    TheClient.SendNormalMessage("Vous etes neutre , action impossible !")
                    Exit Sub
                End If
                Dim MyNumberSplit As Integer
                Dim MyAllPlayers As String = ""
                For Each MyPlayers As GameClient In Players.GetPlayers
                    If Not MyPlayers.Character.Name = TheClient.Character.Name Then
                        If Not MyPlayers.Character.Player.Alignment.Id = TheClient.Character.Player.Alignment.Id Then
                            If Not MyPlayers.Character.Player.Alignment.Id = 0 Then
                                If Not MyPlayers.Character.Player.Alignment.Enabled = False And MyPlayers.Character.Player.Level < TheClient.Character.Player.Level + 20 And MyPlayers.Character.Player.Level < TheClient.Character.Player.Level - 20 Then
                                    MyAllPlayers = (MyAllPlayers & "|" & MyPlayers.Character.Name)
                                    MyNumberSplit += 1
                                End If
                            End If
                        End If
                    End If
                Next
                Dim MySelectCible As String
                Dim MyTraque() As String = MyAllPlayers.Split("|")
                Dim NombreAleatoire As Integer
                Randomize()
                NombreAleatoire = CInt(Rnd() * MyNumberSplit)
                MySelectCible = MyTraque(NombreAleatoire)
                If MySelectCible = "" Then
                    TheClient.SendNormalMessage("Aucune cible de votre niveau n'est disponible en ce moment !")
                    Exit Sub
                End If
                Dim MyTracked As Game.GameClient = World.Players.GetCharacter(MySelectCible)
                TheClient.SendNormalMessage("Votre traque est : <b>" & MyTracked.Character.Name & "</b> ( Level <b>" & MyTracked.Character.Player.Level & "</b> ) , faite <b>.search</b>")
                TheClient.Character.Player.Alignment.TrackingClient = MyTracked.Character.Name
                For i As Integer = 1 To 5
                    Admin.AddItem(TheClient, 7400)
                Next
                MyTracked.SendNormalMessage("Vous etes traquer par : <b>" & TheClient.Character.Name & "</b>")
                'TheClient.SendNormalMessage("Vous obtener <b>x5 Parchemin Lie</b> !")
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

        Public Shared Sub _Search(ByVal TheClient As Game.GameClient)
            Try
                If TheClient.Character.Player.Alignment.TrackingClient = "" Then
                    TheClient.SendNormalMessage("Vous n'avez pas de <b>traque</b> , <b>faite .traque</b> !")
                    Exit Sub
                End If
                Dim MyTracked As Game.GameClient = World.Players.GetCharacter(TheClient.Character.Player.Alignment.TrackingClient)
                TheClient.Character.TeleportTo(MyTracked.Character.MapId, MyTracked.Character.MapCell)
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

    End Class
End Namespace
