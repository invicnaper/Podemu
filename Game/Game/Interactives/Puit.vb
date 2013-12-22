Imports Podemu.Utils

Namespace Game.Interactives
    Public Class Puit : Inherits InteractiveObject

        Private ReloadTimer As Timers.Timer

        Public Sub New(ByVal Map As World.Map, ByVal CellId As Integer)

            MyBase.New(Map, CellId, New List(Of Integer)(New Integer() {102}))

            ReloadTimer = New Timers.Timer(5000)
            AddHandler ReloadTimer.Elapsed, AddressOf Reload

            FrameId = 5
        End Sub

        Public Overrides Sub Use(ByVal Client As GameClient, ByVal SkillId As Integer)
            If SkillId = 102 Then
                Puiser(Client)
            End If
        End Sub

        Public Sub Puiser(ByVal Client As GameClient)

            If Active Then
                ChangeFrame(3, True)

                Dim nbEau As Integer = Basic.Rand(1, 6)

                Dim eau = ItemsHandler.GetItemTemplate(311).GenerateItem(nbEau)

                Client.Character.Items.AddItem(Client, eau)

                Threading.Thread.Sleep(750)

                ShowNumber(Client, nbEau)

                ReloadTimer.Start()
            End If

        End Sub

        Private Sub Reload(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs)
            ReloadTimer.Stop()
            ChangeFrame(5, False)
        End Sub

    End Class
End Namespace