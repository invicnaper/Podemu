Imports Podemu.Game
Imports Podemu.Utils

Namespace World
    Public Class FightsHandler

        Private Shared FightList As New List(Of Fight)

        Public Shared Sub AddFight(ByVal Fight As Fight)
            FightList.Add(Fight)
            Fight.Map.AddFight(Fight)
        End Sub

        Public Shared Sub RemoveFight(ByVal Fight As Fight)
            FightList.Remove(Fight)
            Fight.Map.RemoveFight(Fight)
        End Sub

    End Class
End Namespace