Imports Podemu.Utils
Imports Podemu.World

Namespace Game.Interactives
    Public Class Porte_Enclos : Inherits InteractiveObject

        Private Paddock As World.Paddock

        Public Sub New(ByVal Map As World.Map, ByVal CellId As Integer)

            MyBase.New(Map, CellId, New List(Of Integer)(New Integer() {175, 176, 177, 178}))

            Paddock = Map.GetPaddock(CellId)

            If Paddock Is Nothing Then
                MyConsole.Err(String.Format("Paddock {0}/{1} don't match any paddock on DB", Map.Id, CellId), False)
                Paddock = PaddockManager.GuessPaddock(Map)
                If Paddock Is Nothing Then Throw New Exception
            End If

            FrameId = 5
        End Sub

        Public Overrides Sub Use(ByVal Client As GameClient, ByVal SkillId As Integer)
            Select Case SkillId

                Case 175
                    Paddock.BeginTrade(Client)

            End Select
        End Sub



    End Class
End Namespace