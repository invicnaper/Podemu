Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    <Serializable()> _
    Public Class Quest
        Public id As Integer = 0
        Public Name As String = ""
        Public Steps As New List(Of QuestStep)
        Public NowStep As QuestStep
        Public Conditions As String = ""


        Public Function GetStepByOrdre(ByVal Ordre As Integer) As QuestStep
            For Each S As QuestStep In Steps
                If S.Ordre = Ordre Then
                    Return S
                End If
            Next
            Return Nothing
        End Function

        Public Sub RemplirObjectif(ByVal Client As GameClient, ByVal ObjectifId As Integer)
            NowStep.RemplirObjectif(Client, ObjectifId, Me)
        End Sub

        Public Sub NextStep(ByVal Client As GameClient)
            Try
                Client.Character.Player.Kamas += NowStep.GainKamas
                If Not NowStep.GainKamas = 0 Then
                    Client.SendNormalMessage("Vous avez gagne " & NowStep.GainKamas & " kamas")
                End If
                Client.Character.Player.Exp += NowStep.GainXp
                If Not NowStep.GainXp = 0 Then
                    Client.SendNormalMessage("Vous avez gagne " & NowStep.GainXp & " xp")
                End If
                For Each S As String In NowStep.GainObjects
                    If Not S Is Nothing And Not S Is "" And Not S = 0 Then
                        Admin.AddItem(Client, S, 0)
                        Client.SendNormalMessage("vous avez recu : " & ItemsHandler.GetItemTemplate(S).Name)
                    End If
                Next

            Finally
                Client.Character.SendAccountStats()
            End Try
            If Not NowStep.Ordre >= Steps.Count - 1 Then
                Dim nbre As Integer = NowStep.Ordre
                NowStep = GetStepByOrdre(nbre + 1)
                Client.SendNormalMessage("Quête mise à jour : " & Name)
            Else
                ' Client.Character.Quests.PasseTerminerQuest(Client, Me)
            End If
            Client.Character.Save()
        End Sub

    End Class
End Namespace